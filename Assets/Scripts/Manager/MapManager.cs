using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using Genoverrei.DesignPattern;

public sealed class MapManager : MonoBehaviour
{
    [Header("Observer Channels")]
    [SerializeField] private MapChannelSO _mapChannel;
    [SerializeField] private BombChannelSO _bombChannel;

    [Header("Tilemaps")]
    [SerializeField] private List<Tilemap> _solidTilemaps = new();
    [SerializeField] private List<Tilemap> _destructibleTilemaps = new();

    private HashSet<Vector2Int> _dangerousCells = new();

    private void OnEnable()
    {
        if (_mapChannel != null)
        {
            // 🚀 เสียบปลั๊ก: ฝากฟังก์ชันไว้ที่ Channel
            _mapChannel.OnCheckWalkable = IsWalkable;
            _mapChannel.OnCheckDangerous = IsDangerous;
            _mapChannel.OnCheckSolid = IsSolid;             // 👈 เชื่อมต่อ
            _mapChannel.OnCheckDestructible = IsDestructible; // 👈 เชื่อมต่อ
            _mapChannel.OnRequestDestruction = ExecuteHandleExplosionHit;
            _mapChannel.OnAddDanger = AddDangerZone;
            _mapChannel.OnRemoveDanger = RemoveDangerZone;
        }

        if (_bombChannel != null)
            _bombChannel.OnExplosionHit += ExecuteHandleExplosionHit;
    }

    private void OnDisable()
    {
        if (_mapChannel != null) _mapChannel.Clear();
        if (_bombChannel != null) _bombChannel.OnExplosionHit -= ExecuteHandleExplosionHit;
    }

    // --- Logic เช็คสถานะ Tile ---

    public bool IsWalkable(Vector2Int gridPos)
    {
        Vector3Int cellPos = new(gridPos.x, gridPos.y, 0);
        foreach (var map in _solidTilemaps) if (map != null && map.HasTile(cellPos)) return false;
        foreach (var map in _destructibleTilemaps) if (map != null && map.HasTile(cellPos)) return false;
        return true;
    }

    public bool IsSolid(Vector2Int gridPos)
    {
        Vector3Int cellPos = new(gridPos.x, gridPos.y, 0);
        foreach (var map in _solidTilemaps) if (map != null && map.HasTile(cellPos)) return true;
        return false;
    }

    public bool IsDestructible(Vector2Int gridPos)
    {
        Vector3Int cellPos = new(gridPos.x, gridPos.y, 0);
        foreach (var map in _destructibleTilemaps) if (map != null && map.HasTile(cellPos)) return true;
        return false;
    }

    public bool IsDangerous(Vector2Int gridPos) => _dangerousCells.Contains(gridPos);
    private void AddDangerZone(Vector2Int gridPos) => _dangerousCells.Add(gridPos);
    private void RemoveDangerZone(Vector2Int gridPos) => _dangerousCells.Remove(gridPos);

    private void ExecuteHandleExplosionHit(Vector3Int gridPos, Collider2D intruder)
    {
        foreach (var map in _destructibleTilemaps)
        {
            if (map != null && map.HasTile(gridPos))
            {
                map.SetTile(gridPos, null);
                RemoveDangerZone((Vector2Int)gridPos);
                Debug.Log($"<color=orange>[MapManager]</color> Tile destroyed at {gridPos}");
            }
        }
    }
}