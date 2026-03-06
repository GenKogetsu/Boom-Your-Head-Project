using System.Collections.Generic;
using UnityEngine;
using Genoverrei.DesignPattern;
using Genoverrei.Libary; // เรียกใช้ Library ที่เราเพิ่งรวมร่างกัน
using BombGame.RecordEventSpace;

namespace BombGame.Manager;

/// <summary>
/// <para> Summary : </para>
/// <para> (TH) : ผู้จัดการแผนที่ ทำหน้าที่เป็น IMapProvider เพื่อให้ข้อมูลแก่ IPathfindable (บอท) </para>
/// <para> (EN) : Map manager serving as IMapProvider to provide data for IPathfindable (Bots). </para>
/// </summary>
public sealed class MapManager : MonoBehaviour, IMapProvider, IEventListener
{
    #region Variable

    [Header("Map Settings")]
    [SerializeField] private Vector2Int _mapSize = new Vector2Int(15, 13);

    // เก็บสิ่งกีดขวางถาวร (กำแพง/กล่อง)
    private readonly HashSet<Vector2Int> _staticObstacles = new();

    // เก็บตำแหน่งระเบิดที่วางอยู่ปัจจุบัน
    private readonly Dictionary<Vector2Int, int> _activeBombs = new();

    #endregion //Variable

    #region Unity Lifecycle

    private void Awake() => ExecuteInitializeMap();

    private void OnEnable() => EventBus.Instance.Subscribe<IEvent>(OnHandleEvent);

    private void OnDisable() => EventBus.Instance.Unsubscribe<IEvent>(OnHandleEvent);

    #endregion //Unity Lifecycle

    #region Interface Implementation (IEventListener)

    public void OnHandleEvent(IEvent eventData)
    {
        switch (eventData)
        {
            case BombPlantedEvent bombEvent:
                ExecuteAddBomb(bombEvent.Pos);
                break;

                // TODO: ในอนาคตต้องมี BombExplodedEvent เพื่อลบระเบิดออก
                // case BombExplodedEvent explodeEvent:
                //     ExecuteRemoveBomb(explodeEvent.Pos);
                //     break;
        }
    }

    #endregion //Interface Implementation

    #region Interface Implementation (IMapProvider)

    /// <summary>
    /// <para> (TH) : เช็คว่าบอทเดินก้าวนี้ได้ไหม (ไม่ชนกำแพง/ไม่ทับระเบิด) </para>
    /// </summary>
    public bool IsWalkable(Vector2Int gridPos)
    {
        // 1. เช็คขอบแมพ
        if (gridPos.x < 0 || gridPos.x >= _mapSize.x || gridPos.y < 0 || gridPos.y >= _mapSize.y)
            return false;

        // 2. เช็คกำแพงถาวร
        if (_staticObstacles.Contains(gridPos))
            return false;

        // 3. เช็คว่ามีตัวระเบิดขวางทางอยู่ไหม (ระเบิดมีคอลไลเดอร์ เดินทับไม่ได้)
        if (_activeBombs.ContainsKey(gridPos))
            return false;

        return true;
    }

    /// <summary>
    /// <para> (TH) : เช็คว่าช่องนี้ "เสี่ยงตาย" ไหม (อยู่ในรัศมีระเบิดที่กำลังจะตู้ม) </para>
    /// </summary>
    public bool IsDangerous(Vector2Int gridPos)
    {
        // ถ้าช่องนี้คือที่วางระเบิด หรืออยู่ในรัศมีกากบาทของระเบิดใดๆ
        foreach (var bomb in _activeBombs)
        {
            if (IsInsideExplosionRange(gridPos, bomb.Key)) return true;
        }
        return false;
    }

    #endregion //Interface Implementation

    #region Private Logic

    private void ExecuteAddBomb(Vector2Int pos)
    {
        if (!_activeBombs.ContainsKey(pos)) _activeBombs.Add(pos, 3); // สมมติรัศมี 3 ช่อง
    }

    private bool IsInsideExplosionRange(Vector2Int target, Vector2Int bombPos)
    {
        // เช็คว่าอยู่ในแนวแกนเดียวกัน (กากบาท) และระยะไม่เกินรัศมี
        bool inSameRow = target.y == bombPos.y && Mathf.Abs(target.x - bombPos.x) <= 3;
        bool inSameCol = target.x == bombPos.x && Mathf.Abs(target.y - bombPos.y) <= 3;

        return inSameRow || inSameCol;
    }

    private void ExecuteInitializeMap()
    {
        // ตัวอย่าง: แอดกำแพงขอบแมพหรือเสาทึบ
        // _staticObstacles.Add(new Vector2Int(1, 1));
    }

    #endregion //Private Logic
}