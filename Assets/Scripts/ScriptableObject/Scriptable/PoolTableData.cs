using UnityEngine;
using System;
using System.Collections.Generic;

namespace Genoverrei.DesignPattern
{
    /// <summary>
    /// <para> summary_PoolTableData </para>
    /// <para> (TH) : ข้อมูลตารางคลังวัตถุสำหรับกำหนดค่า Prefab และขนาดสูงสุดผ่าน ScriptableObject </para>
    /// <para> (EN) : Pool table data asset for configuring prefabs and max sizes via ScriptableObject. </para>
    /// </summary>
    [CreateAssetMenu(fileName = "PoolTable_", menuName = "BombGame/Data/PoolTable")]
    public sealed class PoolTableData : ScriptableObject
    {
        [Serializable]
        public struct PoolEntry
        {
            public GameObject Prefab;
            public int MaxSize;
        }

        public List<PoolEntry> Entries = new();
    }
}

