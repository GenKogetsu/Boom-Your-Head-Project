using UnityEngine;
using System;
using System.Collections.Generic;

namespace Genoverrei.DesignPattern
{
    [CreateAssetMenu(fileName = "PoolTable_", menuName = "BombGame/Data/PoolTable")]
    public sealed class PoolTableData : ScriptableObject
    {
        [Serializable]
        public struct PoolEntry
        {
            public GameObject Prefab;

            [Tooltip("จำนวนที่ต้องการให้สร้างรอไว้ทันทีเมื่อเริ่มเกม")]
            public int InitialSize;

            [Tooltip("ขนาดสูงสุดของ Pool")]
            public int MaxSize;
        }

        public List<PoolEntry> Entries = new();
    }
}