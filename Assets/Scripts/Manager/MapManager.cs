using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using Genoverrei.DesignPattern;
using Genoverrei.Libary;
using BombGame.RecordEventSpace;

namespace BombGame.Manager;

/// <summary>
/// <para> Summary : </para>
/// <para> (TH) : ศูนย์กลางจัดการแผนที่ ทั้งการสแกน Grid เริ่มต้น, การทำลาย Tile และการสุ่มไอเทม </para>
/// <para> (EN) : Central map manager handling initial grid scanning, tile destruction, and item drops. </para>
/// </summary>
public sealed class MapManager : Singleton<MapManager>, IMapProvider
{
    #region Variable

    [Header("Observer Channels")]
    [SerializeField] private BombChannelSO _bombChannel;

    [Header("Map Configuration")]
    [SerializeField] private Vector2Int _mapSize = new Vector2Int(15, 13);
    [SerializeField] private Vector2Int _mapOffset = new Vector2Int(-7, -6);
    [SerializeField] private List<Tilemap> _solidTilemaps = new();
    [SerializeField] private List<Tilemap> _destructibleTilemaps = new();

    [Header("Item Drop Configuration")]
    [Range(0f, 100f)]
    [SerializeField] private float _dropRate = 30f;
    [SerializeField] private List<GameObject> _itemPrefabs = new();

    /// <summary>
    /// <para> (TH) : พจนานุกรมเก็บสถานะการเดินผ่านได้ (True = ว่าง, False = มีสิ่งกีดขวาง) </para>
    /// </summary>
    private Dictionary<Vector2Int, bool> _walkableMap = new();

    /// <summary>
    /// <para> (TH) : เก็บตำแหน่งระเบิดปัจจุบันที่ยังไม่ระเบิด พร้อมรัศมีของมัน </para>
    /// </summary>
    private Dictionary<Vector2Int, int> _activeBombs = new Dictionary<Vector2Int, int>();

    #endregion //Variable


    #region Unity Lifecycle

    /// <summary>
    /// override Awake จาก Singleton เพื่อทำการ Initialize ข้อมูลแผนที่
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        ExecuteInitializeMap();
    }

    private void OnEnable()
    {
        if (_bombChannel != null)
        {
            _bombChannel.OnBombPlanted += HandleBombPlanted;
            _bombChannel.OnBombExploded += HandleBombExploded;
        }
    }

    private void OnDisable()
    {
        if (_bombChannel != null)
        {
            _bombChannel.OnBombPlanted -= HandleBombPlanted;
            _bombChannel.OnBombExploded -= HandleBombExploded;
        }
    }

    #endregion //Unity Lifecycle


    #region IMapProvider Implementation

    public bool IsWalkable(Vector2Int gridPos)
    {
        // 1. เช็คขอบแผนที่
        if (gridPos.x < _mapOffset.x || gridPos.x >= _mapSize.x + _mapOffset.x ||
            gridPos.y < _mapOffset.y || gridPos.y >= _mapSize.y + _mapOffset.y)
            return false;

        // 2. เช็คจาก Cache แผนที่ (ถ้าไม่มีใน Dictionary หรือเป็น False แปลว่าเดินไม่ได้)
        if (!_walkableMap.TryGetValue(gridPos, out bool isBaseWalkable) || !isBaseWalkable)
            return false;

        // 3. เช็คว่ามีระเบิดวางขวางอยู่ไหม (ระเบิดถือเป็นสิ่งกีดขวางชั่วคราว)
        if (_activeBombs.ContainsKey(gridPos))
            return false;

        return true;
    }

    public bool IsDangerous(Vector2Int gridPos)
    {
        // ตรวจสอบว่าพิกัดนี้อยู่ในรัศมีทำลายล้างของระเบิดก้อนไหนหรือไม่
        foreach (var bomb in _activeBombs)
        {
            Vector2Int bombPos = bomb.Key;
            int radius = bomb.Value;

            // เช็คแนวรัศมีกากบาท (Horizontal & Vertical)
            bool inRangeX = gridPos.y == bombPos.y && Mathf.Abs(gridPos.x - bombPos.x) <= radius;
            bool inRangeY = gridPos.x == bombPos.x && Mathf.Abs(gridPos.y - bombPos.y) <= radius;

            if (inRangeX || inRangeY) return true;
        }
        return false;
    }

    #endregion //IMapProvider Implementation


    #region Public Methods (Tile Actions)

    /// <summary>
    /// <para> (TH) : ประมวลผลการทำลาย Tile พร้อมอัปเดตระบบนำทางและสุ่มของ </para>
    /// </summary>
    public void ExecuteProcessDestruction(Vector3Int pos)
    {
        foreach (var map in _destructibleTilemaps)
        {
            if (map.HasTile(pos))
            {
                map.SetTile(pos, null);

                // อัปเดตสถานะนำทางให้ช่องนี้เดินผ่านได้ (True)
                ExecuteUpdateNavigationGrid((Vector3)(Vector3Int)pos, true);

                TrySpawnItem((Vector3)(Vector3Int)pos);
            }
        }
    }

    public bool IsSolid(Vector2Int pos)
    {
        foreach (var map in _solidTilemaps)
            if (map.HasTile((Vector3Int)pos)) return true;
        return false;
    }

    public bool IsDestructible(Vector2Int pos)
    {
        foreach (var map in _destructibleTilemaps)
            if (map.HasTile((Vector3Int)pos)) return true;
        return false;
    }

    #endregion //Public Methods


    #region Private Logic (Event Handlers)

    private void HandleBombPlanted(Vector2Int pos, int radius)
    {
        if (!_activeBombs.ContainsKey(pos)) _activeBombs.Add(pos, radius);
    }

    private void HandleBombExploded(Vector2Int pos, int radius)
    {
        if (_activeBombs.ContainsKey(pos)) _activeBombs.Remove(pos);
    }

    #endregion //Private Logic


    #region Private Logic (Internal Map Management)

    /// <summary>
    /// <para> (TH) : สแกนหา Tilemap ทั้งหมดเพื่อสร้างฐานข้อมูลการเดินเริ่มต้น </para>
    /// </summary>
    private void ExecuteInitializeMap()
    {
        _walkableMap.Clear();

        // วนลูปตามขนาดแผนที่เพื่อตรวจสอบสถานะแต่ละช่อง
        for (int x = _mapOffset.x; x < _mapSize.x + _mapOffset.x; x++)
        {
            for (int y = _mapOffset.y; y < _mapSize.y + _mapOffset.y; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);

                // ถ้าช่องนี้ไม่มีกำแพงทึบและไม่มีกล่อง แปลว่าเดินได้ (True)
                bool isBlocked = IsSolid(pos) || IsDestructible(pos);
                _walkableMap.Add(pos, !isBlocked);
            }
        }
    }

    private void TrySpawnItem(Vector3 pos)
    {
        if (_itemPrefabs.Count == 0) return;

        if (Random.Range(0f, 100f) <= _dropRate)
        {
            var itemIndex = Random.Range(0, _itemPrefabs.Count);
            var itemPoolKey = _itemPrefabs[itemIndex].name;
            ObjectPoolManager.Instance.Get<Transform>(itemPoolKey, pos, Quaternion.identity);
        }
    }

    private void ExecuteUpdateNavigationGrid(Vector3 pos, bool isWalkable)
    {
        Vector2Int gridPos = new(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y));
        _walkableMap[gridPos] = isWalkable;
    }

    #endregion //Private Logic
}