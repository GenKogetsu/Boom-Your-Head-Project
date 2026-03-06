using UnityEngine;
using NaughtyAttributes;
using Genoverrei.DesignPattern;
using Genoverrei.Libary; // ดึงมาจาก Library ของพี่
using BombGame.EnumSpace;

namespace BombGame.Controller;

/// <summary>
/// <para> Summary : </para>
/// <para> (TH) : ตัวควบคุมการเคลื่อนที่แบบ Grid-based ของตัวละคร โดยดึงค่าความเร็วจาก StatsController และรับคำสั่งจาก Listener </para>
/// <para> (EN) : Grid-based movement controller, fetching speed from StatsController and receiving commands from Listener. </para>
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(StatsController))]
public sealed class MoveController : MonoBehaviour, ITileMoveable
{
    #region Variable

    // ไม่ต้องมี Identity และ EventBus แล้ว เพราะย้ายไปที่ CharacterActionListener หมดแล้วครับ กริบ!

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

    // ดึงค่าความเร็วจาก StatsController โดยตรง กริบๆ
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
        // ทำการ Snap ตำแหน่งเข้า Grid ตั้งแต่เริ่มเกม
        TargetPosition = TileMoveAbility<MoveController>.SnapToGrid(transform.position);
        if (_rigidbody != null) _rigidbody.position = TargetPosition;
    }

    private void Update()
    {
        // อัปเดตการเดินตามช่อง Grid ด้วย Library ของพี่
        ((ITileMoveable)this).UpdateMovement(Time.deltaTime);
    }

    #endregion //Unity Lifecycle

    #region Explicit Interface Implementation

    void ITileMoveable.Move(Vector2 input) => ExecuteMove(input);

    void ITileMoveable.UpdateMovement(float deltaTime) => TileMoveAbility<MoveController>.ExecuteUpdate(this, deltaTime);

    #endregion //Explicit Interface Implementation

    #region Public Methods

    /// <summary>
    /// <para> Summary : </para>
    /// <para> (TH) : รับค่าทิศทางการเดินจาก Listener ภายนอก (แทนที่ OnHandleSignal ตัวเก่า) </para>
    /// <para> (EN) : Receives movement direction from an external Listener (replaces the old OnHandleSignal). </para>
    /// </summary>
    public void SetMoveDirection(Vector2 direction)
    {
        ExecuteMove(direction);
    }

    #endregion //Public Methods

    #region Private Logic

    /// <summary>
    /// <para> Summary : </para>
    /// <para> (TH) : นำค่า Input มาอัปเดตทิศทาง เพื่อให้ระบบ ITileMoveable นำไปคำนวณการเดิน </para>
    /// <para> (EN) : Updates direction with Input value for the ITileMoveable system to calculate movement. </para>
    /// </summary>
    private void ExecuteMove(Vector2 input)
    {
        _moveInput = input;

        if (input != Vector2.zero)
        {
            // ดึงทิศทางแบบ Discrete (ขึ้น, ลง, ซ้าย, ขวา)
            _lastMoveDirection = TileMoveAbility<MoveController>.GetDiscreteDirection(input);
        }
    }

    #endregion //Private Logic
}