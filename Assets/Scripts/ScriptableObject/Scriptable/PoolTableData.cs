using UnityEngine;
using System;
using System.Collections.Generic;

namespace Genoverrei.DesignPattern
{
    [CreateAssetMenu(fileName = "PoolTable_", menuName = "BombGame/PoolTable")]
    public sealed class PoolTableData : ScriptableObject
    {
        [Serializable]
        public struct PoolEntry
        {
            public GameObject Prefab;
            public int InitialSize;
            public int MaxSize;
            public int LimitOverPercent;

            // 🚀 สร้าง Constructor เพื่อกำหนดค่าเริ่มต้น
            // วิธีเรียกใช้ในโค้ด: var entry = new PoolEntry(myPrefab);
            public PoolEntry(GameObject prefab)
            {
                Prefab = prefab;
                InitialSize = 5;      // 🟢 ค่าเริ่มต้นที่พี่อยากได้
                MaxSize = 10;         // 🟢 ค่าเริ่มต้นที่พี่อยากได้
                LimitOverPercent = 10; // 🟢 ค่าเริ่มต้นที่พี่อยากได้
            }
        }

        public List<PoolEntry> Entries = new List<PoolEntry>();
    }
}