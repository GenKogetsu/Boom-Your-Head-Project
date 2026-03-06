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
    [SerializeField] private GameObject _poolKey; // 🚀 แนะนำให้ใช้ GameObject ลาก Prefab มาใส่

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

    private void Awake()
    {
        _rigidbody.bodyType = RigidbodyType2D.Dynamic;
        _rigidbody.gravityScale = 0f;
        _rigidbody.freezeRotation = true;
    }

    private void Update()
    {
        if (_isExploded) return;
        _lifeTime += Time.deltaTime;

        float totalTime = _noncriticalClip.length + _criticalClip.length;
        if (_lifeTime >= totalTime)
        {
            ExecuteSnapToGrid();
            ForceExplode();
        }

        if (!_onCriticalPhase && _lifeTime >= _noncriticalClip.length)
        {
            _animator.SetTrigger("ToCritical");
            _onCriticalPhase = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
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
        _lifeTime = 0f;
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

        // 🚀 ดึง Key จาก Prefab ที่ลากมาใส่ หรือชื่อตัวเอง
        string key = (_poolKey != null) ? _poolKey.name : gameObject.name.Replace("(Clone)", "").Trim();

        Debug.Log($"<b><color=#40C4FF>[Bomb System]</color></b> 💣 {gameObject.name} is calling Release with Key: <b>{key}</b>");

        ObjectPoolManager.Instance.Release(key, this);
    }

    private void ExecuteSnapToGrid()
    {
        _rigidbody.linearVelocity = Vector2.zero;
        Vector2 snappedPos = new Vector2(Mathf.Round(transform.position.x), Mathf.Round(transform.position.y));
        _rigidbody.MovePosition(snappedPos);
    }
}