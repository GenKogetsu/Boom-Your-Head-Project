using System.Collections.Generic;
using Genoverrei.Libary;
using NaughtyAttributes;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CarMovementController : MonoBehaviour
{
    public enum SpeedState { VerySlow, Slow, Middle, Fast, VeryFast, Nitro }

    #region Variables
    [Header("Movement Settings")]
    [SerializeField] private Rigidbody2D _rb2;
    [SerializeField] private List<Vector2> _targetposition;
    [SerializeField] private float _baseMoveSpeed = 5f;
    [SerializeField] private float _stoppingDistance = 0.15f;


    [Header("Pacing Randomizer")]
    [ReadOnly][SerializeField] private SpeedState _currentState;
    [ReadOnly][SerializeField] private float _stateTimer;
    [ReadOnly][SerializeField] private float _currentTargetDuration;
    [ReadOnly][SerializeField] private float _currentMultiplier;

    [ReadOnly][SerializeField] private int _targetIndex = 0;
    [ReadOnly][SerializeField] private Vector2 _lastMoveDiraction;
    [ReadOnly][SerializeField] private float _currentMoveSpeed;

    public float CurrentMoveSpeed
    {
        get => _currentMoveSpeed;
        private set => _currentMoveSpeed = Mathf.Max(0, value);
    }

    [Header("Animation Settings")]
    [SerializeField] private Animator _animator;
    #endregion

    private void Start() => RandomizeState();

    private void Update()
    {
        UpdatePacing();
        UpdateDirection();
    }

    private void FixedUpdate()
    {
        if (_targetposition == null || _targetposition.Count == 0) return;

        UpdateAnimation();

        Vector2 targetPos = _targetposition[_targetIndex];
        float distToTarget = Vector2.Distance(_rb2.position, targetPos);
        Vector2 nextMovement = _lastMoveDiraction * CurrentMoveSpeed * Time.fixedDeltaTime;

        if (nextMovement.magnitude >= distToTarget)
        {
            _rb2.MovePosition(targetPos);
            _targetIndex = (_targetIndex + 1) % _targetposition.Count;
        }
        else
        {
            _rb2.MovePosition(_rb2.position + nextMovement);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        
        if (other.CompareTag("LivingThings"))
        {
            var target = other.GetComponentInParent<ITakeDamageable>();

            target.TakeDamage(1);
        }

        if (other.TryGetComponent<BombController>(out var bomb))
        {
            bomb.ForceExplode();
        }
    }

    #region AI Logic (Movement & Pacing)

    private void UpdatePacing()
    {
        _stateTimer += Time.deltaTime;
        if (_stateTimer >= _currentTargetDuration) RandomizeState();
    }

    private void RandomizeState()
    {
        _stateTimer = 0;
        _currentState = (SpeedState)Random.Range(0, 6);
        switch (_currentState)
        {
            case SpeedState.VerySlow: _currentMultiplier = 0.2f; _currentTargetDuration = Random.Range(1.0f, 2.0f); break;
            case SpeedState.Slow: _currentMultiplier = 0.5f; _currentTargetDuration = Random.Range(1.5f, 2.5f); break;
            case SpeedState.Middle: _currentMultiplier = 0.8f; _currentTargetDuration = Random.Range(2.0f, 3.0f); break;
            case SpeedState.Fast: _currentMultiplier = 1.2f; _currentTargetDuration = Random.Range(2.0f, 3.5f); break;
            case SpeedState.VeryFast: _currentMultiplier = 1.6f; _currentTargetDuration = Random.Range(1.5f, 2.5f); break;
            case SpeedState.Nitro: _currentMultiplier = 2.2f; _currentTargetDuration = Random.Range(0.8f, 1.5f); break;
        }
    }

    private void UpdateDirection()
    {
        if (_targetposition == null || _targetposition.Count == 0) return;

        Vector2 target = _targetposition[_targetIndex];
        Vector2 diff = target - _rb2.position;
        float distance = diff.magnitude;

        if (distance > _stoppingDistance)
        {
            if (Mathf.Abs(diff.x) >= Mathf.Abs(diff.y))
                _lastMoveDiraction = new Vector2(Mathf.Sign(diff.x), 0);
            else
                _lastMoveDiraction = new Vector2(0, Mathf.Sign(diff.y));

            float targetSpeed = _baseMoveSpeed * _currentMultiplier;
            if (distance < 0.8f) targetSpeed *= Mathf.Clamp01(distance + 0.2f);
            CurrentMoveSpeed = Mathf.Lerp(CurrentMoveSpeed, targetSpeed, Time.deltaTime * 5f);
        }
    }

    private void UpdateAnimation()
    {
        if (_lastMoveDiraction != Vector2.zero)
        {
            _animator.SetFloat("LastMoveX", _lastMoveDiraction.x);
            _animator.SetFloat("LastMoveY", _lastMoveDiraction.y);
            _animator.speed = Mathf.Clamp(CurrentMoveSpeed / _baseMoveSpeed, 0.2f, 3.0f);
        }
        else _animator.speed = 0f;
    }

    #endregion

    private void OnDrawGizmos()
    {
        if (_targetposition == null || _targetposition.Count == 0) return;
        for (int i = 0; i < _targetposition.Count; i++)
        {
            Gizmos.color = (i == _targetIndex) ? Color.green : Color.gray;
            Gizmos.DrawWireSphere(_targetposition[i], 0.3f);
            Vector2 current = _targetposition[i];
            Vector2 next = _targetposition[(i + 1) % _targetposition.Count];
            float t = (_currentMultiplier - 0.2f) / (2.2f - 0.2f);
            Gizmos.color = Color.Lerp(Color.cyan, Color.red, t);
            Gizmos.DrawLine(current, new Vector2(next.x, current.y));
            Gizmos.DrawLine(new Vector2(next.x, current.y), next);
        }
    }
}