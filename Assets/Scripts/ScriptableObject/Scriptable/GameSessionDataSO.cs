using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameSessionData", menuName = "BombGame/Data/GameSession")]
public class GameSessionDataSO : ScriptableObject, IAmScriptableObject
{
    // ... CharacterMapping และ _characterLibrary คงเดิม ...
    [Serializable]
    public struct CharacterMapping
    {
        public Character CharacterType;
        public GameObject Prefab;
    }

    [Header("Character Library")]
    [SerializeField] private List<CharacterMapping> _characterLibrary = new List<CharacterMapping>();

    [Header("Match Setup")]
    public int PlayerCount = 0; // 🚀 ตัวนี้จะถูก MainMenu เป็นคนสั่ง
    public List<Character> SelectedPlayers = new List<Character>();
    public List<Character> SelectedBots = new List<Character>();

    [Header("Progression")]
    public int CurrentStageIndex = 0;

    private Dictionary<Character, GameObject> _cachedLibrary;

    public List<Character> AllMatchParticipants
    {
        get
        {
            List<Character> all = new List<Character>(SelectedPlayers);
            all.AddRange(SelectedBots);
            return all;
        }
    }

    public string ScriptName => this.name;

    private void OnEnable() => _cachedLibrary = null;

    public void ResetScripts()
    {
        CurrentStageIndex = 0;
        PlayerCount = 0;
        SelectedPlayers.Clear();
        SelectedBots.Clear();
        // 🚀 เอา PlayerCount = 1; ออกจากตรงนี้ เพื่อไม่ให้มันรีเซ็ตค่าที่เลือกจากเมนู
    }

    public void SetupMatch(List<Character> humanPlayers)
    {
        // ล้างแค่ลิสต์ตัวละครพอ ไม่ต้องล้าง PlayerCount ที่ส่งมาจากเมนู
        SelectedPlayers.Clear();
        SelectedBots.Clear();

        // 1. บันทึกรายชื่อคนเล่นตามที่เลือกมาจริง
        SelectedPlayers.AddRange(humanPlayers);

        // 🚀 บรรทัดที่พี่ต้องการให้แก้: ไม่ต้องนับใหม่ ใช้ค่าเดิมที่เซ็ตมาจาก MainMenu
        // PlayerCount = SelectedPlayers.Count; <-- ลบทิ้งไปเลย

        // 2. เติมบอทจากตัวละครที่เหลือใน Library ให้ครบ (โหมดนี้เน้นให้บอทเติมเต็มช่องว่าง)
        foreach (var mapping in _characterLibrary)
        {
            Character charType = mapping.CharacterType;
            if (!SelectedPlayers.Contains(charType))
            {
                SelectedBots.Add(charType);
            }
        }

        Debug.Log($"<b><color=#4CAF50>[Session]</color></b> Mode: {PlayerCount} Players, Humans: {SelectedPlayers.Count}, Bots: {SelectedBots.Count}");
    }

    // ... ฟังก์ชัน GetCharacterPrefab และอื่นๆ คงเดิม ...
    public bool IsBot(Character type) => SelectedBots.Contains(type);

    public GameObject GetCharacterPrefab(Character type)
    {
        if (_cachedLibrary == null)
        {
            _cachedLibrary = new Dictionary<Character, GameObject>();
            foreach (var mapping in _characterLibrary)
            {
                if (mapping.Prefab != null && !_cachedLibrary.ContainsKey(mapping.CharacterType))
                    _cachedLibrary.Add(mapping.CharacterType, mapping.Prefab);
            }
        }
        _cachedLibrary.TryGetValue(type, out var result);
        return result;
    }
}