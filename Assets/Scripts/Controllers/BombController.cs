using System;
using UnityEngine;
using NaughtyAttributes;
using Genoverrei.DesignPattern;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(Animator))]
public sealed class BombController : MonoBehaviour
{
    #region Variable

    [Header("Observer")]
    [SerializeField] private BombChannelSO _bombChannel;

    [Header("Components")]
    [SerializeField] private Rigidbody2D _rigidbody;
    [SerializeField] private CircleCollider2D _collider;
    [SerializeField] private Animator _animator;

    [Header("Clips Reference")]
    [SerializeField] private AnimationClip _noncriticalClip;
    [SerializeField] private AnimationClip _criticalClip;

    [Header("Runtime Display")]
    [ReadOnly][SerializeField] private Character _ownerName;
    [ReadOnly][SerializeField] private Vector2Int _gridPosition;
    [ReadOnly][SerializeField] private int _radius;
    [ReadOnly][SerializeField] private float _lifeTime;
    [ReadOnly][SerializeField] private bool _isExploded;

    private StatsController _ownerStats;
    private bool _onCriticalPhase;

    #endregion //Variable

    #region Unity Lifecycle

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (_rigidbody == null) _rigidbody = GetComponent<Rigidbody2D>();
        if (_collider == null) _collider = GetComponent<CircleCollider2D>();
        if (_animator == null) _animator = GetComponent<Animator>();
    }
#endif

    private void Awake()
    {
        _rigidbody.bodyType = RigidbodyType2D.Dynamic;
        _rigidbody.gravityScale = 0f;
        _rigidbody.freezeRotation = true;
        _rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    private void Update()
    {
        if (_isExploded) return;

        _lifeTime += Time.deltaTime;

        // 🚀 1. เช็คเวลาจุดระเบิดก่อน (ต้องเช็คตลอดเวลา)
        float totalTime = _noncriticalClip.length + _criticalClip.length;
        if (_lifeTime >= totalTime)
        {
            ExecuteSnapToGrid();
            ForceExplode();
            return;
        }

        // 🚀 2. ถ้าเข้าช่วง Critical แล้ว ไม่ต้องเช็คเงื่อนไขเปลี่ยนท่าด้านล่างอีก
        if (_onCriticalPhase) return;

        // 🚀 3. เช็คเงื่อนไขเพื่อเข้าสู่ช่วง Critical
        if (_lifeTime >= _noncriticalClip.length)
        {
            if (_animator != null && _criticalClip != null)
            {
                _animator.SetTrigger("ToCritical");
                _onCriticalPhase = true;
            }
        }
    }

    private void FixedUpdate()
    {
        if (_isExploded) return;

        if (_rigidbody.linearVelocity.magnitude < 0.1f && _rigidbody.linearVelocity.magnitude > 0)
        {
            ExecuteSnapToGrid();
        }
    }

    #endregion //Unity Lifecycle

    #region Public Methods

    public void Initialize(BombBuilder builder, Vector2Int gridPos, StatsController ownerStats)
    {
        _ownerStats = ownerStats;
        _ownerName = ownerStats != null ? ownerStats.LivingName : Character.None;
        _gridPosition = gridPos;
        _radius = builder.Radius;

        _isExploded = false;
        _onCriticalPhase = false; // 🚀 Reset ทุกครั้งที่ออกจาก Pool
        _lifeTime = 0f;

        transform.position = new Vector3(gridPos.x, gridPos.y, 0f);
        _rigidbody.position = transform.position;
        _rigidbody.linearVelocity = Vector2.zero;

        _collider.isTrigger = true;

        if (_bombChannel != null) _bombChannel.RaiseBombPlanted(_gridPosition, _radius);
    }

    public void ForceExplode()
    {
        if (_isExploded) return;
        _isExploded = true;

        _gridPosition = Vector2Int.RoundToInt(transform.position);

        if (_bombChannel != null) _bombChannel.RaiseBombExploded(_gridPosition, _radius);

        if (_ownerStats != null) _ownerStats.BombsRemaining++;

        ObjectPoolManager.Instance.Release("Bomb", this);
    }

    #endregion //Public Methods

    #region Private Logic

    private void ExecuteSnapToGrid()
    {
        _rigidbody.linearVelocity = Vector2.zero;
        _rigidbody.angularVelocity = 0f;

        Vector2 snappedPos = new Vector2(Mathf.Round(transform.position.x), Mathf.Round(transform.position.y));
        _rigidbody.MovePosition(snappedPos);
        _gridPosition = Vector2Int.RoundToInt(snappedPos);
    }

    #endregion //Private Logic
}