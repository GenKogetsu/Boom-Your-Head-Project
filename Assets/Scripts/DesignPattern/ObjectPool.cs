using UnityEngine;
using System.Collections.Generic;

namespace Genoverrei.DesignPattern
{
    public class ObjectPool<T> where T : Component
    {
        private T _prefab;
        private int _maxSize;
        private Transform _container;
        private Queue<T> _queue = new Queue<T>();

        // 📊 เก็บสถิติจำนวน Instance ทั้งหมดที่ยังไม่โดน Destroy
        private int _totalCreatedCount = 0;

        public int InactiveCount => _queue.Count;
        public int MaxSize => _maxSize;

        public ObjectPool(T prefab, int maxSize, Transform container)
        {
            _prefab = prefab;
            _maxSize = maxSize;
            _container = container;
        }

        public T Get()
        {
            // --- แบบที่ 1: เรียกของเก่ามาใช้งาน (Reuse) ---
            if (_queue.Count > 0)
            {
                T reuseItem = _queue.Dequeue();
                Debug.Log($"<b><color=#FFEB3B>[Pool Reuse]</color></b> ♻️ Dequeued: <b>{_prefab.name}</b> (In Queue: {_queue.Count} | Total: {_totalCreatedCount})");
                return reuseItem;
            }

            // --- แบบที่ 2: สร้างใหม่เมื่อของไม่พอ (Create) ---
            // 🚀 เช็คขีดจำกัดหยวนให้ 10% (Soft Limit)
            int softLimit = _maxSize + Mathf.CeilToInt(_maxSize * 0.1f);

            if (_totalCreatedCount < softLimit)
            {
                T newItem = Object.Instantiate(_prefab, _container);
                newItem.name = _prefab.name;
                _totalCreatedCount++; // นับยอดรวมสะสม

                Debug.Log($"<b><color=#FF8A65>[Pool Create]</color></b> ✨ Created NEW: <b>{_prefab.name}</b> (Current: {_totalCreatedCount}/{softLimit})");
                return newItem;
            }

            // 🛑 กรณีเกิน 110% (Hard Limit)
            Debug.LogWarning($"<b><color=#FF1744>[Pool Hard Limit]</color></b> 🛑 <b>{_prefab.name}</b> เกินขีดจำกัด 110% ({softLimit}) แล้ว! กรุณาเพิ่ม MaxSize หรือรอของคืน Pool");
            return null;
        }

        public void Return(T item)
        {
            _queue.Enqueue(item);
        }

        public void DestroyItem(T item)
        {
            if (item != null)
            {
                _totalCreatedCount--; // ลดจำนวนนับรวมเมื่อวัตถุถูกทำลายทิ้งจริงๆ
                Object.Destroy(item.gameObject);
            }
        }
    }
}