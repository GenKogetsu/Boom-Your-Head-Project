using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using Genoverrei.DesignPattern;

/// <summary>
/// <para> (TH) : ศูนย์จัดการแผนที่ แก้ปัญหาพิกัดเบี้ยวจาก Offset และปัญหาชนอากาศ </para>
/// </summary>
public sealed class MapManager : MonoBehaviour
{
    [Header("Observer Channels")]
    [SerializeField] private MapChannelSO _mapChannel;
    [SerializeField] private BombChannelSO _bombChannel;

    [Header("Tilemaps")]
    [Tooltip("ใส่เลเยอร์กำแพงทึบ")]
    [SerializeField] private List<Tilemap> _solidTilemaps = new();

    [Tooltip("ใส่เลเยอร์ของพังได้")]
    [SerializeField] private List<Tilemap> _destructibleTilemaps = new();

    private HashSet<Vector2Int> _dangerousCells = new();

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

    #region Core Logic

    public bool IsWalkable(Vector2Int gridPos)
    {
        // 🚀 ต้องไม่ติดทั้งกำแพงและของพังได้
        return !IsSolid(gridPos) && !IsDestructible(gridPos);
    }

    public bool IsSolid(Vector2Int gridPos)
    {
        // 🛡️ หัวใจสำคัญ: ใช้ WorldToCell จาก Tilemap แรกเพื่อหา Cell ที่แม่นยำที่สุด
        Vector3 worldPos = new Vector3(gridPos.x, gridPos.y, 0);

        foreach (var map in _solidTilemaps)
        {
            if (map != null)
            {
                // แปลงพิกัดโดยอ้างอิงจากตำแหน่งจริงของแมพ (รองรับ Offset 0.5)
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
        // ปรับพิกัดเป้าหมายให้ตรงร่อง Cell
        Vector3 worldPos = new Vector3(gridPos.x, gridPos.y, 0);

        foreach (var map in _destructibleTilemaps)
        {
            if (map != null)
            {
                Vector3Int targetCell = map.WorldToCell(worldPos);
                if (map.HasTile(targetCell))
                {
                    map.SetTile(targetCell, null);
                    RemoveDangerZone((Vector2Int)targetCell);
                }
            }
        }
    }

    #endregion

    private void AddDangerZone(Vector2Int gridPos) => _dangerousCells.Add(gridPos);
    private void RemoveDangerZone(Vector2Int gridPos) => _dangerousCells.Remove(gridPos);
    private void ExecuteHandleFlameCollision(Vector3Int gridPos, Collider2D intruder) => ExecuteProcessDestruction(gridPos);
}