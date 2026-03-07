using UnityEngine;
using UnityEngine.SceneManagement;
using Genoverrei.DesignPattern;
using Genoverrei.Manager; // 🚀 เพิ่มเพื่อเรียกหา PlayerManager

/// <summary>
/// <para> (TH) : ตัวจัดการลำดับการเล่นของเกม การเปลี่ยนฉาก และการล้างข้อมูลข้ามด่าน </para>
/// <para> (EN) : Manages game flow, scene transitions, and cross-stage data cleanup. </para>
/// </summary>
public class GameFlowManager : Singleton<GameFlowManager>
{
    [SerializeField] private GameSessionDataSO _sessionData;

    [Header("Stage Scenes")]
    [SerializeField] private string[] _stageSceneNames;

    public void StartGame()
    {
        if (_sessionData == null) return;

        _sessionData.CurrentStageIndex = 0;
        ExecuteLoadStage(_stageSceneNames[0]);
    }

    public void LoadNextStage()
    {
        if (_sessionData == null) return;

        _sessionData.CurrentStageIndex++;

        if (_sessionData.CurrentStageIndex >= _stageSceneNames.Length)
        {
            LoadCredits();
            return;
        }

        ExecuteLoadStage(_stageSceneNames[_sessionData.CurrentStageIndex]);
    }

    public void LoadCredits()
    {
        SceneManager.LoadScene("Credits");
    }

    public void BackToMainMenu()
    {
        if (_sessionData != null) _sessionData.ResetSession();
        SceneManager.LoadScene("MainMenu");
    }

    /// <summary>
    /// <para> (TH) : ฟังก์ชันภายในสำหรับจัดการการเปลี่ยนฉากแบบคลีนๆ </para>
    /// </summary>
    private void ExecuteLoadStage(string sceneName)
    {
        // 🚀 ก่อนเปลี่ยนฉาก สั่งให้ PlayerManager ดูดของกลับ Pool ให้หมด (ถ้ามีอยู่ในฉากเก่า)
        if (ObjectPoolManager.Instance != null)
        {
            ObjectPoolManager.Instance.ReleaseAllPools();
        }

        Debug.Log($"<b><color=#69F0AE>[GameFlow]</color></b> 🚚 Moving to Scene: <b>{sceneName}</b>");
        SceneManager.LoadScene(sceneName);
    }
}