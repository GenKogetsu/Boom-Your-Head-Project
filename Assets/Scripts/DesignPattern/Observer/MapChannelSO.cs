using UnityEngine;
using System;

namespace Genoverrei.DesignPattern
{
    [CreateAssetMenu(fileName = "MapChannel", menuName = "BombGame/Channels/MapChannel")]
    public sealed class MapChannelSO : ScriptableObject
    {
        public Func<Vector2Int, bool> OnCheckWalkable;
        public Func<Vector2Int, bool> OnCheckDangerous;
        public Func<Vector2Int, bool> OnCheckSolid;
        public Func<Vector2Int, bool> OnCheckDestructible;
        public Func<Vector2Int, bool> OnCheckHasBomb; // 🚀 ฟีเจอร์มองเห็นระเบิด

        public Action<Vector3Int, Collider2D> OnRequestDestruction;
        public Action<Vector2Int> OnAddDanger;
        public Action<Vector2Int> OnRemoveDanger;

        public bool IsWalkable(Vector2Int pos) => OnCheckWalkable != null ? OnCheckWalkable(pos) : true;
        public bool IsDangerous(Vector2Int pos) => OnCheckDangerous != null ? OnCheckDangerous(pos) : false;
        public bool IsSolid(Vector2Int pos) => OnCheckSolid != null ? OnCheckSolid(pos) : false;
        public bool IsDestructible(Vector2Int pos) => OnCheckDestructible != null ? OnCheckDestructible(pos) : false;
        public bool HasBomb(Vector2Int pos) => OnCheckHasBomb != null ? OnCheckHasBomb(pos) : false;

        public void RaiseDestruction(Vector3Int pos, Collider2D intruder) => OnRequestDestruction?.Invoke(pos, intruder);
        public void AddDangerZone(Vector2Int pos) => OnAddDanger?.Invoke(pos);
        public void RemoveDangerZone(Vector2Int pos) => OnRemoveDanger?.Invoke(pos);

        public void Clear()
        {
            OnCheckWalkable = null; OnCheckDangerous = null; OnCheckSolid = null;
            OnCheckDestructible = null; OnCheckHasBomb = null;
            OnRequestDestruction = null; OnAddDanger = null; OnRemoveDanger = null;
        }
    }
}