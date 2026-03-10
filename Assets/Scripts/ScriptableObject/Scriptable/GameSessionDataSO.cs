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

    // ✅ ดึงจาก Prefab name แทน CharacterType เพื่อให้ตรงกับตัวจริง
    public Character GetCharacterFromLibraryIndex(int index)
    {
        if (index >= 0 && index < _characterLibrary.Count)
        {
            var prefab = _characterLibrary[index].Prefab;
            if (prefab != null)
            {
                string prefabName = prefab.name;

                if (System.Enum.TryParse<Character>(prefabName, out var character))
                {
                    return character;
                }
                else
                {
                    Debug.LogWarning($"[GameSessionData] Prefab '{prefabName}' ไม่ตรงกับชื่อ Enum Character");
                }
            }
            else
            {
                Debug.LogWarning($"[GameSessionData] Element {index} ไม่มี Prefab ที่กำหนด");
            }
        }
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

    public void SetupMatch(List<Character> humanPlayers, List<int> playerSelectedIndices)
    {
        SelectedPlayers.Clear();
        SelectedBots.Clear();

        SelectedPlayers.AddRange(humanPlayers);

        int targetSlots = _characterLibrary.Count;
        int botNeeded = targetSlots - SelectedPlayers.Count;

        for (int i = 0; i < _characterLibrary.Count && SelectedBots.Count < botNeeded; i++)
        {
            if (playerSelectedIndices.Contains(i)) continue;

            // ✅ ใช้ GetCharacterFromLibraryIndex เหมือน GetCharacterFromIndex
            Character charType = GetCharacterFromLibraryIndex(i);

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