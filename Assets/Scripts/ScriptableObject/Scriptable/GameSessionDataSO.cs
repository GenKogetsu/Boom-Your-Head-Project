using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// <para> (TH) : เก็บข้อมูลเซสชันการเล่น แยกรายชื่อผู้เล่นและบอท เพื่อความสะดวกในการจัดการ AI และ UI </para>
/// <para> (EN) : Stores game session data, separating players and bots for easier AI and UI management. </para>
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

    [Tooltip("(TH) : รายชื่อตัวละครที่เป็นผู้เล่นจริง (Human)")]
    public List<Character> SelectedPlayers = new List<Character>();

    [Tooltip("(TH) : รายชื่อตัวละครที่เป็นบอท (Bot)")]
    public List<Character> SelectedBots = new List<Character>();

    [Header("Progression")]
    public int CurrentStageIndex = 0;

    private Dictionary<Character, GameObject> _cachedLibrary;

    /// <summary>
    /// (TH) : รวมรายชื่อตัวละครทั้งหมด (ทั้งคนและบอท) เพื่อใช้ตอนสั่ง Spawn
    /// </summary>
    public List<Character> AllMatchParticipants
    {
        get
        {
            List<Character> all = new List<Character>(SelectedPlayers);
            all.AddRange(SelectedBots);
            return all;
        }
    }

    private void OnEnable() => _cachedLibrary = null;

    /// <summary>
    /// <para> (TH) : ล้างข้อมูลเซสชันกลับเป็นค่าเริ่มต้น </para>
    /// </summary>
    public void ResetSession()
    {
        CurrentStageIndex = 0;
        SelectedPlayers.Clear();
        SelectedBots.Clear();
        PlayerCount = 1;
        Debug.Log("<b><color=#FFEB3B>[Session]</color></b> ♻️ Session Data has been reset.");
    }

    /// <summary>
    /// (TH) : ตรวจสอบว่า Character นี้เป็นผู้เล่นหรือบอท
    /// </summary>
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