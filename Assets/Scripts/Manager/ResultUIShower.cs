using UnityEngine;
using TMPro;
using System.IO;
using System.Collections.Generic;

public class ResultUIShower : MonoBehaviour
{
    [Header("UI Display (1st - 4th)")]
    public List<TextMeshProUGUI> RankNameTexts; // ลาก Text ที่จะโชว์ชื่ออันดับ 1-4 มาใส่เรียงกัน

    [Header("Character Models (1st - 4th)")]
    public List<Animator> RankAnimators; // ลาก Animator ของโมเดลที่จะมายืนโชว์ตัวอันดับ 1-4

    private string _resultPath;
    private string _configPath;

    private void Start()
    {
        _resultPath = Path.Combine(Application.dataPath, "SaveData/RankResult.txt");
        _configPath = Path.Combine(Application.dataPath, "SaveData/RankConfig.txt");

        ShowResults();
    }

    public void ShowResults()
    {
        if (!File.Exists(_resultPath) || !File.Exists(_configPath))
        {
            Debug.LogError("หาไฟล์ผลการแข่งหรือไฟล์คอนฟิกไม่เจอ!");
            return;
        }

        // 1. อ่านไฟล์ผลการแข่ง (รายชื่อที่ 1 - 4)
        string[] winnerNames = File.ReadAllLines(_resultPath);

        // 2. อ่านไฟล์คอนฟิกท่าทาง (0, 1, 2)
        string[] animIndices = File.ReadAllLines(_configPath);

        for (int i = 0; i < winnerNames.Length; i++)
        {
            // โชว์ชื่อบน UI ตามอันดับ
            if (i < RankNameTexts.Count && RankNameTexts[i] != null)
            {
                RankNameTexts[i].text = winnerNames[i];
            }

            // สั่งโมเดลเล่นท่าตามที่อ่านจาก Config
            if (i < RankAnimators.Count && RankAnimators[i] != null)
            {
                int pose = int.Parse(animIndices[i]);
                RankAnimators[i].SetInteger("PoseIndex", pose);

                // แถม: ถ้าพี่อยากโชว์ชื่อเหนือหัวโมเดลด้วย ก็ทำตรงนี้ได้เลย
                Debug.Log($"อันดับ {i + 1}: {winnerNames[i]} เล่นท่าเบอร์ {pose}");
            }
        }
    }
}