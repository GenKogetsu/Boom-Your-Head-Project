using UnityEngine;
using System.Collections.Generic;

namespace Genoverrei.DesignPattern
{
    public sealed class ObjectPoolManager : Singleton<ObjectPoolManager>
    {
        [Header("Pool Data Configuration")]
        [SerializeField] private List<PoolTableData> _poolTables = new();
        private Dictionary<string, ObjectPool<Transform>> _poolDictionary = new();

        protected override void Awake()
        {
            base.Awake();
            ExecuteInitialize();
        }

        public T Get<T>(string key, Vector3 position, Quaternion rotation) where T : Component
        {
            if (!_poolDictionary.TryGetValue(key, out var pool))
            {
                Debug.LogError($"<b><color=#FF5252>[Pool Get Fail]</color></b> Key: <b>{key}</b> not found! Check PoolTableData.");
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
                // 🚀 สั่งปิดการใช้งานก่อนส่งคืน
                item.gameObject.SetActive(false);
                pool.Return(item.transform);
                Debug.Log($"<b><color=#69F0AE>[Pool Success]</color></b> {item.name} returned to Key: <b>{key}</b>");
            }
            else
            {
                // 🚀 จุดชี้เป็นชี้ตาย: ถ้าหา Key ไม่เจอ เราจะไม่ Destroy ทิ้ง แต่จะฟ้องพี่แทน!
                Debug.LogError($"<b><color=#FF1744>[Pool Release ERROR]</color></b> Key: <b>{key}</b> NOT FOUND for {item.name}! " +
                               "Check if Key matches Prefab Name in PoolTable.");

                // ค้าง Object ไว้ในฉากเพื่อให้พี่คลิกดูชื่อมันได้
                // item.gameObject.SetActive(false); 
            }
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

            // Pre-warm
            int countToPreWarm = Mathf.Min(entry.InitialSize, entry.MaxSize);
            List<Transform> tempStack = new();
            for (int i = 0; i < countToPreWarm; i++) tempStack.Add(pool.Get());
            foreach (var item in tempStack) pool.Return(item);
        }
    }
}