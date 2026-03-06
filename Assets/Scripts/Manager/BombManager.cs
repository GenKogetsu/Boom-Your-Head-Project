using UnityEngine;
using System.Collections.Generic;
using Genoverrei.DesignPattern;
using Genoverrei.Libary;

namespace BombGame.Manager
{
    /// <summary>
    /// <para> (TH) : ศูนย์จัดการระบบระเบิด การงอกของไฟ และการแจ้งเตือนพื้นที่อันตรายผ่าน MapChannel </para>
    /// </summary>
    public sealed class BombManager : MonoBehaviour, ISignalListener
    {
        #region Variable

        [Header("Observer Channels")]
        [SerializeField] private MapChannelSO _mapChannel;     // 🚀 ส่งข้อมูลอันตรายให้บอท
        [SerializeField] private BombChannelSO _bombChannel;   // 🚀 รับสัญญาณจาก BombController และ Sensor

        [Header("Pool Keys (Use Prefab Name)")]
        [SerializeField] private BombController _bombPrefab;
        [SerializeField] private ExplosionAnimationController _explosionPrefab;

        [Header("Settings")]
        [SerializeField] private LayerMask _bombLayer;
        [SerializeField] private Vector2 _collisionCheckSize = new(0.8f, 0.8f);

        private Dictionary<Character, StatsController> _statsCache = new();

        #endregion //Variable

        #region Unity Lifecycle

        private void Start()
        {
            ExecuteInitializeCache();
        }

        private void OnEnable()
        {
            // 🚀 ฟังคำสั่งวางระเบิดจาก CharacterActionListener (Input/AI)
            if (EventBus.Instance != null)
                EventBus.Instance.Subscribe<ISignal>(OnHandleSignal);

            // 🚀 ฟังเหตุการณ์จากลูกระเบิดและเซนเซอร์ไฟ
            if (_bombChannel != null)
            {
                _bombChannel.OnBombExploded += ExecuteHandleBombExploded;
                _bombChannel.OnExplosionHit += ExecuteHandleFlameCollision;
            }
        }

        private void OnDisable()
        {
            if (EventBus.Instance != null)
                EventBus.Instance.Unsubscribe<ISignal>(OnHandleSignal);

            if (_bombChannel != null)
            {
                _bombChannel.OnBombExploded -= ExecuteHandleBombExploded;
                _bombChannel.OnExplosionHit -= ExecuteHandleFlameCollision;
            }
        }

        #endregion //Unity Lifecycle

        #region Public Methods (ISignalListener)

        public void OnHandleSignal(ISignal signal)
        {
            // รับสัญญาณวางระเบิด
            if (signal.Action == ActionType.PlaceBomb)
            {
                TryPlaceBomb(signal.SignalTarget);
            }
        }

        #endregion //Public Methods

        #region Private Logic (Explosion Flow)

        /// <summary>
        /// (TH) : เมื่อระเบิดทำงาน ให้เริ่มกระบวนการงอกของไฟ
        /// </summary>
        private void ExecuteHandleBombExploded(Vector2Int origin, int radius)
        {
            // 1. สร้างจุดระเบิดกลาง
            SpawnExplosionEffect(origin, BombPart.Start, Vector2.zero);

            // 2. งอกไฟออกไป 4 ทิศทาง
            ExecuteSpreadFlame(origin, Vector2Int.up, radius);
            ExecuteSpreadFlame(origin, Vector2Int.down, radius);
            ExecuteSpreadFlame(origin, Vector2Int.left, radius);
            ExecuteSpreadFlame(origin, Vector2Int.right, radius);
        }

        /// <summary>
        /// (TH) : คำนวณการงอกของไฟตามทิศทางและระยะ พร้อมแจ้งพื้นที่อันตรายให้ AI
        /// </summary>
        private void ExecuteSpreadFlame(Vector2Int origin, Vector2Int direction, int radius)
        {
            for (int i = 1; i <= radius; i++)
            {
                Vector2Int targetPos = origin + (direction * i);

                // 🚀 เปลี่ยนจาก ?. มาใช้ if เช็คตรงๆ
                if (_mapChannel != null)
                {
                    _mapChannel.AddDangerZone(targetPos);

                    if (_mapChannel.IsSolid(targetPos)) break;

                    if (_mapChannel.IsDestructible(targetPos))
                    {
                        SpawnExplosionEffect(targetPos, BombPart.End, (Vector2)direction);
                        break;
                    }
                }

                BombPart part = (i == radius) ? BombPart.End : BombPart.Middle;
                SpawnExplosionEffect(targetPos, part, (Vector2)direction);
            }
        }

        private void SpawnExplosionEffect(Vector2Int pos, BombPart part, Vector2 direction)
        {
            if (_explosionPrefab == null) return;

            // ดึงจาก Pool โดยใช้ชื่อ Prefab เป็น Key
            var effect = ObjectPoolManager.Instance.Get<ExplosionAnimationController>(
                _explosionPrefab.name,
                (Vector3)(Vector2)pos,
                Quaternion.identity
            );

            if (effect != null)
            {
                effect.Setup(part, direction);
            }
        }

        /// <summary>
        /// (TH) : เมื่อเซนเซอร์ไฟไปแตะโดนวัตถุ
        /// </summary>
        private void ExecuteHandleFlameCollision(Vector3Int gridPos, Collider2D intruder)
        {
            // 🚀 เปลี่ยนจาก ?. เป็น if
            if (_mapChannel != null)
            {
                _mapChannel.RaiseDestruction(gridPos, intruder);
            }

            if (intruder != null && intruder.CompareTag("Bomb"))
            {
                if (intruder.TryGetComponent<BombController>(out var nextBomb))
                {
                    nextBomb.ForceExplode();
                }
            }
        }

        #endregion //Explosion Flow

        #region Private Logic (Placement)

        private void TryPlaceBomb(Character owner)
        {
            if (!_statsCache.TryGetValue(owner, out var stats)) return;
            if (stats.BombsRemaining <= 0) return;

            // ปัดพิกัดตัวละครให้ลงล็อก Grid
            Vector2 spawnPos = new(Mathf.Round(stats.transform.position.x), Mathf.Round(stats.transform.position.y));

            // เช็คว่ามีระเบิดวางซ้อนกันไหม
            if (Physics2D.OverlapBox(spawnPos, _collisionCheckSize, 0f, _bombLayer)) return;

            if (_bombPrefab == null) return;

            var bomb = ObjectPoolManager.Instance.Get<BombController>(_bombPrefab.name, (Vector3)spawnPos, Quaternion.identity);

            if (bomb != null)
            {
                new BombBuilder()
                    .SetRadius(stats.CurrentExplosionRange)
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
                if (!_statsCache.ContainsKey(stats.LivingName))
                    _statsCache.Add(stats.LivingName, stats);
            }
        }

        #endregion //Placement
    }
}