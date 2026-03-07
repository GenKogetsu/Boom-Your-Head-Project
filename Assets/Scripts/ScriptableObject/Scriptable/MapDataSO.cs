using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// <para> (TH) : เก็บข้อมูลจุดเกิดของแผนที่แต่ละด่าน เพื่อให้ PlayerManager สุ่มตำแหน่งได้ </para>
/// <para> (EN) : Stores map spawn point data for PlayerManager to randomize. </para>
/// </summary>
[CreateAssetMenu(fileName = "MapData", menuName = "BombGame/Data/MapData")]
public class MapDataSO : ScriptableObject
{
    [Serializable]
    public struct SpawnSet
    {
        public string SetName;
        public List<Vector3> SpawnPositions;
    }

    [Header("Map Info")]
    public string MapName;

    [Header("Spawn Configurations")]
    [Tooltip("ใส่พิกัดจุดเกิดหลายๆ แบบให้ระบบสุ่มเลือก")]
    public List<SpawnSet> AvailableSpawnSets = new List<SpawnSet>();
}