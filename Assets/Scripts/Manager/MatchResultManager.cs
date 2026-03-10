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
        // 🚀 เคลียร์ค่าเก่าใน SO ทันทีที่เริ่มด่านใหม่
        if (ResultDataSO != null) ResultDataSO.ClearData();

        RankResults.Clear();
        _gameEnded = false;
    }

    private void Update()
    {
        if (_gameEnded) return;

        // 1. เช็คคนตาย (คนที่ตายก่อนถูกดันไปอยู่หลังสุดของลิสต์สุดท้าย)
        foreach (var p in Players)
        {
            if (p != null && p.CurrentHp <= 0 && !RankResults.Contains(p.LivingName.ToString()))
            {
                RankResults.Insert(0, p.LivingName.ToString());
            }
        }

        // 2. เช็คคนสุดท้ายที่รอด
        var alivePlayers = Players.Where(p => p != null && p.CurrentHp > 0).ToList();

        if (alivePlayers.Count <= 1)
        {
            if (alivePlayers.Count == 1)
            {
                var winner = alivePlayers[0].LivingName.ToString();
                if (!RankResults.Contains(winner)) RankResults.Add(winner);
            }

            RankResults.Reverse(); // กลับลิสต์ให้ Index 0 เป็นที่ 1
            FinishMatch();
        }
    }

    private void FinishMatch()
    {
        _gameEnded = true;

        // 🚀 ยัดข้อมูลลง SO อย่างเดียว ไม่เซฟไฟล์แล้ว!
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