using UnityEngine;
using UnityEngine.SceneManagement;
using Genoverrei.DesignPattern;

public class GameFlowManager : Singleton<GameFlowManager>
{
    [SerializeField] private GameSessionData _sessionData;

    [Header("Stage Scenes")]
    [SerializeField] private string[] _stageSceneNames;

    public void StartGame()
    {
        _sessionData.CurrentStageIndex = 0;
        SceneManager.LoadScene(_stageSceneNames[0]);
    }

    public void LoadNextStage()
    {
        _sessionData.CurrentStageIndex++;

        if (_sessionData.CurrentStageIndex >= _stageSceneNames.Length)
        {
            LoadCredits();
            return;
        }

        SceneManager.LoadScene(_stageSceneNames[_sessionData.CurrentStageIndex]);
    }

    public void LoadCredits()
    {
        SceneManager.LoadScene("Credits");
    }

    public void BackToMainMenu()
    {
        _sessionData.ResetSession();
        SceneManager.LoadScene("MainMenu");
    }
}