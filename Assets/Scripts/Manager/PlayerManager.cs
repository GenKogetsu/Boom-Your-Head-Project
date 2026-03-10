using System;
using System.Collections.Generic;
using UnityEngine;
using Genoverrei.DesignPattern;
using Genoverrei.Libary;

namespace Genoverrei.Manager
{
    /// <summary>
    /// <para> (TH) : ตัวจัดการระบบผู้เล่น คอยจัดการการเกิดตามตำแหน่งที่กำหนดแบบเรียงลำดับ และลงทะเบียนตัวละคร </para>
    /// </summary>
    public sealed class PlayerManager : MonoBehaviour
    {
        [Header("Data Reference")]
        [SerializeField] private GameSessionDataSO _sessionData;
        [SerializeField] private MapDataSO _currentMapData;
        [SerializeField] private CharacterRegistrySO _characterRegistry;
        [SerializeField] private MatchManagerSO _matchManager;

        /// <summary>
        /// 🚀 สั่ง Spawn ตัวละครตามลำดับจุดเกิดที่กำหนดไว้ใน MapData (ไม่มีการสุ่ม)
        /// </summary>
        public void SpawnPlayersInMatch()
        {
            if (_sessionData == null || _currentMapData == null || _currentMapData.AvailableSpawnSets.Count == 0)
            {
                Debug.LogError("<b>[PlayerManager]</b> ข้อมูลไม่ครบถ้วน (เช็ค SO หรือ MapData ด้วยครับพี่)");
                return;
            }

            // 1. ดึงจุดเกิดชุดแรก (Index 0) มาใช้แบบเรียงลำดับ
            var spawnPositions = _currentMapData.AvailableSpawnSets[0].SpawnPositions;

            _characterRegistry.ResetScripts();
            List<Character> startingParticipants = new List<Character>();

            // 2. ดึงรายชื่อผู้เข้าร่วมทั้งหมด (SelectedPlayers + SelectedBots) จาก SO
            List<Character> participants = _sessionData.AllMatchParticipants;

            // 3. วนลูปเกิดตามลำดับ (Sequential Spawn)
            for (int i = 0; i < participants.Count; i++)
            {
                // ถ้าจำนวนคนเยอะกว่าจุดเกิดที่มี ให้ตัดจบแค่เท่าที่มีจุด
                if (i >= spawnPositions.Count) break;

                Character type = participants[i];
                Vector3 spawnPos = spawnPositions[i];

                GameObject prefab = _sessionData.GetCharacterPrefab(type);
                if (prefab != null)
                {
                    // ดึงตัวละครจาก Pool และตั้งค่า Rotation (หันหน้าเข้าฉาก 180 องศา)
                    StatsController stats = ObjectPoolManager.Instance.Get<StatsController>(
                        prefab.name, 
                        spawnPos, 
                        Quaternion.Euler(0, 180, 0)
                    );

                    if (stats != null)
                    {
                        stats.ResetStats();
                        
                        // ลงทะเบียนเข้า Registry เพื่อให้ระบบบอทหรือ UI หาตัวเจอ
                        _characterRegistry.Register(type, stats.gameObject);
                        
                        startingParticipants.Add(type);
                    }
                }
            }

            // 🏆 4. แจ้ง Match Manager ให้เริ่มกระบวนการเริ่มเกม (นับถอยหลัง)
            if (_matchManager != null) 
            {
                _matchManager.InitializeMatch(startingParticipants);
            }

            Debug.Log($"<b><color=#69F0AE>[PlayerManager]</color></b> ✅ Spawn Complete: Sequential Mode (Total: {startingParticipants.Count})");
        }
    }
}