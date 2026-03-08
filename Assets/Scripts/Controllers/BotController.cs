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
    [SerializeField] private int _itemSearchRadius = 8;
    [SerializeField] private LayerMask _itemLayer;

    [Header("Combat Readiness Thresholds (Offsets)")]
    [Tooltip("ถ้าเลือดถึงค่านี้ จะเริ่มบวก")]
    [SerializeField] private int _readyHp = 3;
    [Tooltip("ถ้าระยะระเบิดถึงค่านี้ จะเริ่มบวก")]
    [SerializeField] private int _readyRange = 3;
    [Tooltip("ถ้าความเร็วถึงค่านี้ จะเริ่มบวก")]
    [SerializeField] private float _readySpeed = 5f;
    [Tooltip("ถ้าจำนวนระเบิดถึงค่านี้ จะเริ่มบวก")]
    [SerializeField] private int _readyBombCount = 3;

    [Header("Panic Settings")]
    [Tooltip("ถ้าเลือดต่ำกว่าค่านี้ จะเลิกสู้แล้วหนีไปฟาร์มจนกว่าเลือดจะเด้ง")]
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
            brain.TargetItemPos = new Vector2Int(-9999, -9999);

            // 🚀 คำนวณระยะอันตรายจากระเบิด (Stats + 1) 
            int safeRadius = stats.CurrentExplosionRange + 1;

            // 🎯 PRIORITY 1: DODGE & SURVIVAL (หลบรัศมีระเบิดและไฟ)
            if (_mapChannel.IsDangerous(myGridPos) || IsThreatenedByBomb(myGridPos, safeRadius))
            {
                brain.Behavior = BotFullStateMachine.Fleeing;
                brain.CurrentFullPath = ExecuteGetSafePath(myGridPos, safeRadius);

                if (brain.CurrentFullPath.Count > 0)
                {
                    ExecuteRequestMove(botId, stats, brain.CurrentFullPath[0]);
                }
                else
                {
                    ExecuteRequestMove(botId, stats, myGridPos); // ไม่มีทางหนี ยืนรอมิราเคิล
                }
                continue; // หนีตายสำคัญสุด ข้ามสเตปอื่นทั้งหมด
            }

            // 🎯 PRIORITY 2: ITEM HUNTING
            Vector2Int nearestItem = ExecuteFindNearestItem(myGridPos);
            if (nearestItem.x != -9999)
            {
                brain.Behavior = BotFullStateMachine.Patrolling;
                brain.TargetItemPos = nearestItem;
                brain.CurrentFullPath = GetFullPath(myGridPos, brain.TargetItemPos, false, safeRadius);

                if (brain.CurrentFullPath.Count > 0)
                {
                    ExecuteRequestMove(botId, stats, brain.CurrentFullPath[0]);
                    continue;
                }
            }

            // --- วิเคราะห์สถานะ (Combat Readiness) ---
            bool isPanic = stats.CurrentHp < _panicHpThreshold;

            bool isReadyToFight = (stats.CurrentHp >= _readyHp) ||
                                  (stats.CurrentExplosionRange >= _readyRange) ||
                                  (stats.CurrentSpeed >= _readySpeed) ||
                                  (stats.CurrentBombAmount >= _readyBombCount);

            Vector2Int nearestBox = ExecuteFindNearestBox(myGridPos, 30);
            bool noBoxesLeft = (nearestBox.x == -9999);

            if (noBoxesLeft) isReadyToFight = true;
            else if (isPanic) isReadyToFight = false;

            if (activePlayers.Count > 0)
            {
                brain.TargetPlayer = activePlayers[_targetsToDrive.IndexOf(botId) % activePlayers.Count];
                brain.TargetGridPos = WorldToGrid(_characterRegistry.GetStats(brain.TargetPlayer).transform.position);
            }
            else { brain.TargetPlayer = Character.None; }

            // 🎯 PRIORITY 3: ACTION DECISION
            if (!isReadyToFight && !noBoxesLeft)
            {
                // 🚜 Farming Mode
                brain.Behavior = BotFullStateMachine.Patrolling;
                brain.CurrentFullPath = GetFullPath(myGridPos, nearestBox, true, safeRadius);

                if (brain.CurrentFullPath.Count > 0)
                {
                    Vector2Int nextStep = brain.CurrentFullPath[0];
                    if (!_mapChannel.IsWalkable(nextStep))
                    {
                        brain.TargetBombTile = myGridPos;
                        if (Vector2Int.Distance(myGridPos, nextStep) <= 1.1f) ExecuteRequestBomb(botId, stats);
                        ExecuteRequestMove(botId, stats, myGridPos);
                    }
                    else
                    {
                        ExecuteRequestMove(botId, stats, nextStep);
                    }
                }
            }
            else if (brain.TargetPlayer != Character.None)
            {
                // ⚔️ Combat Mode
                brain.Behavior = BotFullStateMachine.Chasing;
                float distToTarget = Vector2Int.Distance(myGridPos, brain.TargetGridPos);

                if (distToTarget <= 1.2f)
                {
                    brain.TargetBombTile = myGridPos;
                    ExecuteRequestMove(botId, stats, myGridPos);
                    ExecuteRequestBomb(botId, stats);
                    continue;
                }

                if (ExecuteTryKickBomb(botId, stats, brain)) continue;

                // วิ่งไล่ล่า (อ้อมระเบิด)
                brain.CurrentFullPath = GetFullPath(myGridPos, brain.TargetGridPos, true, safeRadius);
                if (brain.CurrentFullPath.Count > 0)
                {
                    Vector2Int nextStep = brain.CurrentFullPath[0];
                    if (!_mapChannel.IsWalkable(nextStep))
                    {
                        brain.TargetBombTile = myGridPos;
                        if (Vector2Int.Distance(myGridPos, nextStep) <= 1.1f) ExecuteRequestBomb(botId, stats);
                        ExecuteRequestMove(botId, stats, myGridPos);
                    }
                    else
                    {
                        ExecuteRequestMove(botId, stats, nextStep);

                        if (distToTarget <= 3f && Random.value < _aggressiveness * 0.4f)
                        {
                            brain.TargetBombTile = myGridPos;
                            ExecuteRequestBomb(botId, stats);
                        }
                    }
                }
            }
        }
    }

    #region Bomb & Safety Logic

    // 🚀 [NEW] ระบบเช็คว่าพิกัดนี้อยู่ในรัศมีทำลายล้างของระเบิดหรือไม่
    private bool IsThreatenedByBomb(Vector2Int pos, int dangerRadius)
    {
        if (_mapChannel.HasBomb(pos)) return true;

        // เช็คแนวกางเขน (Crossfire)
        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        foreach (var d in dirs)
        {
            for (int i = 1; i <= dangerRadius; i++)
            {
                Vector2Int checkPos = pos + d * i;
                if (_mapChannel.IsSolid(checkPos)) break; // กำแพงกันระเบิดไว้ ปลอดภัยในทิศนี้
                if (_mapChannel.HasBomb(checkPos)) return true; // มีระเบิดกำลังเล็งมาที่เรา!
            }
        }

        // เช็คช่องรอบตัว 8 ทิศทาง ป้องกันเดินไปสีกับระเบิดใกล้ๆ
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (_mapChannel.HasBomb(pos + new Vector2Int(x, y))) return true;
            }
        }

        return false;
    }

    private void ExecuteRequestBomb(Character botId, StatsController stats)
    {
        if (stats.BombsRemaining <= 0) return;

        var brain = _botBrains[botId];
        if (Time.time - brain.LastBombTime < 0.4f) return;

        Vector2Int gridPos = WorldToGrid(stats.transform.position);
        stats.transform.position = (Vector2)gridPos;

        _botInputChannel.RaiseEvent(botId, ActionType.PlaceBomb, null);
        brain.LastBombTime = Time.time;
    }

    private Vector2Int ExecuteFindNearestBox(Vector2Int start, int maxRadius)
    {
        for (int r = 1; r <= maxRadius; r++)
        {
            for (int x = -r; x <= r; x++)
            {
                for (int y = -r; y <= r; y++)
                {
                    if (Mathf.Abs(x) != r && Mathf.Abs(y) != r) continue;
                    Vector2Int pos = start + new Vector2Int(x, y);
                    if (_mapChannel.IsDestructible(pos)) return pos;
                }
            }
        }
        return new Vector2Int(-9999, -9999);
    }

    private Vector2Int ExecuteFindNearestItem(Vector2Int start)
    {
        Collider2D[] items = Physics2D.OverlapCircleAll((Vector2)start, _itemSearchRadius, _itemLayer);
        Vector2Int bestPos = new Vector2Int(-9999, -9999);
        float minDist = float.MaxValue;

        foreach (var item in items)
        {
            Vector2Int itemGrid = WorldToGrid(item.transform.position);
            if (_mapChannel.IsWalkable(itemGrid))
            {
                float dist = Vector2Int.Distance(start, itemGrid);
                if (dist < minDist)
                {
                    minDist = dist;
                    bestPos = itemGrid;
                }
            }
        }
        return bestPos;
    }

    private bool ExecuteTryKickBomb(Character botId, StatsController stats, BotBrainState brain)
    {
        Vector2Int myPos = WorldToGrid(stats.transform.position);
        foreach (var dir in new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
        {
            Vector2Int bombPos = myPos + dir;
            if (_mapChannel.HasBomb(bombPos))
            {
                if (IsTargetInLine(bombPos, dir, brain.TargetGridPos) && CanBombSlide(bombPos, dir, brain.TargetGridPos))
                {
                    ExecuteRequestMove(botId, stats, bombPos);
                    return true;
                }
            }
        }
        return false;
    }

    private bool IsTargetInLine(Vector2Int b, Vector2Int d, Vector2Int t)
    {
        Vector2Int diff = t - b;
        if (d.x != 0 && diff.y == 0 && Mathf.Sign(diff.x) == Mathf.Sign(d.x)) return true;
        if (d.y != 0 && diff.x == 0 && Mathf.Sign(diff.y) == Mathf.Sign(d.y)) return true;
        return false;
    }

    private bool CanBombSlide(Vector2Int s, Vector2Int d, Vector2Int t)
    {
        Vector2Int c = s + d;
        while (c != t)
        {
            if (_mapChannel.IsSolid(c) || _mapChannel.IsDestructible(c)) return false;
            c += d;
            if (Vector2Int.Distance(s, c) > 15) break;
        }
        return true;
    }

    #endregion

    #region Navigation

    private void ExecuteRequestMove(Character botId, StatsController stats, Vector2Int targetGrid)
    {
        Vector2 targetWorld = (Vector2)targetGrid;
        Vector2 dir = targetWorld - (Vector2)stats.transform.position;

        if (dir.magnitude < 0.1f)
        {
            stats.transform.position = targetWorld;
            _botInputChannel.RaiseEvent(botId, ActionType.Move, new MoveInputEvent(Vector2.zero));
        }
        else
        {
            Vector2 finalDir = dir.normalized;
            if (Mathf.Abs(finalDir.x) > Mathf.Abs(finalDir.y)) finalDir.y = 0; else finalDir.x = 0;
            _botInputChannel.RaiseEvent(botId, ActionType.Move, new MoveInputEvent(finalDir.normalized));
        }
    }

    // 🚀 [FIX] อัปเกรด A* ให้เดินเลี่ยงวงระเบิด
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
                    // 🛑 ห้ามเดินเข้าไปในวงระเบิดเด็ดขาด (ยกเว้นจะสั่งชนกล่อง)
                    bool isDanger = _mapChannel.IsDangerous(next) || IsThreatenedByBomb(next, safeRadius);
                    if (isDanger) continue;

                    if (ignoreBoxes || _mapChannel.IsWalkable(next)) { cameFrom[next] = curr; q.Enqueue(next); }
                }
            }
        }
        if (cameFrom.ContainsKey(target))
        {
            Vector2Int curr = target;
            while (curr != start) { path.Add(curr); curr = cameFrom[curr]; }
            path.Reverse();
        }
        return path;
    }

    // 🚀 [NEW] หาเส้นทางหนีไปสู่จุดที่ปลอดภัยจริงๆ แบบรวดเดียว
    private List<Vector2Int> ExecuteGetSafePath(Vector2Int origin, int safeRadius)
    {
        Queue<Vector2Int> q = new Queue<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();

        q.Enqueue(origin);
        cameFrom[origin] = origin;

        Vector2Int safeSpot = origin;
        bool found = false;

        while (q.Count > 0)
        {
            Vector2Int curr = q.Dequeue();

            // ปลอดภัยจริงๆ คือ ไม่มีไฟ ไม่อยู่ในระยะระเบิด และยืนได้
            if (!_mapChannel.IsDangerous(curr) && !IsThreatenedByBomb(curr, safeRadius) && _mapChannel.IsWalkable(curr))
            {
                safeSpot = curr;
                found = true;
                break;
            }

            foreach (var d in new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
            {
                Vector2Int next = curr + d;
                if (!cameFrom.ContainsKey(next) && !_mapChannel.IsSolid(next) && _mapChannel.IsWalkable(next))
                {
                    cameFrom[next] = curr;
                    q.Enqueue(next);
                }
            }
        }

        List<Vector2Int> path = new List<Vector2Int>();
        if (found)
        {
            Vector2Int curr = safeSpot;
            while (curr != origin)
            {
                path.Add(curr);
                curr = cameFrom[curr];
            }
            path.Reverse();
        }
        return path;
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

    private void ExecuteWanderLogic(Character botId, StatsController stats, BotBrainState brain)
    {
        brain.Behavior = BotFullStateMachine.Patrolling;
        Vector2Int myGrid = WorldToGrid(stats.transform.position);
        brain.WanderTarget = myGrid + new Vector2Int(Random.Range(-3, 4), Random.Range(-3, 4));
        brain.CurrentFullPath = GetFullPath(myGrid, brain.WanderTarget, false, stats.CurrentExplosionRange + 1);
    }
    #endregion
}