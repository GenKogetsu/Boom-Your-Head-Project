using UnityEngine;
using System.Collections.Generic;
using Genoverrei.DesignPattern;
using Genoverrei.Libary;

namespace BombGame.Manager
{
    /// <summary>
    /// <para> (TH) : ศูนย์จัดการระบบระเบิด การงอกของไฟ และการแจ้งเตือนพื้นที่อันตรายผ่าน MapChannel </para>
    /// <para> (EN) : Bomb system manager handling flame spread and danger zone alerts via MapChannel. </para>
    /// </summary>
    public sealed class BombManager : MonoBehaviour, ISignalListener
    {
        #region Variable

        [Header("Observer Channels")]
        [SerializeField] private MapChannelSO _mapChannel;
        [SerializeField] private BombChannelSO _bombChannel;

        [Header("Data Registry")]
        [SerializeField] private CharacterRegistrySO _characterRegistry; // 🚀 เพิ่มสมุดทะเบียน

        [Header("Pool Keys (Use Prefab Name)")]
        [SerializeField] private BombController _bombPrefab;
        [SerializeField] private ExplosionAnimationController _explosionPrefab;

        [Header("Settings")]
        [SerializeField] private LayerMask _bombLayer;
        [SerializeField] private Vector2 _collisionCheckSize = new Vector2(0.8f, 0.8f);

        #endregion //Variable

        #region Unity Lifecycle

        private void OnEnable()
        {
            if (EventBus.Instance != null)
                EventBus.Instance.Subscribe<ISignal>(OnHandleSignal);

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
            if (signal.Action == ActionType.PlaceBomb)
            {
                TryPlaceBomb(signal.SignalTarget); // SignalTarget คือ Enum Character
            }
        }

        #endregion //Public Methods

        #region Private Logic (Explosion Flow)

        private void ExecuteHandleBombExploded(Vector2Int origin, int radius)
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

        private void ExecuteHandleFlameCollision(Vector3Int gridPos, Collider2D intruder)
        {
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

        private void TryPlaceBomb(Character ownerType)
        {
            // 🚀 ดึง StatsController จากสมุดทะเบียนโดยตรง! (ไม่ต้อง Cache เองแล้ว)
            StatsController stats = _characterRegistry.GetCharacter(ownerType);

            if (stats == null) return;
            if (stats.BombsRemaining <= 0) return;

            Vector2 spawnPos = new Vector2(Mathf.Round(stats.transform.position.x), Mathf.Round(stats.transform.position.y));

            if (Physics2D.OverlapBox(spawnPos, _collisionCheckSize, 0f, _bombLayer)) return;

            if (_bombPrefab == null) return;

            var bomb = ObjectPoolManager.Instance.Get<BombController>(_bombPrefab.name, (Vector3)spawnPos, Quaternion.identity);

            if (bomb != null)
            {
                new BombBuilder()
                    .SetRadius(stats.CurrentExplosionRange)
                    .Build(bomb, Vector2Int.RoundToInt(spawnPos), stats);

                stats.BombsRemaining--;
                Debug.Log($"<b><color=#FF8A65>[BombManager]</color></b> 💣 {ownerType} placed a bomb. ({stats.BombsRemaining} left)");
            }
        }

        #endregion //Placement
    }
}