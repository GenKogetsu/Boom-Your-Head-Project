using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using Genoverrei.DesignPattern;
using Genoverrei.Libary;

namespace BombGame.Manager;

/// <summary>
/// <para> Summary : </para>
/// <para> (TH) : จัดการการทำลายวัตถุบนแผนที่ สุ่มไอเทม และเก็บข้อมูลสถานะ Grid สำหรับระบบอื่น </para>
/// <para> (EN) : Manages map destruction, item drops, and maintains grid status data. </para>
/// </summary>
public sealed class TileManager : Singleton<TileManager>
{
    #region Variable

    [Header("Item Drop Configuration")]
    [Range(0f, 100f)]
    [SerializeField] private float _dropRate = 30f;

    [SerializeField] private List<GameObject> _itemPrefabs = new();

    /// <summary>
    /// <para> Summary : </para>
    /// <para> (TH) : พจนานุกรมเก็บสถานะการเดินผ่านได้ของแต่ละพิกัดบน Grid </para>
    /// <para> (EN) : Dictionary storing the walkability status of each grid coordinate. </para>
    /// </summary>
    private Dictionary<Vector2Int, bool> _walkableMap = new();

    #endregion //Variable


    #region Public Methods

    /// <summary>
    /// <para> Summary : </para>
    /// <para> (TH) : ประมวลผลการทำลาย Tile ตามพิกัดที่ได้รับ พร้อมอัปเดตสถานะ Grid และสุ่มไอเทม </para>
    /// <para> (EN) : Processes tile destruction at the given coordinate, updates grid status, and spawns items. </para>
    /// </summary>
    public void ExecuteProcessDestruction(Vector3Int pos)
    {
        foreach (var map in BombManager.Instance.DestructibleTilemaps)
        {
            if (map.HasTile(pos))
            {
                map.SetTile(pos, null);

                ExecuteUpdateNavigationGrid(pos, true);

                TrySpawnItem(pos);
            }
        }
    }

    /// <summary>
    /// <para> Summary : </para>
    /// <para> (TH) : ตรวจสอบว่าพิกัดที่ระบุสามารถเดินผ่านได้หรือไม่ </para>
    /// <para> (EN) : Checks if the specified coordinate is walkable. </para>
    /// </summary>
    public bool IsWalkable(Vector2Int pos)
    {
        return _walkableMap.TryGetValue(pos, out bool walkable) && walkable;
    }

    #endregion //Public Methods


    #region Private Logic

    /// <summary>
    /// <para> Summary : </para>
    /// <para> (TH) : คำนวณโอกาสและดึงไอเทมจาก Pool มาวางในตำแหน่งที่กำหนด </para>
    /// <para> (EN) : Calculates drop chance and retrieves an item from the pool to spawn at the position. </para>
    /// </summary>
    private void TrySpawnItem(Vector3 pos)
    {
        if (_itemPrefabs.Count == 0) return;

        if (Random.Range(0f, 100f) <= _dropRate)
        {
            var itemIndex = Random.Range(0, _itemPrefabs.Count);

            var _itemPoolKey = _itemPrefabs[itemIndex].name;

            ObjectPoolManager.Instance.Get<Transform>(_itemPoolKey, pos, Quaternion.identity);
        }
    }

    /// <summary>
    /// <para> Summary : </para>
    /// <para> (TH) : บันทึกหรืออัปเดตสถานะพิกัดลงในฐานข้อมูล Grid ภายในคลาส </para>
    /// <para> (EN) : Records or updates the coordinate status in the internal grid database. </para>
    /// </summary>
    private void ExecuteUpdateNavigationGrid(Vector3 pos, bool isWalkable)
    {
        Vector2Int gridPos = new(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y));

        if (_walkableMap.ContainsKey(gridPos))
        {
            _walkableMap[gridPos] = isWalkable;
        }
        else
        {
            _walkableMap.Add(gridPos, isWalkable);
        }
    }

    #endregion //Private Logic
}