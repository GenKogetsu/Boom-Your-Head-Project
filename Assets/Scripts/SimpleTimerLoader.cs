using UnityEngine;
using UnityEngine.SceneManagement;
using Genoverrei.DesignPattern;

public class MultiSceneTimerLoader : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float _delaySeconds = 3.0f;

    [Header("Scene Sequence")]
    [SerializeField] private string _firstScene = "SceneA";

    private void Start()
    {
        Invoke(nameof(LoadNextScene), _delaySeconds);
    }

    private void LoadNextScene()
    {
        // โหลดฉาก
        if (SceneEffectController.Instance != null)
        {
            SceneEffectController.Instance.LoadSceneAndPlayEffect(_firstScene);
        }
        else
        {
            SceneManager.LoadScene(_firstScene);
        }
    }

}