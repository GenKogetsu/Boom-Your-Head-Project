using UnityEngine;
using NaughtyAttributes;
using Genoverrei.DesignPattern; // 🚀 เพิ่ม Namespace สำหรับ MapChannelSO

[RequireComponent(typeof(StatsController))]
[RequireComponent(typeof(Rigidbody2D))]
public sealed class MoveController : MonoBehaviour, ITileMoveable
{
    #region Variable
    [Header("Observer")]
    [SerializeField] private MapChannelSO _mapChannel; // 🚀 เพิ่มช่องสำหรับลากสายไฟแผนที่

    [Header("Components")]
    [ReadOnly][SerializeField] private StatsController _stats;
    [ReadOnly][SerializeField] private Rigidbody2D _rb2;

    [Header("State")]
    [ReadOnly][SerializeField] private Vector2 _moveInputValue;
    [ReadOnly][SerializeField] private bool _isMoving;
    [ReadOnly][SerializeField] private Vector2 _targetPosition;
    public Vector2 LastMoveDirection;

    [Header("Settings")]
    [SerializeField] private LayerMask _collisionLayer;
    [SerializeField] private Vector2 _offset = Vector2.zero;

    private Vector2 _collisionCheckSize = new(0.5f, 0.5f);
    private Vector2 _lastDiscreteInput;
    #endregion

    #region ITileMoveable Implementation

    // 🚀 Implement Property ตาม Interface ให้หาย Error
    public MapChannelSO MapChannel => _mapChannel;

    // 🚀 แก้ Null Propagation ของ Unity (_stats != null)
    public float MoveSpeed => _stats != null ? _stats.CurrentSpeed : 5f;

    public Rigidbody2D Rigidbody => _rb2;
    public Vector2 TargetPosition { get => _targetPosition; set => _targetPosition = value; }
    public bool IsMoving { get => _isMoving; set => _isMoving = value; }
    public LayerMask CollisionLayer => _collisionLayer;
    public Vector2 CollisionCheckSize => _collisionCheckSize;
    public Vector2 MoveInputValue => _moveInputValue;
    public float OffsetX => _offset.x;
    public float OffsetY => _offset.y;

    public void Move(Vector2 input)
    {
        _moveInputValue = input;
        if (input == Vector2.zero) return;

        Vector2 currentDiscrete = TileMoveAbility.GetDiscreteDirection(input);
        if (currentDiscrete != _lastDiscreteInput)
        {
            LastMoveDirection = currentDiscrete;
            TileMoveAbility.ProcessMoveRequest(this);
        }
        _lastDiscreteInput = currentDiscrete;
    }
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        _targetPosition = TileMoveAbility.SnapToGrid(transform.position, _offset.x, _offset.y);

        // 🚀 แก้ไข Rigidbody null check
        if (_rb2 != null)
        {
            _rb2.position = _targetPosition;
        }

        LastMoveDirection = Vector2.down;
    }

    private void FixedUpdate()
    {
        TileMoveAbility.ExecuteUpdate(this, Time.fixedDeltaTime);
    }

    private void OnValidate()
    {
        if (_stats == null) _stats = GetComponent<StatsController>();
        if (_rb2 == null) _rb2 = GetComponent<Rigidbody2D>();
    }
    #endregion

    #region Gizmos
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        Vector2 nextDir = TileMoveAbility.GetDiscreteDirection(_moveInputValue);
        Vector2 checkPos = _targetPosition + nextDir;

        // เช็คสิ่งกีดขวาง
        bool isNextBlocked = _moveInputValue != Vector2.zero && TileMoveAbility.IsPositionOccupied(this, checkPos);

        Gizmos.color = _isMoving ? new Color(0, 1, 0, 0.3f) : (isNextBlocked ? new Color(1, 0, 0, 0.3f) : new Color(0, 1, 0, 0.3f));
        Gizmos.DrawCube(_targetPosition, new Vector3(0.9f, 0.9f, 0.1f));

        if (_moveInputValue != Vector2.zero)
        {
            Gizmos.color = isNextBlocked ? Color.red : Color.blue;
            Gizmos.DrawWireCube(checkPos, _collisionCheckSize);
        }
    }
#endif
    #endregion
}