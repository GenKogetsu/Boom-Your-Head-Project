using System;
using UnityEngine;
using Genoverrei.Libary;
using NaughtyAttributes;

namespace BombGame.Controller;

public sealed class BombController : MonoBehaviour
{
    #region Events

    public event Action<BombExplodedEvent> OnExplodeAction;

    #endregion //Events

    #region Variable

    [Header("Runtime Display")]
    [ReadOnly]
    [SerializeField] private Character _ownerName;

    [ReadOnly]
    [SerializeField] private Vector2Int _gridPosition;

    [ReadOnly]
    [SerializeField] private int _radius;

    [ReadOnly]
    [SerializeField] private float _currentTimer;

    [ReadOnly]
    [SerializeField] private bool _isExploded;

    private StatsController _ownerStats;

    #endregion //Variable

    private void Update()
    {
        if (_isExploded) return;
        _currentTimer -= Time.deltaTime;
        if (_currentTimer <= 0) ForceExplode();
    }

    private void OnDisable()
    {
        if (BombManager.Instance != null) BombManager.Instance.UnregisterBomb(this);
    }

    public void Initialize(BombBuilder builder, Vector2Int gridPos, StatsController ownerStats)
    {
        _ownerStats = ownerStats;
        _ownerName = ownerStats != null ? ownerStats.LivingName : Character.None;
        _gridPosition = gridPos;
        _radius = builder.Radius;
        _currentTimer = 3f;
        _isExploded = false;
    }

    public void ForceExplode()
    {
        if (_isExploded) return;
        _isExploded = true;

        var explodeEvent = new BombExplodedEvent(_gridPosition, _radius, BombManager.Instance.SolidTilemaps, BombManager.Instance.DestructibleTilemaps);
        OnExplodeAction?.Invoke(explodeEvent);

        if (_ownerStats != null) _ownerStats.BombsRemaining++;
        ObjectPoolManager.Instance.Release("Bomb", this);
    }
}