using UnityEngine;
using Genoverrei.DesignPattern;
using NaughtyAttributes;

/// <summary>
/// <para> (TH) : สคริปต์ลงทะเบียนตัวเองเข้า Registry ในรูปแบบ GameObject เพื่อความเสถียรสูงสุด </para>
/// </summary>
[RequireComponent(typeof(StatsController))]
public sealed class CharacterAutoRegister : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private CharacterRegistrySO _registry;

    [Header("Runtime Cache")]
    [SerializeField][ReadOnly] private StatsController _stats;

    private void Awake()
    {
        // 🚀 เตรียม Stats ไว้ดึงค่าชื่อ (LivingName)
        if (_stats == null) _stats = GetComponent<StatsController>();
    }

    private void OnValidate()
    {
        if (_stats == null) _stats = GetComponent<StatsController>();
    }

    private void Start()
    {
        // 🚀 ลงทะเบียนเมื่อเริ่ม
        ExecuteRegister();
    }

    private void OnEnable()
    {
        // 🚀 รองรับ Object Pool (ถ้าเกิดใหม่ให้ลงทะเบียนซ้ำ)
        if (Time.timeSinceLevelLoad > 0.1f)
        {
            ExecuteRegister();
        }
    }

    private void ExecuteRegister()
    {
        if (_registry == null)
        {
            Debug.LogError($"<b><color=red>[Registry Error]</color></b> {name}: ลืมลาก RegistrySO ใส่ใน Inspector!");
            return;
        }

        if (_stats != null && _stats.LivingName != Character.None)
        {
            // 🚀 ส่งตัว "gameObject" (obj) เข้าไปเก็บใน Registry ตรงๆ
            // วิธีนี้ Unity จะไม่ฟ้อง Type Mismatch เพราะเป็นการเก็บ Ref ของ GameObject
            _registry.Register(_stats.LivingName, this.gameObject);

            Debug.Log($"<b><color=#69F0AE>[Registry Success]</color></b> 🛡️ {name} ลงทะเบียนเป็น obj ของ ID: <b>{_stats.LivingName}</b>");
        }
        else
        {
            Debug.LogWarning($"<b><color=yellow>[Registry Warning]</color></b> {name}: LivingName เป็น None หรือหา Stats ไม่เจอ!");
        }
    }
}