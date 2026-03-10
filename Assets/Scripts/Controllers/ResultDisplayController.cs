using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ResultDisplayController : MonoBehaviour
{
    [System.Serializable]
    public class CharacterRankSprites
    {
        public string CharacterName;
        public List<Sprite> RankPortraits; // ช่อง 0=ที่1, 1=ที่2, 2=ที่3, 3=ที่4
    }

    [Header("Data Source")]
    public MatchResultSO ResultDataSO;

    [Header("UI Image Slots (1st - 4th)")]
    public List<Image> DisplaySlots;

    [Header("Character Sprite Database")]
    public List<CharacterRankSprites> SpriteDatabase;

    private void Awake()
    {
        if (ResultDataSO == null || ResultDataSO.FinalRankNames.Count == 0) return;

        List<string> winnerNames = ResultDataSO.FinalRankNames;

        for (int i = 0; i < winnerNames.Count; i++)
        {
            if (i >= DisplaySlots.Count) break;

            string pName = winnerNames[i];
            var charData = SpriteDatabase.Find(x => x.CharacterName == pName);

            if (charData != null && charData.RankPortraits.Count > i)
            {
                DisplaySlots[i].sprite = charData.RankPortraits[i];
                DisplaySlots[i].gameObject.SetActive(true);
            }
        }
    }
}