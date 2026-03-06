using UnityEngine;
using System.Collections.Generic;

namespace Genoverrei.DesignPattern
{
    public sealed class ObjectPoolManager : Singleton<ObjectPoolManager>
    {
        #region Variable
        [Header("Pool Data Configuration")]
        [SerializeField] private List<PoolTableData> _poolTables = new();

        private Dictionary<string, ObjectPool<Transform>> _poolDictionary = new();
        #endregion

        #region Unity Lifecycle
        protected override void Awake()
        {
            base.Awake();
            ExecuteInitialize();
        }
        #endregion

        #region Public Methods
        public T Get<T>(string key, Vector3 position, Quaternion rotation) where T : Component
        {
            if (!_poolDictionary.TryGetValue(key, out var pool))
            {
#if UNITY_EDITOR
                Debug.LogWarning($"<b><color=#FF7043>[PoolManager]</color></b> Key: {key} (Prefab Name) not found in any PoolTables!");
#endif
                return null;
            }

            Transform item = pool.Get();
            item.SetPositionAndRotation(position, rotation);
            return item.GetComponent<T>();
        }

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
        #endregion

        #region Private Logic
        private void ExecuteInitialize()
        {
            if (_poolTables == null || _poolTables.Count == 0) return;

            foreach (var table in _poolTables)
            {
                if (table == null) continue;

                foreach (var entry in table.Entries)
                {
                    // 🚀 เปลี่ยนมาใช้ชื่อของ Prefab เป็น Key
                    if (entry.Prefab == null) continue;
                    string key = entry.Prefab.name;

                    if (_poolDictionary.ContainsKey(key))
                    {
                        Debug.LogWarning($"<b><color=#FFB74D>[PoolManager]</color></b> Duplicate Prefab Name: {key} found in {table.name}. Skipping...");
                        continue;
                    }

                    ExecuteCreatePool(entry, key);
                }
            }
        }

        private void ExecuteCreatePool(PoolTableData.PoolEntry entry, string key)
        {
            GameObject containerObj = new GameObject($"Pool_{key}");
            containerObj.transform.SetParent(transform);

            var pool = new ObjectPool<Transform>(entry.Prefab.transform, entry.MaxSize, containerObj.transform);
            _poolDictionary.Add(key, pool);
        }
        #endregion
    }
}