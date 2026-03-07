using UnityEngine;
using System.Collections.Generic;
using Genoverrei.DesignPattern;
using Genoverrei.Libary;
using NaughtyAttributes;

/// <summary>
/// <para> (TH) : สมอง AI ที่ใช้ MapChannel ในการตัดสินใจหนีระเบิด ค้นหาผู้เล่น และแบ่งเป้าหมายไม่ให้รุม </para>
/// <para> (EN) : AI Brain using MapChannel for bomb evasion, player tracking, and anti-ganging target distribution. </para>
/// </summary>
public sealed class BotController : MonoBehaviour, IPathfindable
{
    #region Variable

    [Header("Observer & Data")]
    [SerializeField] private MapChannelSO _mapChannel;
    [SerializeField] private GameSessionDataSO _sessionData;
    [SerializeField] private CharacterRegistrySO _characterRegistry;

    [Tooltip("ดักฟังพฤติกรรมผู้เล่นที่ CharacterActionListener โยนมาให้")]
    [SerializeField] private BotInputChannelSO _botInputChannel; // 🚀 ฟังเสียงฝีเท้าผู้เล่น

    [Header("Identity")]
    [SerializeField] private Character _targetToDrive;

    [Header("Settings")]
    [SerializeField] private float _thinkInterval = 0.2f;
    [ReadOnly][SerializeField] private Vector2Int _targetGridPos;

    private StatsController _targetStats;
    private float _nextThinkTime;
    private Vector2Int _currentSafeTarget;
    private bool _isFleeing;

    #endregion //Variable

    public Vector2Int CurrentGridPosition
    {
        get
        {
            if (_targetStats != null)
            {
                return new Vector2Int(
                    Mathf.RoundToInt(_targetStats.transform.position.x),
                    Mathf.RoundToInt(_targetStats.transform.position.y)
                );
            }
            return Vector2Int.zero;
        }
    }

    public IMapProvider MapProvider => null;

    #region Unity Lifecycle

    private void OnEnable()
    {
        if (_botInputChannel != null)
        {
            // 🚀 ฟังว่าศัตรูทำอะไรอยู่ เพื่อเอามาประกอบการคิด (ดักทาง)
            _botInputChannel.OnEnemyActionTriggered += ExecutePredictEnemyMove;
        }
    }

    private void OnDisable()
    {
        if (_botInputChannel != null)
        {
            _botInputChannel.OnEnemyActionTriggered -= ExecutePredictEnemyMove;
        }
    }

    private void Update()
    {
        if (_targetStats == null)
        {
            ExecuteRefreshTarget();
            return;
        }

        if (Time.time < _nextThinkTime) return;

        _nextThinkTime = Time.time + _thinkInterval;
        ExecuteThink();
    }

    #endregion //Unity Lifecycle

    #region AI Logic

    private void ExecuteThink()
    {
        if (_mapChannel == null || _targetStats == null) return;

        Vector2Int currentPos = CurrentGridPosition;
        Vector2Int nextMove;

        // 🚀 1. ค้นหาเป้าหมาย (ผู้เล่น) และกระจายเป้าหมายไม่ให้รุม
        ExecuteFindPlayerTarget();

        // 🔥 2. ถาม Channel: "จุดที่ยืนอยู่ตอนนี้อันตรายไหม?"
        if (_mapChannel.IsDangerous(currentPos))
        {
            if (!_isFleeing || _mapChannel.IsDangerous(_currentSafeTarget))
            {
                _currentSafeTarget = ExecuteFindSafeSpot(currentPos);
                _isFleeing = true;
            }
            nextMove = PathfindAbility<BotController>.Execute(this, _currentSafeTarget);
        }
        else
        {
            // ✅ ปลอดภัยแล้ว ให้เดินล่าเป้าหมายที่ได้รับมอบหมาย
            _isFleeing = false;
            nextMove = PathfindAbility<BotController>.Execute(this, _targetGridPos);
        }

        Vector2 direction = new Vector2(
            nextMove.x - _targetStats.transform.position.x,
            nextMove.y - _targetStats.transform.position.y
        );

        if (EventBus.Instance != null)
        {
            EventBus.Instance.Publish<ISignal>(new CharacterAction(
                _targetToDrive,
                ActionType.Move,
                new MoveInputEvent(direction)
            ));
        }
    }

