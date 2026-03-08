using UnityEngine;
using NaughtyAttributes;
using Genoverrei.DesignPattern;
using Genoverrei.Libary;

[RequireComponent(typeof(StatsController))]
[RequireComponent(typeof(Rigidbody2D))]
public sealed class MoveController : MonoBehaviour, ITileMoveable
{
    #region Variable
    [Header("Observer")]
    [SerializeField] private MapChannelSO _mapChannel;

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
    #endregion

    #region ITileMoveable Implementation

    public MapChannelSO MapChannel => _mapChannel;
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

        if (currentDiscrete != Vector2.zero)
        {
            LastMoveDirection = currentDiscrete;

            // 🚀 [FIX] ถ้ายืนนิ่งอยู่ (!isMoving) ให้พยายามประมวลผลการเดินเสมอ
            // ไม่ต้องเช็คซ้ำแล้วว่าเปลี่ยนทิศไหม ป้องกันบั๊กยืนอัดกำแพงแล้วค้าง
            if (!_isMoving)
            {
                TileMoveAbility.ProcessMoveRequest(this);
            }
        }
    }
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        _targetPosition = TileMoveAbility.SnapToGrid(transform.position, _offset.x, _offset.y);

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