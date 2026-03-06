using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Genoverrei.DesignPattern;

public class PlayerManager : Singleton<PlayerManager>
{
    [Header("Data References")]
    [SerializeField] private GameSessionData _sessionData;

    [Header("Characters")]
    [SerializeField] private List<StatsController> _allCharacters = new();

    private readonly List<StatsController> _players = new();
    private readonly List<StatsController> _bots = new();

    private bool _initialized = false;

    public IReadOnlyList<StatsController> Players
    {
        get
        {
            if (!_initialized) SetupCharacters();
            return _players;
        }
    }

    public IReadOnlyList<StatsController> Bots
    {
        get
        {
            if (!_initialized) SetupCharacters();
            return _bots;
        }
    }
    /*
    public override void Awake()
    {
        base.Awake();
        SetupCharacters();
    }
    */
    public void SetupCharacters()
    {
        _players.Clear();
        _bots.Clear();

        if (_allCharacters == null || _allCharacters.Count == 0)
        {
            var found = Object.FindObjectsByType<StatsController>(FindObjectsSortMode.None);
            _allCharacters = new List<StatsController>(found);
        }

        if (_allCharacters.Count == 0) return;

        foreach (var character in _allCharacters)
        {
            if (character == null) continue;

            /*if (IsPlayer(character.LivingName))
                _players.Add(character);*/
            else
                _bots.Add(character);
        }

        _initialized = true;
    }
    /*
    private bool IsPlayer(Character characterID)
    {
        if (_sessionData == null) return false;

        bool isP1 = _sessionData.PlayerCount >= 1 && characterID == _sessionData.Player1Character;
        bool isP2 = _sessionData.PlayerCount == 2 && characterID == _sessionData.Player2Character;

        return isP1 || isP2;
    }
    */
}