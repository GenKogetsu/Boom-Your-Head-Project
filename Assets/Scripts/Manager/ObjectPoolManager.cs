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
            if (!_poolDictionary.TryGetValue(key, out var pool)) return null;

            // 🚀 ระบบหยวน 10%: คำนวณขีดจำกัดสูงสุดจริงๆ (Max + 10%)
            int softLimit = pool.MaxSize + Mathf.CeilToInt(pool.MaxSize * 0.1f);

            // เช็คว่าปัจจุบันมีของในระบบ (Active + Inactive) เกิน Soft Limit หรือยัง
            // (หมายเหตุ: ในที่นี้เราเช็คว่าถ้า Queue ว่าง และเรากำลังจะงอกใหม่)
            if (pool.InactiveCount == 0)
            {
                // ถ้าพี่อยากคุมเข้มไม่ให้งอกเกิน 10% จริงๆ ต้องเก็บนับ TotalInstance ไว้ด้วย 
                // แต่เบื้องต้นผมจะให้มันงอกได้ แล้วไป "เตะออก" ตอน Release แทนครับ
            }

            Transform item = pool.Get();
            item.SetPositionAndRotation(position, rotation);
            item.gameObject.SetActive(true);

            return item.GetComponent<T>();
        }

        public void Release(string key, Component item)
        {
            if (item == null) return;

            if (_poolDictionary.TryGetValue(key, out var pool))
            {
                item.gameObject.SetActive(false);

                // 🚀 ระบบคุมกำเนิด: ถ้าของที่ส่งคืนมา ทำให้ของในคลัง (Inactive) เกิน MaxSize
                // ให้ทำลายทิ้งทันที เพื่อรักษาจำนวนให้คงที่ตามที่ตั้งค่าไว้
                if (pool.InactiveCount >= pool.MaxSize)
                {
                    Debug.Log($"<b><color=#F06292>[Pool Capacity Control]</color></b> 🔥 <b>{key}</b> เกินค่า Max ({pool.MaxSize}) ทำลายทิ้งเพื่อลดภาระเครื่อง");
                    pool.DestroyItem(item.transform);
                }
                else
                {
                    pool.Return(item.transform);
                    Debug.Log($"<b><color=#69F0AE>[Pool Success]</color></b> ✅ {item.name} กลับเข้าคลัง (ในคลังมี: {pool.InactiveCount}/{pool.MaxSize})");
                }
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

            int countToPreWarm = Mathf.Min(entry.InitialSize, entry.MaxSize);
            List<Transform> tempStack = new();
            int i = 0;
            for (i = 0; i < countToPreWarm; i++) tempStack.Add(pool.Get());
            foreach (var item in tempStack) pool.Return(item);

            Debug.Log($"<b><color=#4FC3F7>[Pool Warm-up]</color></b> ❄️ Created <b>{countToPreWarm}</b> instances for Key: <b>{key} {i}items.</b>");
        }
    }
}