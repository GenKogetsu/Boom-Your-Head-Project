using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// <para> (TH) : เก็บข้อมูลเซสชันการเล่น เช่น จำนวนผู้เล่น ตัวละคร และด่านปัจจุบัน </para>
/// <para> (EN) : Stores game session data including player count, characters, and current stage. </para>
/// </summary>
[CreateAssetMenu(fileName = "GameSessionData", menuName = "BombGame/Data/GameSession")]
public class GameSessionDataSO : ScriptableObject
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
    public List<Character> SelectedCharacters = new List<Character>();

    [Header("Progression")]
    public int CurrentStageIndex = 0; // 🚀 เพิ่มตัวนี้เพื่อให้ GameFlowManager หายแดง

    private Dictionary<Character, GameObject> _cachedLibrary;

    private void OnEnable() => _cachedLibrary = null;

    /// <summary>
    /// <para> (TH) : ล้างข้อมูลเซสชันกลับเป็นค่าเริ่มต้น </para>
    /// </summary>
    public void ResetSession() // 🚀 เพิ่มฟังก์ชันนี้ให้ GameFlowManager เรียกใช้
    {
        CurrentStageIndex = 0;
        SelectedCharacters.Clear();
        PlayerCount = 1;
        Debug.Log("<b><color=#FFEB3B>[Session]</color></b> ♻️ Session Data has been reset.");
    }

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