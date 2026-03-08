using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Genoverrei.DesignPattern;
using Genoverrei.Libary;

namespace Genoverrei.Manager
{
    /// <summary>
    /// <para> (TH) : ตัวจัดการระบบผู้เล่น คอยสุ่มจุดเกิดให้ตัวละคร จัดการโหมดการเล่น และระบบเติมบอทอัตโนมัติ </para>
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

            _sessionData.SelectedPlayers.Clear();
            _sessionData.SelectedBots.Clear();

            Debug.Log($"<b><color=#4FC3F7>[PlayerManager]</color></b> Mode set to: <b>{count} Players</b>");
        }

        public void SelectCharacter(Character characterType)
        {
            if (_sessionData == null) return;

            int totalSelected = _sessionData.SelectedPlayers.Count + _sessionData.SelectedBots.Count;

            if (totalSelected < _sessionData.PlayerCount)
            {
                _sessionData.SelectedPlayers.Add(characterType);
                Debug.Log($"<b><color=#69F0AE>[PlayerManager]</color></b> Player added: <b>{characterType}</b>");
            }
        }

        public void PrepareForNextMap()
        {
            _characterRegistry.Clear();
            if (ObjectPoolManager.Instance != null)
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

            // 🤖 1. ระบบ Auto-Fill Bots
            if (_autoFillBots) ExecuteAutoFillBots();

            // 🎲 2. สุ่มเลือก Set จุดเกิด
            int randomSetIndex = UnityEngine.Random.Range(0, _currentMapData.AvailableSpawnSets.Count);
            var selectedSet = _currentMapData.AvailableSpawnSets[randomSetIndex].SpawnPositions;
            List<Vector3> availablePositions = new List<Vector3>(selectedSet);

            _characterRegistry.Clear();
            List<Character> startingParticipants = new List<Character>();

            // 🚀 3. เริ่มกระบวนการ Spawn
            foreach (Character type in _sessionData.AllMatchParticipants)
            {
                if (availablePositions.Count == 0) break;

                int randPosIndex = UnityEngine.Random.Range(0, availablePositions.Count);
                Vector3 spawnPos = availablePositions[randPosIndex];
                availablePositions.RemoveAt(randPosIndex);

                GameObject prefab = _sessionData.GetCharacterPrefab(type);
                if (prefab != null)
                {
                    StatsController stats = ObjectPoolManager.Instance.Get<StatsController>(prefab.name, spawnPos, Quaternion.identity);

                    if (stats != null)
                    {
                        stats.ResetStats();

                        // =========================================================
                        // ✅ FIX: ส่ง stats.gameObject เข้าไปเก็บใน Registry (เป็น obj)
                        // เพื่อให้ตรงกับ Register(Character type, GameObject obj)
                        // =========================================================
                        _characterRegistry.Register(type, stats.gameObject);

                        startingParticipants.Add(type);

                        string roleColor = _sessionData.IsBot(type) ? "#FFCA28" : "#4FC3F7";
                        string roleTag = _sessionData.IsBot(type) ? "BOT" : "PLAYER";

                        Debug.Log($"<b><color={roleColor}>[PlayerManager]</color></b> 👤 Spawned {roleTag}: <b>{type}</b> at {spawnPos}");
                    }
                }
            }

            // 🏆 4. แจ้ง MatchManager
            if (_matchManager != null) _matchManager.InitializeMatch(startingParticipants);
            Debug.Log($"<b><color=#69F0AE>[PlayerManager]</color></b> ✅ Spawn Complete (Total: {startingParticipants.Count})");
        }

        private void ExecuteAutoFillBots()
        {
            int currentCount = _sessionData.SelectedPlayers.Count + _sessionData.SelectedBots.Count;
            int needed = _sessionData.PlayerCount - currentCount;

            if (needed <= 0) return;

            Debug.Log($"<b><color=#FFEB3B>[PlayerManager]</color></b> 🤖 Filling {needed} Bots to complete the match.");

            var allTypes = System.Enum.GetValues(typeof(Character)).Cast<Character>().Where(c => c != Character.None).ToList();

            for (int i = 0; i < needed; i++)
            {
                Character randomBot = allTypes[UnityEngine.Random.Range(0, allTypes.Count)];
                _sessionData.SelectedBots.Add(randomBot);
            }
        }

        #endregion
    }
}