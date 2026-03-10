using UnityEngine;
using UnityEngine.SceneManagement;
using Genoverrei.DesignPattern;

public class MultiSceneTimerLoader : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float _delaySeconds = 3.0f;

    [Header("Scene Sequence")]
    [SerializeField] private string _firstScene = "SceneA";
    [SerializeField] private string _secondScene = "SceneB";

    // 🚀 ใช้ static เพื่อให้ค่าคงอยู่แม้จะเปลี่ยน Scene ไปแล้ว
    private static int _loadCount = 0;

    private void Start()
    {
        Invoke(nameof(LoadNextScene), _delaySeconds);
    }

    private void LoadNextScene()
    {
        string targetScene = "";

        // เช็คว่าครั้งที่เท่าไหร่
        if (_loadCount == 0)
        {
            targetScene = _firstScene;
            _loadCount = 1; // เปลี่ยนเป็น 1 สำหรับครั้งหน้า
        }
        else
        {
            targetScene = _secondScene;
            // _loadCount = 0; // ถ้าอยากให้มันวนลูปกลับไปที่ 1 ใหม่ ให้ปลดคอมเมนต์บรรทัดนี้
        }

        Debug.Log($"<b>[TimerLoader]</b> Loading {targetScene} (Attempt: {_loadCount})");

        // โหลดฉาก
        if (SceneEffectController.Instance != null)
        {
            SceneEffectController.Instance.LoadSceneAndPlayEffect(targetScene);
        }
        else
        {
            SceneManager.LoadScene(targetScene);
        }
    }

    // แถม: ฟังก์ชันเอาไว้ Reset ค่าใหม่ถ้าต้องการ (เช่น กลับหน้าจบท้ายสุด)
    public static void ResetLoadCount()
    {
        _loadCount = 0;
    }
}