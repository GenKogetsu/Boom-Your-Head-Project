using UnityEngine;
using System.Collections.Generic;
using Genoverrei.DesignPattern;


namespace Genoverrei.Libary;

/// <summary>
/// <para> summary_ObjectPoolManager </para>
/// <para> (TH) : ตัวจัดการคลังวัตถุส่วนกลางที่รองรับการโหลดข้อมูลจากหลายตาราง (Multi-Table) </para>
/// <para> (EN) : Central pool manager supporting data loading from multiple tables. </para>
/// </summary>
public sealed class ObjectPoolManager : Singleton<ObjectPoolManager>
{
    #region Variable

    [Header("Pool Data Configuration")]
    [SerializeField] private List<PoolTableData> _poolTables = new();

    private Dictionary<string, ObjectPool<Transform>> _poolDictionary = new();

    #endregion //Variable

    #region Unity Lifecycle

    protected override void Awake()
    {
        base.Awake();
        ExecuteInitialize();
    }

    #endregion //Unity Lifecycle

    #region Public Methods

    /// <summary>
    /// <para> summary : </para>
    /// <para> (TH) : ดึงวัตถุออกจากคลังตามคีย์ที่กำหนด </para>
    /// <para> (EN) : Retrieves an object from the pool using the specified key. </para>
    /// </summary>
    public T Get<T>(string key, Vector3 position, Quaternion rotation) where T : Component
    {
        if (!_poolDictionary.TryGetValue(key, out var pool))
        {
#if UNITY_EDITOR
            Debug.LogWarning($"<b><color=#FF7043>[PoolManager]</color></b> Key: {key} not found in any PoolTables!");
#endif
            return null;
        }

        Transform item = pool.Get();
        item.SetPositionAndRotation(position, rotation);

        return item.GetComponent<T>();
    }

    /// <summary>
    /// <para> summary : </para>
    /// <para> (TH) : ส่งวัตถุกลับเข้าคลังตามคีย์ที่ระบุ </para>
    /// <para> (EN) : Returns an object to its corresponding pool using the key. </para>
    /// </summary>
    public void Release(string key, Component item)
    {
        if (item == null) return;

        if (_poolDictionary.TryGetValue(key, out var pool))
        {
            pool.Return(item.transform);
        }
        else
        {
            Destroy(item.gameObject);
        }
    }

    #endregion //Public Methods

    #region Private Logic

    /// <summary>
    /// <para> summary : </para>
    /// <para> (TH) : วนลูปอ่านข้อมูลจากทุกตารางในลิสต์เพื่อสร้าง Pool </para>
    /// <para> (EN) : Iterates through all tables in the list to initialize pools. </para>
    /// </summary>
    private void ExecuteInitialize()
    {
        if (_poolTables == null || _poolTables.Count == 0) return;

        foreach (var table in _poolTables)
        {
            if (table == null) continue;

            foreach (var entry in table.Entries)
            {
                if (entry.Prefab == null || string.IsNullOrEmpty(entry.Key)) continue;

                // ตรวจสอบเพื่อไม่ให้สร้าง Key ซ้ำกันระหว่างตาราง
                if (_poolDictionary.ContainsKey(entry.Key))
                {
                    Debug.LogWarning($"<b><color=#FFB74D>[PoolManager]</color></b> Duplicate Key: {entry.Key} found in {table.name}. Skipping...");
                    continue;
                }

                ExecuteCreatePool(entry);
            }
        }
    }

    private void ExecuteCreatePool(PoolTableData.PoolEntry entry)
    {
        Transform container = new GameObject($"Pool_{entry.Key}").transform;
        container.SetParent(transform);

        var pool = new ObjectPool<Transform>(entry.Prefab.transform, entry.MaxSize, container);
        _poolDictionary.Add(entry.Key, pool);
    }

    #endregion //Private Logic
}