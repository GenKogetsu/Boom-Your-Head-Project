namespace Genoverrei.DesignPattern;

/// <summary>
/// <para>Summary :</para>
/// <para>(TH) : คลาสฐานสำหรับสร้าง Singleton Pattern เพื่อให้เข้าถึง Instance ได้จากทั่วทั้ง Project</para>
/// <para>(EN) : Base class for implementing the Singleton Pattern to provide global access to an instance.</para>
/// </summary>

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    #region Fields

    private static T _instance;
    private static readonly object _lock = new();

    #endregion // Fields

    #region Properties

    /// <summary>
    /// <para>Summary :</para>
    /// <para>(TH) : เข้าถึง Instance หลักของคลาสนี้ (Global Access Point)</para>
    /// <para>(EN) : Access the main instance of this class (Global Access Point).</para>
    /// </summary>

    public static T Instance
    {
        get
        {
            lock (_lock)
            {
                if (_instance != null) return _instance;

                _instance = (T)UnityEngine.Object.FindFirstObjectByType(typeof(T));

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
    /// <para>(EN) : Manages the lifecycle when the object awakes to prevent duplicate instances.</para>
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

    #endregion // Unity Lifecycle
}