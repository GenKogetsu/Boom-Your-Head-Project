using UnityEngine;
using System.Collections.Generic;
using Genoverrei.DesignPattern;
using Genoverrei.Libary;
using NaughtyAttributes;

/// <summary>
/// <para> (TH) : สมอง AI ที่ใช้ MapChannel ในการตัดสินใจหนีระเบิดและหาทางเดิน </para>
/// </summary>
public sealed class BotController : MonoBehaviour, IPathfindable
{
    #region Variable
    [Header("Observer")]
    [SerializeField] private MapChannelSO _mapChannel;
    [SerializeField] private PlayerSessionChannelSO _sessionChannel;

    [Header("Identity")]
    [SerializeField] private Character _targetToDrive;

    [Header("Settings")]
    [SerializeField] private float _thinkInterval = 0.2f;
    [ReadOnly][SerializeField] private Vector2Int _targetGridPos;

    private Transform _bodyTransform;
    private float _nextThinkTime;
    private Vector2Int _currentSafeTarget;
    private bool _isFleeing;
    #endregion

    // 🚀 เปลี่ยนการเช็ค bodyTransform เป็นแบบมาตรฐาน Unity
    public Vector2Int CurrentGridPosition
    {
        get
        {
            if (_bodyTransform != null)
            {
                return new Vector2Int(
                    Mathf.RoundToInt(_bodyTransform.position.x),
                    Mathf.RoundToInt(_bodyTransform.position.y)
                );
            }
            return Vector2Int.zero;
        }
    }

    public IMapProvider MapProvider => null;

    private void Update()
    {
        // 🚀 แก้ไข null propagation ตรง ExecuteRefreshBody
        if (_bodyTransform == null)
        {
            ExecuteRefreshBody();
            return;
        }

        if (Time.time < _nextThinkTime) return;

        _nextThinkTime = Time.time + _thinkInterval;
        ExecuteThink();
    }

    private void ExecuteThink()
    {
        // 🚀 เช็ค Channel แบบปลอดภัย
        if (_mapChannel == null || _bodyTransform == null) return;

        Vector2Int currentPos = CurrentGridPosition;
        Vector2Int nextMove;

        // ถาม Channel ว่า "ตรงนี้อันตรายไหม?"
        if (_mapChannel.IsDangerous(currentPos))
        {
            // ถ้าเป้าหมายเดิมที่กำลังหนีไปมันเกิดอันตรายขึ้นมาใหม่ หรือยังไม่มีเป้าหมายหนี
            if (!_isFleeing || _mapChannel.IsDangerous(_currentSafeTarget))
            {
                _currentSafeTarget = ExecuteFindSafeSpot(currentPos);
                _isFleeing = true;
            }
            nextMove = PathfindAbility<BotController>.Execute(this, _currentSafeTarget);
        }
        else
        {
            _isFleeing = false;
            nextMove = PathfindAbility<BotController>.Execute(this, _targetGridPos);
        }

        // คำนวณทิศทาง
        Vector2 direction = new Vector2(
            nextMove.x - _bodyTransform.position.x,
            nextMove.y - _bodyTransform.position.y
        );

        // ยิง Signal บอกตัวละครให้เดินผ่าน EventBus
        if (EventBus.Instance != null)
        {
            EventBus.Instance.Publish<ISignal>(new CharacterAction(
                _targetToDrive,
                ActionType.Move,
                new MoveInputEvent(direction)
            ));
        }
    }

    private Vector2Int ExecuteFindSafeSpot(Vector2Int origin)
    {
        if (_mapChannel == null) return origin;

        Queue<Vector2Int> queue = new();
        HashSet<Vector2Int> visited = new();

        queue.Enqueue(origin);
        visited.Add(origin);

        int limit = 50; // กัน Infinite Loop กรณีหาที่ปลอดภัยไม่เจอจริง ๆ
        while (queue.Count > 0 && limit-- > 0)
        {
            Vector2Int curr = queue.Dequeue();

            // ถาม Channel: "ปลอดภัยยัง?"
            if (!_mapChannel.IsDangerous(curr)) return curr;

            foreach (var dir in new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
            {
                Vector2Int next = curr + dir;

                // ถาม Channel: "เดินได้ไหม?" และยังไม่เคยไป
                if (_mapChannel.IsWalkable(next) && !visited.Contains(next))
                {
                    queue.Enqueue(next);
                    visited.Add(next);
                }
            }
        }
        return origin;
    }

    private void ExecuteRefreshBody()
    {
        // 🚀 เปลี่ยนจาก _sessionChannel?.GetBody เป็นการเช็ค if
        if (_sessionChannel != null)
        {
            _bodyTransform = _sessionChannel.GetBody(_targetToDrive);
        }
    }

    Vector2Int IPathfindable.GetNextPath(Vector2Int target) => PathfindAbility<BotController>.Execute(this, target);
}