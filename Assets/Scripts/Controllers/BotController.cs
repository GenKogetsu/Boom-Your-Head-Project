using UnityEngine;
using System.Collections.Generic;
using Genoverrei.DesignPattern;
using Genoverrei.Libary;
using NaughtyAttributes;
using System.Linq;

public sealed class BotController : MonoBehaviour, IPathfindable
{
    #region Variables
    [Header("Observer & Data")]
    [SerializeField] private MapChannelSO _mapChannel;
    [SerializeField] private GameSessionDataSO _sessionData;
    [SerializeField] private CharacterRegistrySO _characterRegistry;
    [SerializeField] private BotInputChannelSO _botInputChannel;

    [Header("AI Configs")]
    [SerializeField] private float _thinkInterval = 0.12f;
    [Range(0.1f, 1f)]
    [SerializeField] private float _aggressiveness = 0.8f;

    [Header("Radar Layers")]
    [SerializeField] private int _itemSearchRadius = 8;
    [SerializeField] private int _entitySearchRadius = 4; // 🚀 [NEW] แยกระยะตรวจจับรถให้เหลือ 4
    [SerializeField] private LayerMask _itemLayer;
    [SerializeField] private LayerMask _bombLayer;
    [SerializeField] private LayerMask _entityLayer;

    [Header("Combat Readiness Thresholds (Offsets)")]
    [SerializeField] private int _readyHp = 3;
    [SerializeField] private int _readyRange = 3;
    [SerializeField] private float _readySpeed = 5f;
    [SerializeField] private int _readyBombCount = 3;

    [Header("Panic Settings")]
    [SerializeField] private int _panicHpThreshold = 2;

    [Header("Identity (Multi-Drive)")]
    [ReadOnly][SerializeField] private List<Character> _targetsToDrive = new List<Character>();

    private Dictionary<Character, StatsController> _controlledStatsMap = new Dictionary<Character, StatsController>();
    private Dictionary<Character, BotBrainState> _botBrains = new Dictionary<Character, BotBrainState>();

    private float _nextThinkTime;
    private Vector2Int _currentPathfindOrigin;
    #endregion

    public Vector2Int CurrentGridPosition => _currentPathfindOrigin;
    public IMapProvider MapProvider => null;
    Vector2Int IPathfindable.GetNextPath(Vector2Int target) => PathfindAbility<BotController>.Execute(this, target);

    #region Unity Lifecycle

