using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using System.Collections.Generic;
using Genoverrei.DesignPattern;
using Genoverrei.Libary;
using BombGame.RecordEventSpace;
using BombGame.EnumSpace;
using BombGame.Controller;

namespace BombGame.Manager;

/// <summary>
/// <para> Summary : </para>
/// <para> (TH) : ศูนย์กลางจัดการระบบระเบิด รับผิดชอบการงอกไฟ (Flame) และส่งค่า Animation Length ให้กับระเบิด </para>
/// <para> (EN) : Central bomb manager, responsible for flame propagation and passing animation length to bombs. </para>
/// </summary>
public sealed class BombManager : Singleton<BombManager>, ISignalListener
{
    #region Variable

    [Header("Observer Channels")]
    [SerializeField] private BombChannelSO _bombChannel;

    [Header("Dependencies")]
    [SerializeField] private MapManager _mapManager;

    [Header("Animation Data")]
    [Tooltip("ใส่คลิปท่า Idle ของระเบิดเพื่อใช้คำนวณเวลาถอยหลัง")]
    [SerializeField] private AnimationClip _nonCriticalBombClip;

    [Header("Pool Keys")]
    [SerializeField] private string _bombPoolKey = "Bomb";
    [SerializeField] private string _explosionPoolKey = "Explosion";

    [Header("Layer & Collision")]
    [SerializeField] private LayerMask _bombLayer;
    [SerializeField] private Vector2 _collisionCheckSize = new(0.8f, 0.8f);

    private Dictionary<Character, StatsController> _statsCache = new();

    #endregion //Variable

    #region Unity Lifecycle

    private void Start() => ExecuteInitializeCache();

    private void OnEnable()
    {
        EventBus.Instance.Subscribe<ISignal>(OnHandleSignal);

        if (_bombChannel != null)
        {
            _bombChannel.OnBombExploded += ExecuteHandleBombExplosion;
            _bombChannel.OnExplosionHit += ExecuteHandleFlameCollision;
        }
    }

    private void OnDisable()
    {
        EventBus.Instance.Unsubscribe<ISignal>(OnHandleSignal);

        if (_bombChannel != null)
        {
            _bombChannel.OnBombExploded -= ExecuteHandleBombExplosion;
            _bombChannel.OnExplosionHit -= ExecuteHandleFlameCollision;
        }
    }

    #endregion //Unity Lifecycle

    #region Public Methods

    public void OnHandleSignal(ISignal signal)
    {
        if (signal.Action == ActionType.PlaceBomb) TryPlaceBomb(signal.SignalTarget);
    }

    #endregion //Public Methods

    #region Private Logic (Explosion Flow)

    private void ExecuteHandleBombExplosion(Vector2Int origin, int radius)
    {
        ExecuteExplosionProcess(origin, radius);
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

            if (_mapManager.IsSolid(targetPos)) break;

            if (_mapManager.IsDestructible(targetPos))
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
        if (effect != null) effect.Setup(part, direction);
    }

    private void ExecuteHandleFlameCollision(Vector3Int gridPos, Collider2D intruder)
    {
        if (MapManager.Instance != null)
        {
            MapManager.Instance.ExecuteProcessDestruction(gridPos);
        }

        if (intruder.CompareTag("Bomb") && intruder.TryGetComponent<BombController>(out var nextBomb))
        {
            nextBomb.ForceExplode();
        }
    }

    private void TryPlaceBomb(Character owner)
    {
        if (!_statsCache.TryGetValue(owner, out var stats) || stats.BombsRemaining <= 0) return;

        Vector2 spawnPos = new Vector2(Mathf.Round(stats.transform.position.x), Mathf.Round(stats.transform.position.y));
        if (Physics2D.OverlapBox(spawnPos, _collisionCheckSize, 0f, _bombLayer)) return;

        // ดึงจาก Pool
        var bomb = ObjectPoolManager.Instance.Get<BombController>(_bombPoolKey, (Vector3)spawnPos, Quaternion.identity);
        if (bomb != null)
        {
            float clipTime = (_nonCriticalBombClip != null) ? _nonCriticalBombClip.length : 3f;

            new BombBuilder()
                .SetRadius(stats.CurrentExplosionRange)
                .SetNonCriticalTimer(clipTime)
                .Build(bomb, Vector2Int.RoundToInt(spawnPos), stats);

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