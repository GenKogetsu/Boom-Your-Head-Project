using UnityEngine;
using NaughtyAttributes;
using Genoverrei.DesignPattern;
using Genoverrei.Libary;

namespace BombGame.Controller;

/// <summary>
/// <para> summary_MoveController </para>
/// <para> (TH) : ตัวควบคุมการเคลื่อนที่หลักของตัวละครที่รองรับระบบ Signal และ Grid-based Movement </para>
/// <para> (EN) : Main character movement controller supporting Signal system and Grid-based movement. </para>
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(StatsController))]
public class MoveController : MonoBehaviour, ITileMoveable, ISignalListener
{
    #region Variable

    [Header("Identity Settings")]
    [SerializeField] private Character _identity;

    [Header("Movement Settings")]
    [SerializeField] private LayerMask _collisionLayer;

    [Header("Linked Components")]
    [ReadOnly]
    [SerializeField] private Rigidbody2D _rigidbody;

    [ReadOnly]
    [SerializeField] private StatsController _stats;

    [Header("Debug Info")]
    [ReadOnly]
    [SerializeField] private Vector2 _lastMoveDirection = Vector2.down;

    private Vector2 _moveInput;

    #endregion //Variable

    #region ITileMoveable Properties

    public float MoveSpeed => _stats.CurrentSpeed;
    public Rigidbody2D Rigidbody => _rigidbody;
    public Vector2 TargetPosition { get; set; }
    public bool IsMoving { get; set; }
    public LayerMask CollisionLayer => _collisionLayer;
    public Vector2 CollisionCheckSize => new(0.8f, 0.8f);
    public Vector2 MoveInputValue => _moveInput;
    public Vector2 LastMoveDirection => _lastMoveDirection;

    #endregion //ITileMoveable Properties

    #region Unity Lifecycle

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (_rigidbody == null) _rigidbody = GetComponent<Rigidbody2D>();
        if (_stats == null) _stats = GetComponent<StatsController>();
    }
#endif

    private void Awake()
    {
        TargetPosition = TileMoveAbility<MoveController>.SnapToGrid(transform.position);
        if (_rigidbody != null) _rigidbody.position = TargetPosition;
    }

    private void OnEnable() => EventBus.Instance.Subscribe<ISignal>(OnHandleSignal);

    private void OnDisable() => EventBus.Instance.Unsubscribe<ISignal>(OnHandleSignal);

    private void Update() => ((ITileMoveable)this).UpdateMovement(Time.deltaTime);

    #endregion //Unity Lifecycle

    #region Explicit Interface Implementation

    void ITileMoveable.Move(Vector2 input) => ExecuteMove(input);

    void ITileMoveable.UpdateMovement(float deltaTime) => TileMoveAbility<MoveController>.ExecuteUpdate(this, deltaTime);

    #endregion //Explicit Interface Implementation

    #region Public Methods

    public void OnHandleSignal(ISignal signal)
    {
        if (signal.SignalTarget == _identity || signal.SignalTarget == Character.All)
        {
            if (signal.Action == ActionType.Move && signal.Event is MoveInputEvent data)
            {
                ExecuteMove(data.Direction);
            }
        }
    }

    #endregion //Public Methods

    #region Private Logic

    private void ExecuteMove(Vector2 input)
    {
        _moveInput = input;
        if (input != Vector2.zero)
        {
            _lastMoveDirection = TileMoveAbility<MoveController>.GetDiscreteDirection(input);
        }
    }

    #endregion //Private Logic
}