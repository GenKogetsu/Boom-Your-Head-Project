using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using System.Collections.Generic;
using Genoverrei.DesignPattern;
using Genoverrei.Libary;

namespace BombGame.Manager;

/// <summary>
/// <para> summary_BombManager </para>
/// <para> (TH) : ศูนย์กลางจัดการระบบระเบิดที่ใช้ระบบ Pool และเชื่อมต่อ Observer ระหว่าง Sensor กับ Manager อื่นๆ </para>
/// <para> (EN) : Central bomb manager using the pool system and connecting observers between sensors and other managers. </para>
/// </summary>
public sealed class BombManager : Singleton<BombManager>, ISignalListener
{
    #region Events

    /// <summary>
    /// <para> (TH) : เหตุการณ์เมื่อเกิดการระเบิด เพื่อให้ Manager อื่น (เช่น TileManager) มาดักฟังไปจัดการต่อ </para>
    /// </summary>
    public event Action<BombExplodedEvent> OnBombExploded;

    #endregion //Events

    #region Variable

    [Header("Pool Keys")]
    [SerializeField] private string _bombPoolKey = "Bomb";

    [SerializeField] private string _explosionPoolKey = "Explosion";

    [Header("Layer & Collision")]
    [SerializeField] private LayerMask _bombLayer;

    [SerializeField] private Vector2 _collisionCheckSize = new(0.8f, 0.8f);

    [Header("Grid Data")]
    [SerializeField] private List<Tilemap> _solidTilemaps = new();

    [SerializeField] private List<Tilemap> _destructibleTilemaps = new();

    private Dictionary<Character, StatsController> _statsCache = new();

    #endregion //Variable

    #region Properties

    public List<Tilemap> SolidTilemaps => _solidTilemaps;

    public List<Tilemap> DestructibleTilemaps => _destructibleTilemaps;

    #endregion //Properties

    #region Unity Lifecycle

    private void Start() => ExecuteInitializeCache();

    private void OnEnable() => EventBus.Instance.Subscribe<ISignal>(OnHandleSignal);

    private void OnDisable() => EventBus.Instance.Unsubscribe<ISignal>(OnHandleSignal);

    #endregion //Unity Lifecycle

    #region Public Methods (Observer Registration)

    public void RegisterBomb(BombController bomb) => bomb.OnExplodeAction += ExecuteHandleBombExplosion;

    public void UnregisterBomb(BombController bomb) => bomb.OnExplodeAction -= ExecuteHandleBombExplosion;

    // แก้ไขให้รองรับ Parameter ใหม่จาก Sensor (Vector2Int)
    public void RegisterExplosionSensor(BombTriggerSensor sensor) => sensor.OnExplodeHit += ExecuteHandleFlameCollision;

    public void UnregisterExplosionSensor(BombTriggerSensor sensor) => sensor.OnExplodeHit -= ExecuteHandleFlameCollision;

    public void OnHandleSignal(ISignal signal)
    {
        if (signal.Action == ActionType.PlaceBomb) TryPlaceBomb(signal.SignalTarget);
    }

    #endregion //Public Methods

    #region Private Logic (Explosion Flow)

    private void ExecuteHandleBombExplosion(BombExplodedEvent data)
    {
        OnBombExploded?.Invoke(data);
        ExecuteExplosionProcess(data.Position, data.Radius);
    }

    private void ExecuteExplosionProcess(Vector2Int origin, int radius)
    {
        SpawnExplosionEffect(origin, BombPart.Start, Vector2.zero);

        ExecuteSpreadFlame(origin, Vector2Int.up, radius);
        ExecuteSpreadFlame(origin, Vector2Int.down, radius);
        ExecuteSpreadFlame(origin, Vector2Int.left, radius);
        ExecuteSpreadFlame(origin, Vector2Int.right, radius);
    }

    private void ExecuteSpreadFlame(Vector2Int origin, Vector2Int direction, int radius)
    {
        for (int i = 1; i <= radius; i++)
        {
            Vector2Int targetPos = origin + (direction * i);

            if (ExecuteCheckTilemaps(targetPos, _solidTilemaps)) break;

            if (ExecuteCheckTilemaps(targetPos, _destructibleTilemaps))
            {
                SpawnExplosionEffect(targetPos, BombPart.End, (Vector2)direction);
                break;
            }

            BombPart part = (i == radius) ? BombPart.End : BombPart.Middle;
            SpawnExplosionEffect(targetPos, part, (Vector2)direction);
        }
    }

    private void SpawnExplosionEffect(Vector2Int pos, BombPart part, Vector2 direction)
    {
        var effect = ObjectPoolManager.Instance.Get<ExplosionAnimationController>(_explosionPoolKey, (Vector3)(Vector2)pos, Quaternion.identity);

        if (effect != null)
        {
            effect.Setup(part, direction);
        }
    }

    /// <summary>
    /// <para> (TH) : จัดการการชนของไฟระเบิด โดยส่งต่อตำแหน่งให้ TileManager และทำ Chain Reaction </para>
    /// </summary>
    private void ExecuteHandleFlameCollision(Vector3Int gridPos, Collider2D intruder)
    {
        // [Direct Observer] ส่งตำแหน่งให้ TileManager ไปจัดการลบ Tile และสุ่มของต่อทันที
        if (TileManager.Instance != null)
        {
            TileManager.Instance.ExecuteProcessDestruction(gridPos);
        }

        // Chain Reaction Logic
        if (intruder.CompareTag("Bomb") && intruder.TryGetComponent<BombController>(out var nextBomb))
        {
            nextBomb.ForceExplode();
        }
    }

    private bool ExecuteCheckTilemaps(Vector2Int pos, List<Tilemap> maps)
    {
        foreach (var map in maps)
        {
            if (map.HasTile((Vector3Int)pos)) return true;
        }
        return false;
    }

    private void TryPlaceBomb(Character owner)
    {
        if (!_statsCache.TryGetValue(owner, out var stats) || stats.BombsRemaining <= 0) return;

        Vector2 spawnPos = new Vector2(Mathf.Round(stats.transform.position.x), Mathf.Round(stats.transform.position.y));

        if (Physics2D.OverlapBox(spawnPos, _collisionCheckSize, 0f, _bombLayer)) return;

        var bomb = ObjectPoolManager.Instance.Get<BombController>(_bombPoolKey, (Vector3)spawnPos, Quaternion.identity);

        if (bomb != null)
        {
            RegisterBomb(bomb);
            new BombBuilder().SetRadius(stats.CurrentExplosionRange).Build(bomb, Vector2Int.RoundToInt(spawnPos), stats);
            stats.BombsRemaining--;
        }
    }

    private void ExecuteInitializeCache()
    {
        _statsCache.Clear();
        var allStats = FindObjectsByType<StatsController>(FindObjectsSortMode.None);
        foreach (var stats in allStats)
        {
            if (!_statsCache.ContainsKey(stats.LivingName)) _statsCache.Add(stats.LivingName, stats);
        }
    }

    #endregion //Private Logic
}