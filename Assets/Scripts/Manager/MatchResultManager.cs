using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Genoverrei.DesignPattern;

public class MatchResultManager : MonoBehaviour
{
    [Header("Players to Watch")]
    public List<StatsController> Players;

    [Header("Data & Scene")]
    public MatchResultSO ResultDataSO;
    public string ResultSceneName = "ResultScene";

    private List<string> RankResults = new List<string>();
    private bool _gameEnded = false;

    private void Start()
    {
        if (ResultDataSO != null) ResultDataSO.ClearData();

        RankResults.Clear();
        _gameEnded = false;
    }

    private void Update()
    {
        if (_gameEnded) return;

        foreach (var p in Players)
        {
            if (p != null && p.CurrentHp <= 0 && !RankResults.Contains(p.LivingName.ToString()))
            {
                RankResults.Add(p.LivingName.ToString());
            }
        }

        int aliveCount = 0;
        StatsController survivor = null;

        for (int i = 0; i < Players.Count; i++)
        {
            if (Players[i] != null && Players[i].CurrentHp > 0)
            {
                aliveCount++;
                survivor = Players[i];
            }
        }

        if (aliveCount <= 1)
        {
            if (aliveCount == 1 && survivor != null)
            {
                var winner = survivor.LivingName.ToString();
                if (!RankResults.Contains(winner)) RankResults.Add(winner);
            }

            RankResults.Reverse();
            FinishMatch();
        }
    }

    private void FinishMatch()
    {
        _gameEnded = true;

        if (ResultDataSO != null)
        {
            ResultDataSO.SetResults(RankResults);
        }

        Invoke(nameof(GoToResultScene), 1.5f);
    }

    private void GoToResultScene()
    {
        if (SceneEffectController.Instance != null)
            SceneEffectController.Instance.LoadSceneAndPlayEffect(ResultSceneName);
    }
}