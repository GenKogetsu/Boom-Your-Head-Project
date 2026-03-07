using UnityEngine;
using System.Collections.Generic;
using Genoverrei.DesignPattern;
using System.Linq;

namespace Genoverrei.Manager
{
    /// <summary>
    /// <para> (TH) : ตัวจัดการระบบผู้เล่น คอยสุ่มจุดเกิดให้ตัวละคร จัดการโหมดการเล่น และระบบเติมบอทอัตโนมัติ </para>
    /// <para> (EN) : Player manager handling random spawn points, game modes, and auto-filling bots. </para>
    /// </summary>
    public sealed class PlayerManager : MonoBehaviour
    {
        [Header("Data Reference")]
        [SerializeField] private GameSessionDataSO _sessionData;
        [SerializeField] private MapDataSO _currentMapData;
        [SerializeField] private CharacterRegistrySO _characterRegistry;
        [SerializeField] private MatchManagerSO _matchManager;

        [Header("Feature Settings")]
        [Tooltip("ถ้าคนเลือกไม่ครบโหมด จะให้สุ่มตัวละครอื่นมาเป็น Bot หรือไม่")]
        [SerializeField] private bool _autoFillBots = true;

        #region UI Selection & Mode Setup

        public void SetPlayerCount(int count)
        {
            if (_sessionData == null) return;
            _sessionData.PlayerCount = Mathf.Clamp(count, 1, 4);
            _sessionData.SelectedCharacters.Clear();
            Debug.Log($"<b><color=#4FC3F7>[PlayerManager]</color></b> Mode set to: <b>{count} Players</b>");
        }

        public void SelectCharacter(Character characterType)
        {
            if (_sessionData == null) return;
            if (_sessionData.SelectedCharacters.Count < _sessionData.PlayerCount)
            {
                _sessionData.SelectedCharacters.Add(characterType);
                Debug.Log($"<b><color=#69F0AE>[PlayerManager]</color></b> Player {_sessionData.SelectedCharacters.Count} selected: <b>{characterType}</b>");
            }
        }

        /// <summary>
        /// 🧹 เรียกใช้ก่อนเปลี่ยน Scene เพื่อล้างกระดาน
        /// </summary>
        public void PrepareForNextMap()
        {
            _characterRegistry.Clear();
            ObjectPoolManager.Instance.ReleaseAllPools();
            Debug.Log("<b><color=#FFEB3B>[PlayerManager]</color></b> 🧹 Cleanup all registries and pools for next map.");
        }

        #endregion

        #region Match Initialization (Spawning)

        public void SpawnPlayersInMatch()
        {
            if (_sessionData == null || _currentMapData == null || _currentMapData.AvailableSpawnSets.Count == 0)
            {
                Debug.LogError("<b><color=#FF5252>[PlayerManager]</color></b> ❌ ข้อมูลไม่ครบถ้วน ไม่สามารถเริ่มเกมได้");
                return;
            }

            // 🤖 1. ระบบ Auto-Fill Bots (ถ้าเลือกมาไม่ครบจำนวนโหมด)
            if (_autoFillBots) ExecuteAutoFillBots();

            // 🎲 2. สุ่มเลือก Set จุดเกิด
            int randomSetIndex = UnityEngine.Random.Range(0, _currentMapData.AvailableSpawnSets.Count);
            var selectedSet = _currentMapData.AvailableSpawnSets[randomSetIndex].SpawnPositions;
            List<Vector3> availablePositions = new List<Vector3>(selectedSet);

            _characterRegistry.Clear();
            List<Character> startingPlayers = new List<Character>();

            // 🚀 3. เริ่มกระบวนการ Spawn
            foreach (Character type in _sessionData.SelectedCharacters)
            {
                if (availablePositions.Count == 0) break;

                // สุ่มจับฉลากตำแหน่ง (แบบไม่ซ้ำ)
                int randPosIndex = UnityEngine.Random.Range(0, availablePositions.Count);
                Vector3 spawnPos = availablePositions[randPosIndex];
                availablePositions.RemoveAt(randPosIndex);

                GameObject prefab = _sessionData.GetCharacterPrefab(type);
                if (prefab != null)
                {
                    // ♻️ ดึงจาก Pool
                    StatsController stats = ObjectPoolManager.Instance.Get<StatsController>(prefab.name, spawnPos, Quaternion.identity);

                    if (stats != null)
                    {
                        stats.ResetStats();
                        _characterRegistry.Register(type, stats);
                        startingPlayers.Add(type);

                        Debug.Log($"<b><color=#4FC3F7>[PlayerManager]</color></b> 👤 Spawned: <b>{type}</b> at {spawnPos}");
                    }
                }
            }

            // 🏆 4. แจ้ง MatchManager ให้เริ่มนับยอดคนรอดชีวิต
            if (_matchManager != null) _matchManager.InitializeMatch(startingPlayers);
            Debug.Log($"<b><color=#69F0AE>[PlayerManager]</color></b> ✅ ระบบสุ่มจุดเกิดทำงานสำเร็จ (ใช้ Set: {randomSetIndex})");
        }

        /// <summary>
        /// 🤖 Logic สุ่มตัวละครมาเติมช่องว่างให้เต็มตามจำนวน PlayerCount
        /// </summary>
        private void ExecuteAutoFillBots()
        {
            int currentCount = _sessionData.SelectedCharacters.Count;
            int needed = _sessionData.PlayerCount - currentCount;

            if (needed <= 0) return;

            Debug.Log($"<b><color=#FFEB3B>[PlayerManager]</color></b> 🤖 Filling {needed} Bots to complete the match.");

            // ดึง Character ทั้งหมดที่มีใน Enum (สมมติว่าพี่ทำ Character Library ไว้ครบ)
            // หรือจะสุ่มจากตัวที่ยังไม่ได้โดนเลือกก็ได้
            for (int i = 0; i < needed; i++)
            {
                // สุ่มตัวละครมา 1 ตัว (ตัวอย่างนี้อาจจะสุ่มซ้ำได้ ถ้าพี่อยากได้บอทหน้าซ้ำ)
                // หรือพี่จะสร้าง Enum Character.Bot มาโดยเฉพาะก็ได้ครับ
                Character randomBot = (Character)UnityEngine.Random.Range(0, System.Enum.GetValues(typeof(Character)).Length);
                _sessionData.SelectedCharacters.Add(randomBot);
            }
        }

        #endregion
    }
}