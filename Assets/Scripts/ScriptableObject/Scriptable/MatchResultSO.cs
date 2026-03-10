using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MatchResultData", menuName = "BombGame/Data/MatchResultData")]
public class MatchResultSO : ScriptableObject
{
    public List<string> FinalRankNames = new List<string>();

    public void SetResults(List<string> results)
    {
        FinalRankNames = new List<string>(results);
    }

    public void ClearData()
    {
        FinalRankNames.Clear();
    }
}