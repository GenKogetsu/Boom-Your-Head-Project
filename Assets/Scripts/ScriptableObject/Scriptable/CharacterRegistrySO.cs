using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// <para> (TH) : ??????????? Reference ??????????????? ?????????????????????????????????????????? </para>
/// <para> (EN) : Registry for in-scene character references to avoid finding objects. </para>
/// </summary>
[CreateAssetMenu(fileName = "CharacterRegistry", menuName = "BombGame/Data/CharacterRegistry")]
public class CharacterRegistrySO : ScriptableObject
{
    private Dictionary<Character, StatsController> _activeCharacters = new Dictionary<Character, StatsController>();

    public void Register(Character type, StatsController stats)
    {
        if (!_activeCharacters.ContainsKey(type))
            _activeCharacters.Add(type, stats);
        else
            _activeCharacters[type] = stats;
    }

    public StatsController GetCharacter(Character type)
    {
        _activeCharacters.TryGetValue(type, out var stats);
        return stats;
    }

    public void Clear() => _activeCharacters.Clear();
}