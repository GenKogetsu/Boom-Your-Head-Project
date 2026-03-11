using Genoverrei.DesignPattern;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class MatchResultManager : MonoBehaviour
{
    [Header("Players to Watch")]
    public List<StatsController> Players = new();

    private Dictionary<string, StatsController> _alive = new();

    [Header("Data & Scene")]
    public MatchResultSO ResultDataSO;
    public string ResultSceneName = "ResultScene";
     
    private List<string> _deathOrder = new();

    private List<string> _rankResults = new();
    private bool _gameEnded = false;

    private void Start()
    {
        if (ResultDataSO != null) ResultDataSO.ClearData();

        _alive.Clear();
        _deathOrder.Clear();
        _rankResults.Clear();
        _gameEnded = false;

        foreach (var player in Players)
        {
            if (player == null) continue;
            _alive[player.gameObject.name] = player;
        }
    }

    private void OnEnable()
    {
        if (EventBus.Instance != null)
            EventBus.Instance.Subscribe<CharacterDeathEvent>(OnCharacterDeath);
    }

    private void OnDisable()
    {
        if (EventBus.Instance != null)
            EventBus.Instance.Unsubscribe<CharacterDeathEvent>(OnCharacterDeath);
    }

    private void OnCharacterDeath(CharacterDeathEvent deathData)
    {
        if (_gameEnded) return;

        string victimName = deathData.Victim != null ? deathData.Victim.name : deathData.CharacterName.ToString();

        if (_deathOrder.Contains(victimName)) return;

        _deathOrder.Add(victimName);
        if (_alive.ContainsKey(victimName))
            _alive.Remove(victimName);

        if (_alive.Count <= 1)
        {
            BuildFinalRankingAndFinish();
        }
    }

    private void BuildFinalRankingAndFinish()
    {
        _rankResults.Clear();

        string winner = _alive.Keys.FirstOrDefault();
        if (!string.IsNullOrEmpty(winner))
        {
            _rankResults.Add(winner);
        }

        for (int i = _deathOrder.Count - 1; i >= 0; i--)
        {
            var name = _deathOrder[i];
            if (!_rankResults.Contains(name))
                _rankResults.Add(name);
        }

        _gameEnded = true;
        if (ResultDataSO != null)
            ResultDataSO.SetResults(_rankResults);

        Invoke(nameof(GoToResultScene), 1.5f);
    }

    private void GoToResultScene()
    {
        if (SceneEffectController.Instance != null)
            SceneEffectController.Instance.LoadSceneAndPlayEffect(ResultSceneName);
    }
}