using UnityEngine;
using System.Collections.Generic;

namespace Genoverrei.DesignPattern
{
    /// <summary>
    /// <para> (TH) : ศูนย์กลางจัดการ Object Pool ทั้งหมดในเกม คอยเริ่มการทำงานและกระจายคำสั่ง Get/Release </para>
    /// <para> (EN) : Central Object Pool manager handling initialization and Get/Release requests. </para>
    /// </summary>
    public sealed class ObjectPoolManager : Singleton<ObjectPoolManager>
    {
        [Header("Pool Data Configuration")]
        [SerializeField] private List<PoolTableData> _poolTables = new List<PoolTableData>();

        private Dictionary<string, ObjectPool<Transform>> _poolDictionary = new Dictionary<string, ObjectPool<Transform>>();

        protected override void Awake()
        {
            base.Awake();
            ExecuteInitialize();
        }

        /// <summary>
        /// (TH) : ดึง Object จาก Pool ออกมาใช้งานตามชื่อ Key (ชื่อ Prefab)
        /// </summary>
        public T Get<T>(string key, Vector3 position, Quaternion rotation) where T : Component
        {
            if (!_poolDictionary.TryGetValue(key, out var pool))
            {
                Debug.LogError($"<b><color=#FF5252>[Pool Get Fail]</color></b> Key: <b>{key}</b> not found!");
                return null;
            }

            Transform item = pool.Get();
            if (item == null) return null;

            item.SetPositionAndRotation(position, rotation);
            item.gameObject.SetActive(true);
            return item.GetComponent<T>();
        }

        /// <summary>
        /// (TH) : ส่งคืน Object กลับเข้า Pool เมื่อเลิกใช้งาน
        /// </summary>
        public void Release(string key, Component item)
        {
            if (item == null) return;

            if (_poolDictionary.TryGetValue(key, out var pool))
            {
                item.gameObject.SetActive(false);

                // ถ้าในคิวเต็มเกิน MaxSize แล้ว ให้ทำลายทิ้งเพื่อไม่ให้ Memory บวม
                if (pool.InactiveCount >= pool.MaxSize)
                {
                    Debug.Log($"<b><color=#F06292>[Pool Capacity Control]</color></b> 🔥 <b>{key}</b> เกินค่า Max ({pool.MaxSize}) ทำลายทิ้งทันที");
                    pool.DestroyItem(item.transform);
                }
                else
                {
                    pool.Return(item.transform);
                    Debug.Log($"<b><color=#69F0AE>[Pool Return]</color></b> ✅ <b>{key}</b> returned to pool.");
                }
            }
            else
            {
                Debug.LogWarning($"<b><color=#FF1744>[Pool Error]</color></b> Key <b>{key}</b> not found during release! Destroying object.");
                Destroy(item.gameObject);
            }
        }

        /// <summary>
        /// (TH) : เรียกใช้เพื่อเคลียร์ Object ทั้งหมดในฉากกลับเข้า Pool (แนะนำให้เรียกก่อนโหลด Scene ใหม่)
        /// </summary>
        public void ReleaseAllPools()
        {
            foreach (var kvp in _poolDictionary)
            {
                kvp.Value.ReturnAllActive();
            }
            Debug.Log("<b><color=#FFEB3B>[PoolManager]</color></b> 🧹 ดูดทุก Object กลับเข้า Pool เรียบร้อย!");
        }

        private void ExecuteInitialize()
        {
            foreach (var table in _poolTables)
            {
                if (table == null) continue;
                foreach (var entry in table.Entries)
                {
                    if (entry.Prefab == null) continue;

                    string key = entry.Prefab.name;
                    if (_poolDictionary.ContainsKey(key)) continue;

                    GameObject containerObj = new GameObject($"Pool_{key}");
                    containerObj.transform.SetParent(transform);

                    var pool = new ObjectPool<Transform>(
                        entry.Prefab.transform,
                        entry.MaxSize,
                        entry.LimitOverPercent,
                        containerObj.transform
                    );

                    _poolDictionary.Add(key, pool);

                    // --- แก้ไขจุดนี้ครับพี่ ---
                    int countToPreWarm = Mathf.Min(entry.InitialSize, entry.MaxSize);
                    List<Transform> tempStack = new List<Transform>();

                    for (int i = 0; i < countToPreWarm; i++)
                    {
                        Transform item = pool.Get();
                        if (item != null)
                        {
                            // 🚀 สั่งปิดการทำงานก่อนส่งคืนเข้าคิว
                            item.gameObject.SetActive(false);
                            tempStack.Add(item);
                        }
                    }

                    foreach (var item in tempStack)
                    {
                        pool.Return(item);
                    }
                    // -------------------------

                    Debug.Log($"<b><color=#4FC3F7>[Pool Initialized]</color></b> 🚀 <b>{key}</b> (Init: {countToPreWarm} | Max: {entry.MaxSize})");
                }
            }
        }
    }
}