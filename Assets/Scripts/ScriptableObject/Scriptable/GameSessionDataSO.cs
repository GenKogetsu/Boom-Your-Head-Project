using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameSessionData", menuName = "BombGame/Data/GameSession")]
public class GameSessionDataSO : ScriptableObject , IAmScriptableObject
{
    [Serializable]
    public struct CharacterMapping
    {
        public Character CharacterType;
        public GameObject Prefab;
    }

    [Header("Character Library")]
    [SerializeField] private List<CharacterMapping> _characterLibrary = new List<CharacterMapping>();

    [Header("Match Setup")]
    public int PlayerCount = 1;
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

    public void ResetSession()
    {
        CurrentStageIndex = 0;
        SelectedPlayers.Clear();
        SelectedBots.Clear();
        PlayerCount = 1;
    }

    // 🚀 [NEW] ฟังก์ชันบันทึกข้อมูลและเติมบอทอัตโนมัติ
    public void SetupMatch(List<Character> humanPlayers)
    {
        ResetSession();

        // 1. บันทึกรายชื่อคนเล่น
        SelectedPlayers.AddRange(humanPlayers);
        PlayerCount = SelectedPlayers.Count;

        // 2. เติมบอทจากตัวละครที่เหลือใน Library (อัตโนมัติ)
        // วนลูปตาม Character ทั้งหมดที่มีใน Enum (หรือตามที่ลิสต์ไว้ใน Library)
        foreach (var mapping in _characterLibrary)
        {
            Character charType = mapping.CharacterType;

            // ถ้าตัวละครนี้ไม่มีคนเลือก ให้เอาไปใส่ในลิสต์บอท
            if (!SelectedPlayers.Contains(charType))
            {
                SelectedBots.Add(charType);
            }
        }

        Debug.Log($"<b><color=#4CAF50>[Session]</color></b> Match Setup Complete: " +
                  $"{SelectedPlayers.Count} Players, {SelectedBots.Count} Bots.");
    }

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

    public void ResetScripts()
    {
        SelectedBots.Clear();
        SelectedPlayers.Clear();
        AllMatchParticipants.Clear();
    }
}