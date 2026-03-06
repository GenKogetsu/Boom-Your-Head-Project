using UnityEngine;

/// <summary>
/// <para>Summary :</para>
/// <para>(TH) : คลาสฐานสำหรับสร้าง Singleton Pattern ที่ป้องกันบัคการสร้าง Object ใหม่ตอนปิดเกม</para>
/// <para>(EN) : Base class for implementing the Singleton Pattern, protected against quitting ghost objects.</para>
/// </summary>
public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    #region Fields

    private static T _instance;
    private static readonly object _lock = new();

    // 🚀 เพิ่มตัวแปรเช็คว่าเกมกำลังปิดอยู่หรือไม่
    private static bool _applicationIsQuitting = false;

    #endregion // Fields

    #region Properties

    /// <summary>
    /// <para>Summary :</para>
    /// <para>(TH) : เข้าถึง Instance หลักของคลาสนี้ (Global Access Point)</para>
    /// </summary>
    public static T Instance
    {
        get
        {
            // 🚀 ถ้าเกมกำลังปิด ห้ามสร้าง Instance ใหม่เด็ดขาด ให้คืนค่า null กลับไปเลย
            if (_applicationIsQuitting)
            {
                return null;
            }

            lock (_lock)
            {
                if (_instance != null) return _instance;

                _instance = (T)Object.FindFirstObjectByType(typeof(T));

                if (_instance != null) return _instance;

                GameObject singletonObject = new();
                _instance = singletonObject.AddComponent<T>();
                singletonObject.name = $"{typeof(T)} (Singleton)";

                return _instance;
            }
        }
    }

    #endregion // Properties

    #region Unity Lifecycle

    /// <summary>
    /// <para>Summary :</para>
    /// <para>(TH) : จัดการ Lifecycle เมื่อ Object ตื่นขึ้น เพื่อป้องกันการเกิด Duplicate Instance</para>
    /// </summary>
    protected virtual void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this as T;

        if (transform.parent != null) transform.SetParent(null);

        DontDestroyOnLoad(gameObject);
    }

    // 🚀 เพิ่มฟังก์ชันนี้เพื่อจับจังหวะตอนกด Stop Play
    protected virtual void OnApplicationQuit()
    {
        _applicationIsQuitting = true;
    }

    // 🚀 ป้องกันกรณี Object โดนทำลายไปก่อนแล้วมีคนพยายามเรียกใช้
    protected virtual void OnDestroy()
    {
        if (_instance == this)
        {
            _applicationIsQuitting = true;
        }
    }

    #endregion // Unity Lifecycle
}