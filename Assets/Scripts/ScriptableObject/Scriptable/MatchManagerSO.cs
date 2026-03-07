using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// <para> (TH) : ตัวจัดการสถานะของแมตช์ คอยตรวจสอบผู้รอดชีวิต จัดอันดับ และประกาศจบเกม </para>
/// <para> (EN) : Match state manager tracking alive players, placements, and match end events. </para>
/// </summary>
[CreateAssetMenu(fileName = "MatchManager", menuName = "BombGame/Manager/MatchManager")]
public class MatchManagerSO : ScriptableObject
{
    [Header("Runtime State")]
    public List<Character> AlivePlayers = new List<Character>();
    public List<Character> MatchPlacements = new List<Character>();

    [Header("Events")]
    public UnityEvent OnMatchEnded;

    public void InitializeMatch(List<Character> startingPlayers)
    {
        AlivePlayers.Clear();
        MatchPlacements.Clear();
        AlivePlayers.AddRange(startingPlayers);
    }

    public void ReportPlayerDeath(Character deadPlayer)
    {
        if (AlivePlayers.Contains(deadPlayer))
        {
            AlivePlayers.Remove(deadPlayer);
            MatchPlacements.Add(deadPlayer);
        }

        CheckMatchCondition();
    }

    private void CheckMatchCondition()
    {
        if (AlivePlayers.Count <= 1)
        {
            if (AlivePlayers.Count == 1)
            {
                MatchPlacements.Add(AlivePlayers[0]);
                AlivePlayers.Clear();
            }
            OnMatchEnded?.Invoke();
        }
    }
}