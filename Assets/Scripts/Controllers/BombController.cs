using System;
using UnityEngine;
using NaughtyAttributes;
using Genoverrei.DesignPattern;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(Animator))]
public sealed class BombController : MonoBehaviour
{
    [Header("Observer")]
    [SerializeField] private BombChannelSO _bombChannel;
    [SerializeField] private GameObject _poolKey;

    [Header("Components")]
    [SerializeField] private Rigidbody2D _rigidbody;
    [SerializeField] private CircleCollider2D _collider;
    [SerializeField] private Animator _animator;

    [Header("Clips Reference")]
    [SerializeField] private AnimationClip _noncriticalClip;
    [SerializeField] private AnimationClip _criticalClip;

    [ReadOnly][SerializeField] private Vector2Int _gridPosition;
    [ReadOnly][SerializeField] private int _radius;
    [ReadOnly][SerializeField] private float _lifeTime;
    [ReadOnly][SerializeField] private bool _isExploded;

    private StatsController _ownerStats;
    private bool _onCriticalPhase;

    // 🚀 ตัวแปรเสริมสำหรับ Smart Snapping
    private bool _isSmartSnapping;
    private Vector2 _targetSnapPos;

    private void Awake()
    {
        _rigidbody.bodyType = RigidbodyType2D.Dynamic;
        _rigidbody.gravityScale = 0f;
        _rigidbody.freezeRotation = true;

        // 🚀 [FIX] ทำให้ระเบิดเบาหวิว เพื่อไม่ให้มีแรงส่งไปผลักตัวละคร B
        _rigidbody.mass = 0.0001f;
        _rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    private void Update()
    {
        if (_isExploded) return;
        _lifeTime += Time.deltaTime;

        float totalTime = _noncriticalClip.length + _criticalClip.length;
        float timeLeft = totalTime - _lifeTime;

        if (timeLeft <= 0)
        {
            ExecuteSnapToGrid();
            ForceExplode();
            return;
        }

        if (!_onCriticalPhase && _lifeTime >= _noncriticalClip.length)
        {
            _animator.SetTrigger("ToCritical");
            _onCriticalPhase = true;

            // 🚀 เริ่มคำนวณเป้าหมายล่วงหน้าเมื่อเข้าระยะใกล้ระเบิด
            CalculateSmartSnapTarget(timeLeft);
        }

        // 🚀 ปรับความเร็วให้ค่อยๆ ไหลไปตรงกลางช่องเป้าหมายแบบเนียนๆ
        if (_isSmartSnapping && timeLeft > 0)
        {
            // ปรับ Velocity ให้พอดีกับเวลาที่เหลือ เพื่อให้ถึงจุดกึ่งกลาง Int เป๊ะพอดี
            _rigidbody.linearVelocity = (_targetSnapPos - (Vector2)transform.position) / timeLeft;
        }
        else if (_rigidbody.linearVelocity.sqrMagnitude > 0.01f)
        {
            // 🚀 ช่วยให้ระเบิดไหลตรงเลน (Align to Grid Axis) ตลอดเวลาที่เคลื่อนที่
            Vector2 currentPos = transform.position;
            Vector2 vel = _rigidbody.linearVelocity;
            if (Mathf.Abs(vel.x) > Mathf.Abs(vel.y))
                currentPos.y = Mathf.Lerp(currentPos.y, Mathf.Round(currentPos.y), Time.deltaTime * 10f);
            else
                currentPos.x = Mathf.Lerp(currentPos.x, Mathf.Round(currentPos.x), Time.deltaTime * 10f);

            transform.position = currentPos;
        }
    }

    private void CalculateSmartSnapTarget(float timeLeft)
    {
        Vector2 vel = _rigidbody.linearVelocity;
        if (vel.sqrMagnitude < 0.01f) return; // ไม่ได้เคลื่อนที่ ไม่ต้องคำนวณ

        Vector2 current = transform.position;
        Vector2 expected = current + (vel * timeLeft); // คาดการณ์จุดตก

        int targetX = Mathf.RoundToInt(expected.x);
        int targetY = Mathf.RoundToInt(expected.y);

        // 🚀 ดักไว้ไม่ให้ Snap กลับหลัง (ถ้าปัดเศษแล้วมันถอยหลัง ให้ปัดไปข้างหน้าแทน)
        if (vel.x > 0.1f && targetX < current.x) targetX = Mathf.CeilToInt(current.x);
        if (vel.x < -0.1f && targetX > current.x) targetX = Mathf.FloorToInt(current.x);
        if (vel.y > 0.1f && targetY < current.y) targetY = Mathf.CeilToInt(current.y);
        if (vel.y < -0.1f && targetY > current.y) targetY = Mathf.FloorToInt(current.y);

        // ล็อคแกนที่ไม่ได้เคลื่อนที่ให้อยู่ตรงกลางเป๊ะๆ
        if (Mathf.Abs(vel.x) > Mathf.Abs(vel.y)) targetY = Mathf.RoundToInt(current.y);
        else targetX = Mathf.RoundToInt(current.x);

        _targetSnapPos = new Vector2(targetX, targetY);
        _isSmartSnapping = true;
    }

    // 🚀 [FIX] เมื่อระเบิดที่กำลังวิ่งไปชนตัวละคร หรือชนกำแพง
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("LivingThings"))
        {
            _rigidbody.linearVelocity = Vector2.zero;
            _isSmartSnapping = false; // ชนปุ๊บ ยกเลิกระบบไหลล่วงหน้าทันที ให้ไปใช้ Snap ปกติแทน
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // 🚀 เมื่อตัวละครเดินพ้นระเบิด (รวมถึงตอนวางยัดตีน) ให้กลายเป็น Solid เพื่อปิดทาง
        if (other.CompareTag("Player") || other.gameObject.layer == LayerMask.NameToLayer("LivingThings"))
        {
            _collider.isTrigger = false;
        }
    }

