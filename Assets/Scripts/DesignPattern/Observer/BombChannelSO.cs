
using UnityEngine;

using System;



namespace BombGame.RecordEventSpace 
{
    [CreateAssetMenu(fileName = "BombChannel", menuName = "BombGame/Events/Bomb Channel")]

    public sealed class BombChannelSO : ScriptableObject

    {

        public event Action<Vector2Int, int> OnBombPlanted;

        public event Action<Vector2Int, int> OnBombExploded;



        // 🚀 เพิ่มตัวนี้เพื่อให้ Sensor ตะโกนบอก Manager ได้โดยตรง

        public event Action<Vector3Int, Collider2D> OnExplosionHit;



        public void RaiseBombPlanted(Vector2Int pos, int radius) => OnBombPlanted?.Invoke(pos, radius);

        public void RaiseBombExploded(Vector2Int pos, int radius) => OnBombExploded?.Invoke(pos, radius);



        // 🚀 ฟังก์ชันตะโกนเมื่อไฟไปแตะโดนอะไรเข้า

        public void RaiseExplosionHit(Vector3Int gridPos, Collider2D other) => OnExplosionHit?.Invoke(gridPos, other);

    }
}




