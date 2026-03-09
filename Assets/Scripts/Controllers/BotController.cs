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

    [Range(1f, 10f)]
    [Tooltip("หน่วงเวลาเฉพาะการวางแผนหาของ/สู้รบ (การหลบระเบิดจะไวเท่าเดิม)")]
    [SerializeField] private float _thinkDelayOffset = 1.0f;

    [Range(0.1f, 1f)]
    [SerializeField] private float _aggressiveness = 0.8f;

    [Header("Radar Layers")]
    [SerializeField] private int _itemSearchRadius = 8;
    [SerializeField] private int _entitySearchRadius = 4;
    [SerializeField] private LayerMask _itemLayer;
    [SerializeField] private LayerMask _bombLayer;
    [SerializeField] private LayerMask _entityLayer;

    [Header("Combat Readiness Thresholds")]
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
    private float _nextReflexTime; // 🚀แยกรอบคิดสำหรับการหลบหลีก
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

        // 🚀 1. Reflex: การหลบหลีกภัยคุกคาม (คิดเร็วที่สุดเสมอ ไม่สน Offset ป้องกันบอทยืนเอ๋อตาย)
        if (Time.fixedTime >= _nextReflexTime)
        {
            _nextReflexTime = Time.fixedTime + _thinkInterval;
            ExecuteReflexes();
        }

        // 🚀 2. Planning: การวางแผนหาของ วางระเบิด ล่าศัตรู (คิดช้าลงตามที่ตั้งค่า Offset)
        if (Time.fixedTime >= _nextThinkTime)
        {
            _nextThinkTime = Time.fixedTime + (_thinkInterval * _thinkDelayOffset);
            ExecutePlanning();
        }

        // 🚀 3. Movement: จัดการเดินตามเส้นทาง (อัปเดตทุกเฟรมเพื่อป้องกันการเดินเลยซอย)
        ExecuteMovement();
    }

    // ... (ฟังก์ชัน OnDrawGizmos ปล่อยไว้เหมือนเดิมครับ ไม่ต้องแก้) ...
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

    #region AI Logic (Reflexes & Planning)

    private void ExecuteReflexes()
    {
        if (_sessionData == null) return;

        foreach (var botId in _targetsToDrive)
        {
            if (!_controlledStatsMap.TryGetValue(botId, out var stats) || stats == null || stats.CurrentHp <= 0) continue;
            if (!_botBrains.ContainsKey(botId)) _botBrains[botId] = new BotBrainState();

            var brain = _botBrains[botId];
            Vector2Int myGridPos = WorldToGrid(stats.transform.position);
            int safeRadius = stats.CurrentExplosionRange + 1;

            bool isCurrentlyThreatened = _mapChannel.IsDangerous(myGridPos) || IsThreatenedByBomb(myGridPos, safeRadius);

            if (isCurrentlyThreatened)
            {
                brain.Behavior = BotFullStateMachine.Fleeing;
                bool needNewSafeSpot = brain.SafeTarget.x == -9999 || _mapChannel.IsDangerous(brain.SafeTarget) || IsThreatenedByBomb(brain.SafeTarget, safeRadius) || myGridPos == brain.SafeTarget;
                if (needNewSafeSpot) brain.SafeTarget = ExecuteGetSafeSpot(myGridPos, safeRadius);

                if (brain.SafeTarget.x != -9999)
                {
                    brain.CurrentFullPath = GetEscapePath(myGridPos, brain.SafeTarget);
                }
                brain.TargetItemPos = new Vector2Int(-9999, -9999);
            }
            else if (brain.Behavior == BotFullStateMachine.Fleeing)
            {
                brain.Behavior = BotFullStateMachine.Patrolling;
                brain.SafeTarget = new Vector2Int(-9999, -9999);
                brain.CurrentFullPath.Clear();
            }
        }
    }

    private void ExecutePlanning()
    {
        if (_sessionData == null) return;
        var activePlayers = _sessionData.SelectedPlayers.Where(p => _characterRegistry.GetStats(p)?.CurrentHp > 0).ToList();

        foreach (var botId in _targetsToDrive)
        {
            if (!_controlledStatsMap.TryGetValue(botId, out var stats) || stats == null || stats.CurrentHp <= 0) continue;
            if (!_botBrains.ContainsKey(botId)) continue;

            var brain = _botBrains[botId];

            // 🛑 ถ้ากำลังวิ่งหนีตายอยู่ ไม่ต้องสนใจล่าของหรือโจมตี!
            if (brain.Behavior == BotFullStateMachine.Fleeing) continue;

            Vector2Int myGridPos = WorldToGrid(stats.transform.position);
            _currentPathfindOrigin = myGridPos;
            brain.TargetBombTile = new Vector2Int(-9999, -9999);
            int safeRadius = stats.CurrentExplosionRange + 1;

            // 🎯 PRIORITY 2: ITEM HUNTING
            bool needNewItem = brain.TargetItemPos.x == -9999 || !CheckHasItemAt(brain.TargetItemPos);
            if (needNewItem) brain.TargetItemPos = ExecuteFindNearestObject(myGridPos, _itemLayer, _itemSearchRadius);

            if (brain.TargetItemPos.x != -9999)
            {
                brain.Behavior = BotFullStateMachine.Patrolling;
                brain.CurrentFullPath = GetFullPath(myGridPos, brain.TargetItemPos, false, safeRadius);
                continue;
            }

            bool isPanic = stats.CurrentHp < _panicHpThreshold;
            bool isReadyToFight = (stats.CurrentHp >= _readyHp) || (stats.CurrentExplosionRange >= _readyRange) || (stats.CurrentSpeed >= _readySpeed) || (stats.CurrentBombAmount >= _readyBombCount);

            // 🎯 PRIORITY 2.5: ENTITY HUNTING
            if (isReadyToFight && !isPanic)
            {
                Vector2Int nearestEntity = ExecuteFindNearestObject(myGridPos, _entityLayer, _entitySearchRadius);
                if (nearestEntity.x != -9999)
                {
                    brain.Behavior = BotFullStateMachine.Chasing;
                    brain.CurrentFullPath = GetFullPath(myGridPos, nearestEntity, true, safeRadius);
                    if (brain.CurrentFullPath.Count > 0 && !_mapChannel.IsWalkable(brain.CurrentFullPath[0]))
                    {
                        if (Vector2Int.Distance(myGridPos, brain.CurrentFullPath[0]) <= 1.1f) ExecuteRequestBomb(botId, stats);
                        brain.CurrentFullPath.Clear(); // หยุดเดินชนกำแพง
                    }
                    continue;
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
                brain.CurrentFullPath = GetFullPath(myGridPos, nearestBox, true, safeRadius);
                if (brain.CurrentFullPath.Count > 0 && !_mapChannel.IsWalkable(brain.CurrentFullPath[0]))
                {
                    if (Vector2Int.Distance(myGridPos, brain.CurrentFullPath[0]) <= 1.1f) ExecuteRequestBomb(botId, stats);
                    brain.CurrentFullPath.Clear();
                }
            }
            else if (brain.TargetPlayer != Character.None)
            {
                brain.Behavior = BotFullStateMachine.Chasing;
                float distToTarget = Vector2Int.Distance(myGridPos, brain.TargetGridPos);

                if (distToTarget <= 1.2f) { ExecuteRequestBomb(botId, stats); continue; }

                brain.CurrentFullPath = GetFullPath(myGridPos, brain.TargetGridPos, true, safeRadius);
                if (brain.CurrentFullPath.Count > 0 && !_mapChannel.IsWalkable(brain.CurrentFullPath[0]))
                {
                    if (Vector2Int.Distance(myGridPos, brain.CurrentFullPath[0]) <= 1.1f) ExecuteRequestBomb(botId, stats);
                    brain.CurrentFullPath.Clear();
                }
            }
        }
    }

    #endregion

    #region Movement System (Path Following)

    private void ExecuteMovement()
    {
        foreach (var botId in _targetsToDrive)
        {
            if (!_controlledStatsMap.TryGetValue(botId, out var stats) || stats == null || stats.CurrentHp <= 0) continue;
            var brain = _botBrains[botId];

            if (brain.CurrentFullPath != null && brain.CurrentFullPath.Count > 0)
            {
                Vector2Int nextStep = brain.CurrentFullPath[0];
                Vector2 currentPos = stats.transform.position;
                Vector2 dir = (Vector2)nextStep - currentPos;

                // 🚀 ถ้าระยะห่างน้อยมาก ถือว่าเดินมาถึงจุดกึ่งกลางของช่องนั้นแล้ว ให้ตัด Node ทิ้ง
                if (dir.magnitude < 0.05f)
                {
                    stats.transform.position = (Vector2)nextStep; // Snap เข้ากลางช่องเป๊ะๆ
                    brain.CurrentFullPath.RemoveAt(0); // ลบเป้าหมายนี้ทิ้ง

                    if (brain.CurrentFullPath.Count > 0)
                    {
                        nextStep = brain.CurrentFullPath[0]; // ดึงเป้าหมายถัดไป
                    }
                    else
                    {
                        // หมดทางเดิน หยุดนิ่ง
                        _botInputChannel.RaiseEvent(botId, ActionType.Move, new MoveInputEvent(Vector2.zero));
                        continue;
                    }
                }

                ExecuteRequestMove(botId, stats, nextStep);
            }
            else
            {
                // ถ้าไม่มีเป้าหมายให้เดิน ให้หยุดอยู่กับที่
                _botInputChannel.RaiseEvent(botId, ActionType.Move, new MoveInputEvent(Vector2.zero));
            }
        }
    }

    private void ExecuteRequestMove(Character botId, StatsController stats, Vector2Int targetGrid)
    {
        Vector2 targetWorld = (Vector2)targetGrid;
        Vector2 currentWorld = stats.transform.position;
        Vector2 dir = targetWorld - currentWorld;

        Vector2 finalDir = Vector2.zero;
        float alignThreshold = 0.1f; // ระยะคลาดเคลื่อนที่ยอมรับได้

        // 🚀 [THE MAGIC] ลอจิกการเข้าโค้ง: ถ้าเบี้ยวทั้ง 2 แกน ให้ปรับแกนที่เบี้ยวน้อยกว่าให้ตรงเลนก่อนเลี้ยวเข้าซอย
        if (Mathf.Abs(dir.x) > alignThreshold && Mathf.Abs(dir.y) > alignThreshold)
        {
            if (Mathf.Abs(dir.x) < Mathf.Abs(dir.y)) finalDir.x = Mathf.Sign(dir.x);
            else finalDir.y = Mathf.Sign(dir.y);
        }
        else
        {
            // ถ้าเลนตรงแล้ว ให้ก้าวเดินมุ่งหน้าไปเลย พร้อม Snap ตัวเองให้อยู่ตรงกลางเลนเพื่อป้องกันขูดกำแพง
            if (Mathf.Abs(dir.x) > alignThreshold)
            {
                finalDir.x = Mathf.Sign(dir.x);
                stats.transform.position = new Vector2(currentWorld.x, targetWorld.y); // Lock Y
            }
            else if (Mathf.Abs(dir.y) > alignThreshold)
            {
                finalDir.y = Mathf.Sign(dir.y);
                stats.transform.position = new Vector2(targetWorld.x, currentWorld.y); // Lock X
            }
        }

        _botInputChannel.RaiseEvent(botId, ActionType.Move, new MoveInputEvent(finalDir.normalized));
    }

    #endregion

    #region Radar Helpers
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