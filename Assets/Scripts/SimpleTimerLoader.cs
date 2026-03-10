using UnityEngine;
using UnityEngine.SceneManagement;
using Genoverrei.DesignPattern;

public class MultiSceneTimerLoader : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float _delaySeconds = 3.0f;

    [Header("Scene Sequence")]
    [SerializeField] private string _firstScene = "SceneA";

    // 🚀 ใช้ static เพื่อให้ค่าคงอยู่แม้จะเปลี่ยน Scene ไปแล้ว
    private static int _loadCount = 0;

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

    // แถม: ฟังก์ชันเอาไว้ Reset ค่าใหม่ถ้าต้องการ (เช่น กลับหน้าจบท้ายสุด)
    public static void ResetLoadCount()
    {
        _loadCount = 0;
    }
}