using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameSessionData", menuName = "BombGame/Data/GameSession")]
public class GameSessionDataSO : ScriptableObject, IAmScriptableObject
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
    public int PlayerCount = 0;
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

    public string ScriptName => name;

    public Character GetCharacterFromLibraryIndex(int index)
    {
        if (index >= 0 && index < _characterLibrary.Count)
            return _characterLibrary[index].CharacterType;
        return Character.None;
    }

    private void OnEnable()
    {
        _cachedLibrary = null;
    }

    public void ResetScripts()
    {
        CurrentStageIndex = 0;
        PlayerCount = 0;
        SelectedPlayers.Clear();
        SelectedBots.Clear();
    }

    // ✅ รับ index ที่ Player เลือกแล้วหาบอทจากที่เหลือ
    public void SetupMatch(List<Character> humanPlayers, List<int> playerSelectedIndices)
    {
        SelectedPlayers.Clear();
        SelectedBots.Clear();

        SelectedPlayers.AddRange(humanPlayers);

        int targetSlots = _characterLibrary.Count; // ✅ ใช้จำนวน library
        int botNeeded = targetSlots - SelectedPlayers.Count;

        // ✅ Skip index ที่ player เลือก และเพิ่มตามลำดับใน library
        for (int i = 0; i < _characterLibrary.Count && SelectedBots.Count < botNeeded; i++)
        {
            if (playerSelectedIndices.Contains(i)) continue; // ✅ Skip index ที่ player ใช้

            var mapping = _characterLibrary[i];
            Character charType = mapping.CharacterType;

            if (charType == Character.None || charType == Character.All)
                continue;

            SelectedBots.Add(charType);
        }

        Debug.Log($"[Session] Players:{SelectedPlayers.Count} Bots:{SelectedBots.Count}");
    }

    public bool IsBot(Character type) => SelectedBots.Contains(type);

    public GameObject GetCharacterPrefab(Character type)
    {
        if (_cachedLibrary == null)
        {
            _cachedLibrary = new Dictionary<Character, GameObject>();

            foreach (var mapping in _characterLibrary)
            {
                if (mapping.Prefab != null &&
                    !_cachedLibrary.ContainsKey(mapping.CharacterType))
                {
                    _cachedLibrary.Add(mapping.CharacterType, mapping.Prefab);
                }
            }
        }

        _cachedLibrary.TryGetValue(type, out var result);
        return result;
    }
}