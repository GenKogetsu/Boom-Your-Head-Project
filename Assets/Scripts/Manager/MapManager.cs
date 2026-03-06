using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using Genoverrei.DesignPattern;

/// <summary>
/// <para> Summary : </para>
/// <para> (TH) : ศูนย์จัดการแผนที่ จัดการการทำลายกล่องและการสุ่มดรอปไอเทมผ่านระบบ Object Pool </para>
/// </summary>
public sealed class MapManager : MonoBehaviour
{
    #region Variable
    [Header("Observer Channels")]
    [SerializeField] private MapChannelSO _mapChannel;
    [SerializeField] private BombChannelSO _bombChannel;

    [Header("Tilemaps")]
    [Tooltip("ใส่เลเยอร์กำแพงทึบ")]
    [SerializeField] private List<Tilemap> _solidTilemaps = new();

    [Tooltip("ใส่เลเยอร์ของพังได้ (กล่อง)")]
    [SerializeField] private List<Tilemap> _destructibleTilemaps = new();

    [Header("Item Drop Settings")]
    [Range(0, 100)]
    [SerializeField] private float _dropChance = 25f;

    [Tooltip("ลาก Prefab ไอเทมมาใส่ที่นี่ (ชื่อ Prefab ต้องตรงกับ Key ใน PoolManager)")]
    [SerializeField] private List<GameObject> _itemPrefabs = new();

    private HashSet<Vector2Int> _dangerousCells = new();
    #endregion

    #region Unity Lifecycle
    private void OnEnable()
    {
        if (_mapChannel != null)
        {
            _mapChannel.OnCheckWalkable = IsWalkable;
            _mapChannel.OnCheckSolid = IsSolid;
            _mapChannel.OnCheckDestructible = IsDestructible;
            _mapChannel.OnRequestDestruction = (gridPos, intruder) => ExecuteProcessDestruction(gridPos);
            _mapChannel.OnAddDanger = AddDangerZone;
            _mapChannel.OnRemoveDanger = RemoveDangerZone;
        }

        if (_bombChannel != null)
            _bombChannel.OnExplosionHit += ExecuteHandleFlameCollision;
    }

    private void OnDisable()
    {
        if (_mapChannel != null) _mapChannel.Clear();
        if (_bombChannel != null) _bombChannel.OnExplosionHit -= ExecuteHandleFlameCollision;
    }
    #endregion

    #region Core Logic

    public bool IsWalkable(Vector2Int gridPos)
    {
        return !IsSolid(gridPos) && !IsDestructible(gridPos);
    }

    public bool IsSolid(Vector2Int gridPos)
    {
        Vector3 worldPos = new Vector3(gridPos.x, gridPos.y, 0);
        foreach (var map in _solidTilemaps)
        {
            if (map != null)
            {
                Vector3Int cellPos = map.WorldToCell(worldPos);
                if (map.HasTile(cellPos)) return true;
            }
        }
        return false;
    }

    public bool IsDestructible(Vector2Int gridPos)
    {
        Vector3 worldPos = new Vector3(gridPos.x, gridPos.y, 0);
        foreach (var map in _destructibleTilemaps)
        {
            if (map != null)
            {
                Vector3Int cellPos = map.WorldToCell(worldPos);
                if (map.HasTile(cellPos)) return true;
            }
        }
        return false;
    }

    public void ExecuteProcessDestruction(Vector3Int gridPos)
    {
        Vector3 worldPos = new Vector3(gridPos.x, gridPos.y, 0);

        foreach (var map in _destructibleTilemaps)
        {
            if (map != null)
            {
                Vector3Int targetCell = map.WorldToCell(worldPos);
                if (map.HasTile(targetCell))
                {
                    // 1. ลบ Tile กล่องออก
                    map.SetTile(targetCell, null);
                    RemoveDangerZone((Vector2Int)targetCell);

                    // 2. 🚀 ดรอปไอเทมโดยคำนวณตำแหน่งกึ่งกลางช่อง
                    TryDropItem(map.GetCellCenterWorld(targetCell));

                    Debug.Log($"<color=orange>[MapManager]</color> Destroyed at {targetCell}");
                }
            }
        }
    }

    #endregion

    #region Private Logic

    /// <summary>
    /// (TH) : สุ่มดรอปไอเทมผ่าน ObjectPoolManager
    /// </summary>
    private void TryDropItem(Vector3 spawnPos)
    {
        if (_itemPrefabs == null || _itemPrefabs.Count == 0) return;

        float roll = Random.Range(0f, 100f);
        if (roll <= _dropChance)
        {
            int randomIndex = Random.Range(0, _itemPrefabs.Count);
            GameObject selectedPrefab = _itemPrefabs[randomIndex];

            if (selectedPrefab != null)
            {
                // 🚀 ดึงไอเทมออกมาจาก Pool โดยใช้ชื่อ Prefab เป็น Key
                ObjectPoolManager.Instance.Get<Transform>(selectedPrefab.name, spawnPos, Quaternion.identity);
            }
        }
    }

    private void AddDangerZone(Vector2Int gridPos) => _dangerousCells.Add(gridPos);
    private void RemoveDangerZone(Vector2Int gridPos) => _dangerousCells.Remove(gridPos);
    private void ExecuteHandleFlameCollision(Vector3Int gridPos, Collider2D intruder) => ExecuteProcessDestruction(gridPos);

    #endregion
}