    public void Initialize(BombBuilder builder, Vector2Int gridPos, StatsController ownerStats)
    {
        _ownerStats = ownerStats;
        _gridPosition = gridPos;
        _radius = builder.Radius;
        _isExploded = false;
        _onCriticalPhase = false;
        _isSmartSnapping = false; // เคลียร์สถานะด้วย
        _lifeTime = 0f;

        // 🚀 เริ่มต้นเป็น Trigger เพื่อให้วางยัดตีนได้ไม่ติดตัวละคร
        _collider.isTrigger = true;

        transform.position = new Vector3(gridPos.x, gridPos.y, 0f);
        _rigidbody.position = transform.position;
        _rigidbody.linearVelocity = Vector2.zero;
    }

    public void ForceExplode()
    {
        if (_isExploded) return;
        _isExploded = true;

        if (_bombChannel != null) _bombChannel.RaiseBombExploded(Vector2Int.RoundToInt(transform.position), _radius);
        if (_ownerStats != null) _ownerStats.BombsRemaining++;

        string key = (_poolKey != null) ? _poolKey.name : gameObject.name.Replace("(Clone)", "").Trim();
        ObjectPoolManager.Instance.Release(key, this);
    }

    private void ExecuteSnapToGrid()
    {
        _rigidbody.linearVelocity = Vector2.zero;

        // 🚀 ถ้าระบบ Smart Snap ทำงานอยู่ ให้ Snap ไปเป้าหมายข้างหน้าเลย
        Vector2 snappedPos = _isSmartSnapping ? _targetSnapPos : new Vector2(Mathf.Round(transform.position.x), Mathf.Round(transform.position.y));

        _rigidbody.MovePosition(snappedPos);
        transform.position = new Vector3(snappedPos.x, snappedPos.y, 0f); // ชัวร์ 100% ว่าย้ายจริง
    }
}