    /// <summary>
    /// <para> (TH) : ระบบ Anti-Ganging ค้นหาผู้เล่นที่ยังมีชีวิต และใช้ Modulo เพื่อกระจายเป้าหมาย </para>
    /// </summary>
    private void ExecuteFindPlayerTarget()
    {
        if (_sessionData == null || _characterRegistry == null) return;

        List<Character> alivePlayers = new List<Character>();

        // ดึงเฉพาะผู้เล่น (ไม่รวมบอท) ที่ยังไม่ตาย
        for (int i = 0; i < _sessionData.PlayerCount; i++)
        {
            if (i >= _sessionData.SelectedCharacters.Count) continue;

            Character p = _sessionData.SelectedCharacters[i];
            StatsController stats = _characterRegistry.GetCharacter(p);

            // เช็คว่าเลือดมากกว่า 0 คือยังไม่ตาย
            if (stats != null && stats.CurrentHp > 0)
            {
                alivePlayers.Add(p);
            }
        }

        if (alivePlayers.Count == 0) return; // ตายหมดแล้ว ยืนนิ่งๆ

        // 🧠 แบ่งเป้าหมาย: เอา ID ของบอทตัวเอง (int) มา Mod ด้วยจำนวนผู้เล่นที่รอด
        // ทำให้บอทแต่ละตัวเล็งเป้าหมายคนละคน (ถ้ามีผู้เล่นหลายคน)
        int targetIndex = ((int)_targetToDrive) % alivePlayers.Count;
        Character designatedTarget = alivePlayers[targetIndex];

        StatsController targetStats = _characterRegistry.GetCharacter(designatedTarget);
        if (targetStats != null)
        {
            _targetGridPos = new Vector2Int(
                Mathf.RoundToInt(targetStats.transform.position.x),
                Mathf.RoundToInt(targetStats.transform.position.y)
            );
        }
    }

    private Vector2Int ExecuteFindSafeSpot(Vector2Int origin)
    {
        if (_mapChannel == null) return origin;

        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        queue.Enqueue(origin);
        visited.Add(origin);

        int searchLimit = 50;
        while (queue.Count > 0 && searchLimit-- > 0)
        {
            Vector2Int curr = queue.Dequeue();

            if (!_mapChannel.IsDangerous(curr)) return curr;

            foreach (var dir in new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
            {
                Vector2Int next = curr + dir;

                if (_mapChannel.IsWalkable(next) && !visited.Contains(next))
                {
                    queue.Enqueue(next);
                    visited.Add(next);
                }
            }
        }
        return origin;
    }

    private void ExecuteRefreshTarget()
    {
        if (_characterRegistry != null)
        {
            _targetStats = _characterRegistry.GetCharacter(_targetToDrive);
        }
    }

    /// <summary>
    /// <para> (TH) : รับข้อมูลฝีเท้าผู้เล่นเพื่อเอามาทำนายการดักหน้า (Future Feature) </para>
    /// </summary>
    private void ExecutePredictEnemyMove(ISignal signal)
    {
        // 🚀 ตรงนี้พี่สามารถเขียน Logic ให้บอทดักหน้าผู้เล่นได้เลยครับ 
        // เช่น ถ้ารู้ว่าผู้เล่นกำลังกด Move ไปทิศไหน เราอาจจะปรับ _targetGridPos ให้ไปดักรอล่วงหน้า 1 ช่อง
    }

    #endregion //AI Logic

    #region Interface Implementation
    Vector2Int IPathfindable.GetNextPath(Vector2Int target) => PathfindAbility<BotController>.Execute(this, target);
    #endregion
}