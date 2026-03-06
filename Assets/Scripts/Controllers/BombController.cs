using System;
using UnityEngine;
using Genoverrei.Libary;
using NaughtyAttributes;
using BombGame.RecordEventSpace;
using BombGame.EnumSpace;

namespace BombGame.Controller;

/// <summary>
/// <para> Summary : </para>
/// <para> (TH) : ตัวควบคุมระเบิดที่ใช้ค่าเวลาจาก AnimationClip ที่ Manager ส่งมาให้ พร้อมระบบ Snap Grid </para>
/// <para> (EN) : Bomb controller using timing from AnimationClip provided by Manager, featuring perfect grid snapping. </para>
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public sealed class BombController : MonoBehaviour
{
    #region Variable

    [Header("Observer")]
    [SerializeField] private BombChannelSO _bombChannel;

    [Header("Components")]
    [SerializeField] private Rigidbody2D _rigidbody;
    [SerializeField] private CircleCollider2D _collider;

    [Header("Runtime Display")]
    [ReadOnly][SerializeField] private Character _ownerName;
    [ReadOnly][SerializeField] private Vector2Int _gridPosition;
    [ReadOnly][SerializeField] private int _radius;
    [ReadOnly][SerializeField] private float _currentTimer;
    [ReadOnly][SerializeField] private bool _isExploded;
    [ReadOnly][SerializeField] private float _nonCircleTimer;

    private StatsController _ownerStats;
    private bool _isPlayerExited;

    #endregion //Variable

    #region Unity Lifecycle

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _collider = GetComponent<CircleCollider2D>();

        // Physics Setup: ตั้งค่าให้พร้อมสำหรับการโดนผลักและ Snap Grid
        _rigidbody.bodyType = RigidbodyType2D.Dynamic;
        _rigidbody.gravityScale = 0f;
        _rigidbody.freezeRotation = true;
        _rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    private void Update()
    {
        if (_isExploded) return;

        // นับถอยหลังตามเวลาที่ Manager ส่งมาให้
        _currentTimer -= Time.deltaTime;
        if (_currentTimer <= 0)
        {
            ExecuteSnapToGrid();
            ForceExplode();
        }

        // ถ้าคนวางเดินพ้นตัวระเบิดแล้ว ให้กลายเป็น Solid (isTrigger = false)
        if (!_isPlayerExited) ExecuteCheckPlayerExit();
    }

    private void FixedUpdate()
    {
        if (_isExploded) return;

        // ระบบ Snap: เมื่อแรงผลักเริ่มหมด ให้ดูดเข้ากึ่งกลาง Tile
        if (_rigidbody.linearVelocity.magnitude < 0.1f && _rigidbody.linearVelocity.magnitude > 0)
        {
            ExecuteSnapToGrid();
        }
    }

    #endregion //Unity Lifecycle

    #region Public Methods

    /// <summary>
    /// <para> (TH) : รับค่าเริ่มต้นจาก Manager โดยใช้ความยาวของ Clip เป็นตัวกำหนดเวลาถอยหลัง </para>
    /// </summary>
    public void Initialize(BombBuilder builder, Vector2Int gridPos, StatsController ownerStats)
    {
        _ownerStats = ownerStats;
        _ownerName = ownerStats != null ? ownerStats.LivingName : Character.None;
        _gridPosition = gridPos;
        _radius = builder.Radius;
        _isExploded = false;
        _isPlayerExited = false;

        // 🚀 ใช้เวลาถอยหลังตามความยาว Animation Clip ที่ส่งมาจาก Manager
        _currentTimer = _nonCircleTimer > 0 ? _nonCircleTimer : 4f;

        // วางที่ตำแหน่งกึ่งกลาง Tile
        transform.position = new Vector3(gridPos.x, gridPos.y, 0f);
        _rigidbody.position = transform.position;

        // เริ่มต้นเป็น Trigger เพื่อไม่ให้ติดเท้าคนวาง
        _collider.isTrigger = true;

        if (_bombChannel != null) _bombChannel.RaiseBombPlanted(_gridPosition, _radius);
    }

    public void ForceExplode()
    {
        if (_isExploded) return;
        _isExploded = true;

        // อัปเดตพิกัด Grid สุดท้ายก่อนส่งสัญญาณระเบิด
        _gridPosition = Vector2Int.RoundToInt(transform.position);

        if (_bombChannel != null) _bombChannel.RaiseBombExploded(_gridPosition, _radius);

        if (_ownerStats != null) _ownerStats.BombsRemaining++;

        ObjectPoolManager.Instance.Release("Bomb", this);
    }

    #endregion //Public Methods

    #region Private Logic

    private void ExecuteCheckPlayerExit()
    {
        // ตรวจสอบว่าไม่มีตัวละครอยู่ในรัศมี (ใช้ Layer Character)
        Collider2D hit = Physics2D.OverlapCircle(transform.position, 0.3f, LayerMask.GetMask("Character"));

        if (hit == null)
        {
            _isPlayerExited = true;
            _collider.isTrigger = false; // กลายเป็นสิ่งกีดขวางที่ชนได้และผลักได้
        }
    }

    private void ExecuteSnapToGrid()
    {
        _rigidbody.linearVelocity = Vector2.zero;
        _rigidbody.angularVelocity = 0f;

        // ปัดเศษพิกัดให้ลงล็อกกึ่งกลาง Tile (Perfect Tile)
        Vector2 snappedPos = new Vector2(Mathf.Round(transform.position.x), Mathf.Round(transform.position.y));
        _rigidbody.MovePosition(snappedPos);
        _gridPosition = Vector2Int.RoundToInt(snappedPos);
    }

    #endregion //Private Logic
}