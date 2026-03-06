using UnityEngine;
using Genoverrei.DesignPattern;
using Genoverrei.Libary;
using System.Collections.Generic;
using NaughtyAttributes;

/// <summary>
/// <para> Summary : </para>
/// <para> (TH) : สมองบอทที่สอดส่องความปลอดภัยผ่าน MapManager และสั่งการตัวละครผ่าน EventBus </para>
/// <para> (EN) : Bot brain monitoring safety via MapManager and commanding characters via EventBus. </para>
/// </summary>
public sealed class BotController : MonoBehaviour, IPathfindable
{
    #region Variable

    [Header("Session Observer")]
    [SerializeField] private PlayerSessionChannelSO _sessionChannel;

    [Header("Assigned Target")]
    [SerializeField] private Character _targetToDrive;

    private Transform _bodyTransform;

    [Header("Navigation & Logic")]
    [SerializeField] private MapManager _mapManager;

    [ReadOnly]
    [SerializeField] private Vector2Int _targetGridPos;
    [SerializeField] private float _thinkInterval = 0.2f;

    private float _nextThinkTime;
    private Vector2Int _currentSafeTarget;
    private bool _isFleeing;

    #endregion //Variable

    #region IPathfindable Implementation

    public Vector2Int CurrentGridPosition => _bodyTransform == null ? Vector2Int.zero : new Vector2Int(
        Mathf.RoundToInt(_bodyTransform.position.x),
        Mathf.RoundToInt(_bodyTransform.position.y)
    );

    public IMapProvider MapProvider => _mapManager;

    /// <summary>
    /// Explicit Interface Implementation ตามมาตรฐานที่พี่วางไว้
    /// </summary>
    Vector2Int IPathfindable.GetNextPath(Vector2Int target) => ExecuteGetNextPath(target);

    #endregion //IPathfindable Implementation

    #region Unity Lifecycle

    private void OnEnable()
    {
        if (_sessionChannel != null)
            _sessionChannel.OnSessionUpdated += ExecuteRefreshBodyReference;

        ExecuteRefreshBodyReference();
    }

    private void OnDisable()
    {
        if (_sessionChannel != null)
            _sessionChannel.OnSessionUpdated -= ExecuteRefreshBodyReference;
    }

    private void Update()
    {
        if (_bodyTransform == null || Time.time < _nextThinkTime) return;

        _nextThinkTime = Time.time + _thinkInterval;
        ExecuteThink();
    }

    #endregion //Unity Lifecycle

    #region Private Logic

    private void ExecuteRefreshBodyReference()
    {
        if (_sessionChannel == null) return;
        _bodyTransform = _sessionChannel.GetBody(_targetToDrive);
    }

    /// <summary>
    /// <para> (TH) : ประมวลผลความคิดบอท โดยเลือกระหว่างการหนีระเบิดหรือเดินไปเป้าหมาย </para>
    /// </summary>
    private void ExecuteThink()
    {
        Vector2Int currentPos = CurrentGridPosition;
        Vector2Int nextMove;

        // 1. ตรวจสอบว่าพื้นที่ปัจจุบันอันตรายหรือไม่
        if (_mapManager.IsDangerous(currentPos))
        {
            // ถ้าพิกัดที่กำลังหนีไปยังไม่ปลอดภัย หรือยังไม่มีเป้าหมายหนี
            if (!_isFleeing || _mapManager.IsDangerous(_currentSafeTarget))
            {
                _currentSafeTarget = ExecuteFindSafeSpot(currentPos);
                _isFleeing = true;
            }

            nextMove = ((IPathfindable)this).GetNextPath(_currentSafeTarget);
        }
        else
        {
            // พื้นที่ปัจจุบันปลอดภัยแล้ว กลับสู่โหมดเดินตามเป้าหมายปกติ
            _isFleeing = false;
            nextMove = ((IPathfindable)this).GetNextPath(_targetGridPos);
        }

        // 2. คำนวณทิศทางส่งให้ MoveController
        Vector2 direction = new Vector2(
            nextMove.x - _bodyTransform.position.x,
            nextMove.y - _bodyTransform.position.y
        );

        // 3. ยิง Signal ผ่าน EventBus
        EventBus.Instance.Publish(new CharacterAction(
            _targetToDrive,
            ActionType.Move,
            new MoveInputEvent(direction)
        ));
    }

    /// <summary>
    /// <para> (TH) : อัลกอริทึมค้นหาช่องที่ใกล้ที่สุดที่ IsWalkable และ !IsDangerous </para>
    /// </summary>
    private Vector2Int ExecuteFindSafeSpot(Vector2Int origin)
    {
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        queue.Enqueue(origin);
        visited.Add(origin);

        // ค้นหาแบบ BFS วงกว้างออกไปเรื่อยๆ จนกว่าจะเจอที่ปลอดภัย
        int searchLimit = 50; // กัน Infinite Loop
        int count = 0;

        while (queue.Count > 0 && count < searchLimit)
        {
            Vector2Int current = queue.Dequeue();
            count++;

            if (!_mapManager.IsDangerous(current)) return current;

            Vector2Int[] neighbors = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            foreach (var dir in neighbors)
            {
                Vector2Int neighbor = current + dir;
                if (_mapManager.IsWalkable(neighbor) && !visited.Contains(neighbor))
                {
                    queue.Enqueue(neighbor);
                    visited.Add(neighbor);
                }
            }
        }

        return origin; // ถ้าหาไม่เจอจริงๆ ให้ยืนที่เดิม (หรืออาจจะสุ่มเดิน)
    }

    private Vector2Int ExecuteGetNextPath(Vector2Int target)
    {
        return PathfindAbility<BotController>.Execute(this, target);
    }

    #endregion //Private Logic
}