    private void FixedUpdate()
    {
        if (_controlledStatsMap.Count == 0 || _controlledStatsMap.Any(kvp => kvp.Value == null))
        {
            ExecuteRefreshTarget();
            return;
        }

        if (Time.fixedTime < _nextThinkTime) return;
        _nextThinkTime = Time.fixedTime + _thinkInterval;

        ExecuteThink();
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        foreach (var kvp in _controlledStatsMap)
        {
            if (kvp.Value == null || !_botBrains.ContainsKey(kvp.Key)) continue;
            var brain = _botBrains[kvp.Key];

            Color pathColor = Color.yellow;
            if (brain.Behavior == BotFullStateMachine.Fleeing) pathColor = Color.cyan;
            else if (brain.Behavior == BotFullStateMachine.Chasing) pathColor = Color.red;
            else if (brain.TargetItemPos.x != -9999) pathColor = Color.green;

            Gizmos.color = pathColor;
            Vector2 start = (Vector2)WorldToGrid(kvp.Value.transform.position);

            if (brain.CurrentFullPath != null)
            {
                foreach (var node in brain.CurrentFullPath)
                {
                    if (_mapChannel.IsSolid(node) || _mapChannel.IsDestructible(node)) break;
                    Gizmos.DrawLine(start, (Vector2)node);
                    Gizmos.DrawWireSphere((Vector2)node, 0.1f);
                    start = (Vector2)node;
                }
            }

            if (brain.TargetBombTile != new Vector2Int(-9999, -9999))
            {
                Gizmos.color = new Color(1f, 0.5f, 0f, 0.8f);
                Gizmos.DrawWireSphere((Vector3)(Vector2)brain.TargetBombTile, 0.25f);
            }

            if (brain.Behavior == BotFullStateMachine.Fleeing && brain.SafeTarget.x != -9999)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireCube((Vector2)brain.SafeTarget, Vector3.one * 0.7f);
            }

            if (brain.TargetItemPos.x != -9999)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube((Vector2)brain.TargetItemPos, Vector3.one * 0.5f);
            }

            Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
            Gizmos.DrawCube((Vector2)WorldToGrid(kvp.Value.transform.position), Vector3.one * 0.9f);
        }
    }

    #endregion

    private Vector2Int WorldToGrid(Vector2 worldPos) => new Vector2Int(Mathf.RoundToInt(worldPos.x), Mathf.RoundToInt(worldPos.y));

    private void ExecuteThink()
    {
        if (_sessionData == null) return;
        var activePlayers = _sessionData.SelectedPlayers.Where(p => _characterRegistry.GetStats(p)?.CurrentHp > 0).ToList();

        foreach (var botId in _targetsToDrive)
        {
            if (!_controlledStatsMap.TryGetValue(botId, out var stats) || stats == null || stats.CurrentHp <= 0) continue;

            if (!_botBrains.ContainsKey(botId)) _botBrains[botId] = new BotBrainState();
            var brain = _botBrains[botId];

            Vector2Int myGridPos = WorldToGrid(stats.transform.position);
            _currentPathfindOrigin = myGridPos;
            brain.CurrentFullPath.Clear();
            brain.TargetBombTile = new Vector2Int(-9999, -9999);

            int safeRadius = stats.CurrentExplosionRange + 1;
            bool isCurrentlyThreatened = _mapChannel.IsDangerous(myGridPos) || IsThreatenedByBomb(myGridPos, safeRadius);

            // 🎯 PRIORITY 1: DODGE & SURVIVAL
            if (isCurrentlyThreatened)
            {
                brain.Behavior = BotFullStateMachine.Fleeing;
                bool needNewSafeSpot = brain.SafeTarget.x == -9999 || _mapChannel.IsDangerous(brain.SafeTarget) || IsThreatenedByBomb(brain.SafeTarget, safeRadius) || myGridPos == brain.SafeTarget;
                if (needNewSafeSpot) brain.SafeTarget = ExecuteGetSafeSpot(myGridPos, safeRadius);

                if (brain.SafeTarget.x != -9999)
                {
                    brain.CurrentFullPath = GetEscapePath(myGridPos, brain.SafeTarget);
                    ExecuteRequestMove(botId, stats, brain.CurrentFullPath.Count > 0 ? brain.CurrentFullPath[0] : myGridPos);
                }
                continue;
            }

            // 🎯 PRIORITY 2: ITEM HUNTING
            bool needNewItem = brain.TargetItemPos.x == -9999 || !CheckHasItemAt(brain.TargetItemPos);
            // 🚀 [FIX] ส่ง _itemSearchRadius เข้าไปแสกนหาไอเทม
            if (needNewItem) brain.TargetItemPos = ExecuteFindNearestObject(myGridPos, _itemLayer, _itemSearchRadius);

            if (brain.TargetItemPos.x != -9999)
            {
                brain.Behavior = BotFullStateMachine.Patrolling;
                brain.CurrentFullPath = GetFullPath(myGridPos, brain.TargetItemPos, false, safeRadius);
                if (brain.CurrentFullPath.Count > 0) ExecuteRequestMove(botId, stats, brain.CurrentFullPath[0]);
                continue;
            }

            // --- วิเคราะห์ความพร้อมก่อนบวก ---
            bool isPanic = stats.CurrentHp < _panicHpThreshold;
            bool isReadyToFight = (stats.CurrentHp >= _readyHp) || (stats.CurrentExplosionRange >= _readyRange) || (stats.CurrentSpeed >= _readySpeed) || (stats.CurrentBombAmount >= _readyBombCount);

            // 🎯 PRIORITY 2.5: ENTITY HUNTING
            if (isReadyToFight && !isPanic)
            {
                // 🚀 [FIX] ส่ง _entitySearchRadius เข้าไปแสกนหารถแทน! ระยะจะสั้นลงตามที่ตั้งค่า
                Vector2Int nearestEntity = ExecuteFindNearestObject(myGridPos, _entityLayer, _entitySearchRadius);
                if (nearestEntity.x != -9999)
                {
                    brain.Behavior = BotFullStateMachine.Chasing;
                    brain.CurrentFullPath = GetFullPath(myGridPos, nearestEntity, true, safeRadius);
                    if (brain.CurrentFullPath.Count > 0)
                    {
                        Vector2Int nextStep = brain.CurrentFullPath[0];
                        if (!_mapChannel.IsWalkable(nextStep))
                        {
                            if (Vector2Int.Distance(myGridPos, nextStep) <= 1.1f) ExecuteRequestBomb(botId, stats);
                            ExecuteRequestMove(botId, stats, myGridPos);
                        }
                        else { ExecuteRequestMove(botId, stats, nextStep); }
                        continue;
                    }
                }
            }

            // 🎯 PRIORITY 3: PLAYER HUNTING / FARMING
            Vector2Int nearestBox = ExecuteFindNearestBox(myGridPos, 30);
            bool noBoxesLeft = (nearestBox.x == -9999);
            if (noBoxesLeft) isReadyToFight = true; else if (isPanic) isReadyToFight = false;

            if (activePlayers.Count > 0)
            {
                brain.TargetPlayer = activePlayers[_targetsToDrive.IndexOf(botId) % activePlayers.Count];
                brain.TargetGridPos = WorldToGrid(_characterRegistry.GetStats(brain.TargetPlayer).transform.position);
            }

            if (!isReadyToFight && !noBoxesLeft)
            {
                // 🚜 Farming Mode
                brain.CurrentFullPath = GetFullPath(myGridPos, nearestBox, true, safeRadius);
                if (brain.CurrentFullPath.Count > 0)
                {
                    Vector2Int nextStep = brain.CurrentFullPath[0];
                    if (!_mapChannel.IsWalkable(nextStep))
                    {
                        if (Vector2Int.Distance(myGridPos, nextStep) <= 1.1f) ExecuteRequestBomb(botId, stats);
                        ExecuteRequestMove(botId, stats, myGridPos);
                    }
                    else { ExecuteRequestMove(botId, stats, nextStep); }
                }
            }
            else if (brain.TargetPlayer != Character.None)
            {
                // ⚔️ Combat Mode
                brain.Behavior = BotFullStateMachine.Chasing;
                float distToTarget = Vector2Int.Distance(myGridPos, brain.TargetGridPos);
                if (distToTarget <= 1.2f) { ExecuteRequestMove(botId, stats, myGridPos); ExecuteRequestBomb(botId, stats); continue; }

                brain.CurrentFullPath = GetFullPath(myGridPos, brain.TargetGridPos, true, safeRadius);
                if (brain.CurrentFullPath.Count > 0)
                {
                    Vector2Int nextStep = brain.CurrentFullPath[0];
                    if (!_mapChannel.IsWalkable(nextStep))
                    {
                        if (Vector2Int.Distance(myGridPos, nextStep) <= 1.1f) ExecuteRequestBomb(botId, stats);
                        ExecuteRequestMove(botId, stats, myGridPos);
                    }
                    else { ExecuteRequestMove(botId, stats, nextStep); }
                }
            }
        }
    }

    #region Radar Helpers

    // 🚀 [FIX] รับค่า radius เข้ามาเป็นพารามิเตอร์ เพื่อให้ยืดหยุ่นแยกระหว่างไอเทมกับรถได้
    private Vector2Int ExecuteFindNearestObject(Vector2Int start, LayerMask layer, float radius)
    {
        Collider2D[] objects = Physics2D.OverlapCircleAll((Vector2)start, radius, layer);
        Vector2Int bestPos = new Vector2Int(-9999, -9999);
        float minDist = float.MaxValue;

        foreach (var obj in objects)
        {
            if (obj.gameObject == this.gameObject) continue;

            Vector2Int gridPos = WorldToGrid(obj.transform.position);
            if (_mapChannel.IsWalkable(gridPos) || layer == _entityLayer)
            {
                float dist = Vector2Int.Distance(start, gridPos);
                if (dist < minDist) { minDist = dist; bestPos = gridPos; }
            }
        }
        return bestPos;
    }

    private bool CheckHasItemAt(Vector2Int pos) => Physics2D.OverlapCircle((Vector2)pos, 0.3f, _itemLayer) != null;
    private bool CheckHasBombAt(Vector2Int pos) => Physics2D.OverlapCircle((Vector2)pos, 0.3f, _bombLayer) != null;

    #endregion

    #region Navigation & Safety

    private void ExecuteRequestMove(Character botId, StatsController stats, Vector2Int targetGrid)
    {
        Vector2 targetWorld = (Vector2)targetGrid;
        Vector2 currentWorld = stats.transform.position;
        Vector2 dir = targetWorld - currentWorld;

        if (dir.magnitude < 0.1f)
        {
            stats.transform.position = targetWorld;
            _botInputChannel.RaiseEvent(botId, ActionType.Move, new MoveInputEvent(Vector2.zero));
            return;
        }

        Vector2 finalDir = Vector2.zero;
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            if (Mathf.Abs(dir.y) > 0.15f) finalDir.y = Mathf.Sign(dir.y);
            else { finalDir.x = Mathf.Sign(dir.x); stats.transform.position = new Vector2(currentWorld.x, targetWorld.y); }
        }
        else
        {
            if (Mathf.Abs(dir.x) > 0.15f) finalDir.x = Mathf.Sign(dir.x);
            else { finalDir.y = Mathf.Sign(dir.y); stats.transform.position = new Vector2(targetWorld.x, currentWorld.y); }
        }

        _botInputChannel.RaiseEvent(botId, ActionType.Move, new MoveInputEvent(finalDir.normalized));
    }

    private bool IsThreatenedByBomb(Vector2Int pos, int dangerRadius)
    {
        if (CheckHasBombAt(pos)) return true;
        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        foreach (var d in dirs)
        {
            for (int i = 1; i <= dangerRadius; i++)
            {
                Vector2Int checkPos = pos + d * i;
                if (_mapChannel.IsSolid(checkPos) || _mapChannel.IsDestructible(checkPos)) { if (CheckHasBombAt(checkPos)) return true; break; }
                if (CheckHasBombAt(checkPos)) return true;
            }
        }
        for (int x = -1; x <= 1; x++) { for (int y = -1; y <= 1; y++) { if (CheckHasBombAt(pos + new Vector2Int(x, y))) return true; } }
        return false;
    }

    private void ExecuteRequestBomb(Character botId, StatsController stats)
    {
        if (stats.BombsRemaining <= 0) return;
        var brain = _botBrains[botId];
        if (Time.time - brain.LastBombTime < 0.4f) return;
        stats.transform.position = (Vector2)WorldToGrid(stats.transform.position);
        _botInputChannel.RaiseEvent(botId, ActionType.PlaceBomb, null);
        brain.LastBombTime = Time.time;
    }

    private Vector2Int ExecuteFindNearestBox(Vector2Int start, int maxRadius)
    {
        for (int r = 1; r <= maxRadius; r++) { for (int x = -r; x <= r; x++) { for (int y = -r; y <= r; y++) { if (Mathf.Abs(x) != r && Mathf.Abs(y) != r) continue; Vector2Int pos = start + new Vector2Int(x, y); if (_mapChannel.IsDestructible(pos)) return pos; } } }
        return new Vector2Int(-9999, -9999);
    }

    private List<Vector2Int> GetFullPath(Vector2Int start, Vector2Int target, bool ignoreBoxes, int safeRadius)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        Queue<Vector2Int> q = new Queue<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        q.Enqueue(start); cameFrom[start] = start;
        int limit = 400;
        while (q.Count > 0 && limit-- > 0)
        {
            Vector2Int curr = q.Dequeue();
            if (curr == target) break;
            foreach (var d in new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
            {
                Vector2Int next = curr + d;
                if (!cameFrom.ContainsKey(next) && !_mapChannel.IsSolid(next))
                {
                    bool isDanger = _mapChannel.IsDangerous(next) || IsThreatenedByBomb(next, safeRadius);
                    if (!ignoreBoxes && isDanger) continue;
                    if (ignoreBoxes || _mapChannel.IsWalkable(next)) { cameFrom[next] = curr; q.Enqueue(next); }
                }
            }
        }
        if (cameFrom.ContainsKey(target)) { Vector2Int curr = target; while (curr != start) { path.Add(curr); curr = cameFrom[curr]; } path.Reverse(); }
        return path;
    }

    private List<Vector2Int> GetEscapePath(Vector2Int start, Vector2Int target)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        Queue<Vector2Int> q = new Queue<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        q.Enqueue(start); cameFrom[start] = start;
        int limit = 400;
        while (q.Count > 0 && limit-- > 0)
        {
            Vector2Int curr = q.Dequeue();
            if (curr == target) break;
            foreach (var d in new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
            {
                Vector2Int next = curr + d;
                if (!cameFrom.ContainsKey(next) && !_mapChannel.IsSolid(next) && _mapChannel.IsWalkable(next)) { cameFrom[next] = curr; q.Enqueue(next); }
            }
        }
        if (cameFrom.ContainsKey(target)) { Vector2Int curr = target; while (curr != start) { path.Add(curr); curr = cameFrom[curr]; } path.Reverse(); }
        return path;
    }

    private Vector2Int ExecuteGetSafeSpot(Vector2Int origin, int safeRadius)
    {
        Queue<Vector2Int> q = new Queue<Vector2Int>();
        HashSet<Vector2Int> v = new HashSet<Vector2Int>();
        q.Enqueue(origin); v.Add(origin);
        while (q.Count > 0)
        {
            Vector2Int curr = q.Dequeue();
            if (!_mapChannel.IsDangerous(curr) && !IsThreatenedByBomb(curr, safeRadius) && _mapChannel.IsWalkable(curr)) return curr;
            foreach (var d in new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
            {
                Vector2Int next = curr + d;
                if (!v.Contains(next) && !_mapChannel.IsSolid(next) && _mapChannel.IsWalkable(next)) { v.Add(next); q.Enqueue(next); }
            }
        }
        return new Vector2Int(-9999, -9999);
    }

    private void ExecuteRefreshTarget()
    {
        if (_sessionData == null || _characterRegistry == null) return;
        _targetsToDrive = new List<Character>(_sessionData.SelectedBots);
        _controlledStatsMap.Clear();
        foreach (var botId in _targetsToDrive)
        {
            var s = _characterRegistry.GetStats(botId);
            if (s != null) _controlledStatsMap.Add(botId, s);
        }
    }

    private bool ExecuteTryKickBomb(Character botId, StatsController stats, BotBrainState brain)
    {
        Vector2Int myPos = WorldToGrid(stats.transform.position);
        foreach (var dir in new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
        {
            Vector2Int bombPos = myPos + dir;
            if (CheckHasBombAt(bombPos))
            {
                Vector2Int diff = brain.TargetGridPos - bombPos;
                bool isTargetInLine = (dir.x != 0 && diff.y == 0 && Mathf.Sign(diff.x) == Mathf.Sign(dir.x)) || (dir.y != 0 && diff.x == 0 && Mathf.Sign(diff.y) == Mathf.Sign(dir.y));
                if (isTargetInLine) { ExecuteRequestMove(botId, stats, bombPos); return true; }
            }
        }
        return false;
    }
    #endregion
}