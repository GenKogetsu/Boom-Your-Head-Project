using UnityEngine;
using System.Collections.Generic;

namespace Genoverrei.DesignPattern
{
    /// <summary>
    /// <para> (TH) : ระบบจัดการ Pool ของ Object ชนิดเดียว รองรับการงอกเกินขีดจำกัดแบบเปอร์เซ็นต์ </para>
    /// <para> (EN) : Single type object pool system supporting percentage-based overflow. </para>
    /// </summary>
    public class ObjectPool<T> where T : Component
    {
        private T _prefab;
        private int _maxSize;
        private int _overPercent;
        private Transform _container;

        private Queue<T> _queue = new Queue<T>();
        private List<T> _activeItems = new List<T>();
        private int _totalCreatedCount = 0;

        public int InactiveCount => _queue.Count;
        public int MaxSize => _maxSize;

        public ObjectPool(T prefab, int maxSize, int overPercent, Transform container)
        {
            _prefab = prefab;
            _maxSize = maxSize;
            _overPercent = overPercent;
            _container = container;
        }

        public T Get()
        {
            T item = null;

            // 1. ♻️ ถ้ามีของในคิว ให้ดึงออกมาใช้ซ้ำ
            if (_queue.Count > 0)
            {
                item = _queue.Dequeue();
                Debug.Log($"<b><color=#FFEB3B>[Pool Reuse]</color></b> ♻️ Dequeued: <b>{_prefab.name}</b> (In Queue: {_queue.Count} | Total: {_totalCreatedCount})");
            }
            // 2. ✨ ถ้าไม่มีของ ให้เช็คว่าสร้างเพิ่มได้ไหม
            else
            {
                // คำนวณขีดจำกัดสูงสุดรวม % ที่อนุญาต
                float bonus = _maxSize * (_overPercent / 100f);
                int finalLimit = _maxSize + Mathf.CeilToInt(bonus);

                if (_totalCreatedCount < finalLimit)
                {
                    item = Object.Instantiate(_prefab, _container);
                    item.name = _prefab.name;
                    _totalCreatedCount++;

                    // เลือกสี Debug ตามสถานะ (ถ้าเริ่มเข้าช่วง Over จะเป็นสีส้มเหลือง)
                    string color = _totalCreatedCount > _maxSize ? "#FFCA28" : "#FF8A65";
                    Debug.Log($"<b><color={color}>[Pool Create]</color></b> ✨ Created NEW: <b>{_prefab.name}</b> ({_totalCreatedCount}/{finalLimit}) [Over: {_overPercent}%]");
                }
                else
                {
                    Debug.LogWarning($"<b><color=#FF1744>[Pool Limit]</color></b> 🛑 {_prefab.name} reached HARD LIMIT ({finalLimit})!");
                    return null;
                }
            }

            if (item != null) _activeItems.Add(item);
            return item;
        }

        public void Return(T item)
        {
            if (item == null) return;
            if (_activeItems.Contains(item)) _activeItems.Remove(item);
            _queue.Enqueue(item);
        }

        /// <summary>
        /// (TH) : ดูด Object ที่กำลังทำงานอยู่ทั้งหมดกลับเข้า Pool (ใช้ตอนเปลี่ยน Scene)
        /// </summary>
        public void ReturnAllActive()
        {
            for (int i = _activeItems.Count - 1; i >= 0; i--)
            {
                T item = _activeItems[i];
                if (item != null)
                {
                    item.gameObject.SetActive(false);
                    _queue.Enqueue(item);
                }
            }
            _activeItems.Clear();
        }

        public void DestroyItem(T item)
        {
            if (item != null)
            {
                if (_activeItems.Contains(item)) _activeItems.Remove(item);
                _totalCreatedCount--;
                Object.Destroy(item.gameObject);
            }
        }
    }
}