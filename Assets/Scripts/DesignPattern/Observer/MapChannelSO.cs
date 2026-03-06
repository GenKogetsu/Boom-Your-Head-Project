using UnityEngine;
using System;

namespace Genoverrei.DesignPattern
{
    [CreateAssetMenu(fileName = "MapChannel", menuName = "BombGame/Events/MapChannel")]
    public sealed class MapChannelSO : ScriptableObject
    {
        public Func<Vector2Int, bool> OnCheckWalkable;
        public Func<Vector2Int, bool> OnCheckDangerous;
        public Func<Vector2Int, bool> OnCheckSolid;
        public Func<Vector2Int, bool> OnCheckDestructible;

        public Action<Vector3Int, Collider2D> OnRequestDestruction;
        public Action<Vector2Int> OnAddDanger;
        public Action<Vector2Int> OnRemoveDanger;

        // 🚀 เปลี่ยนจาก ?.Invoke เป็นการเช็ค null ปกติ
        public bool IsWalkable(Vector2Int pos) => OnCheckWalkable != null ? OnCheckWalkable(pos) : false;
        public bool IsDangerous(Vector2Int pos) => OnCheckDangerous != null ? OnCheckDangerous(pos) : false;
        public bool IsSolid(Vector2Int pos) => OnCheckSolid != null ? OnCheckSolid(pos) : false;
        public bool IsDestructible(Vector2Int pos) => OnCheckDestructible != null ? OnCheckDestructible(pos) : false;

        public void RaiseDestruction(Vector3Int pos, Collider2D intruder)
        {
            if (OnRequestDestruction != null) OnRequestDestruction(pos, intruder);
        }

        public void AddDangerZone(Vector2Int pos)
        {
            if (OnAddDanger != null) OnAddDanger(pos);
        }

        public void RemoveDangerZone(Vector2Int pos)
        {
            if (OnRemoveDanger != null) OnRemoveDanger(pos);
        }

        public void Clear()
        {
            OnCheckWalkable = null;
            OnCheckDangerous = null;
            OnCheckSolid = null;
            OnCheckDestructible = null;
            OnRequestDestruction = null;
            OnAddDanger = null;
            OnRemoveDanger = null;
        }
    }
}