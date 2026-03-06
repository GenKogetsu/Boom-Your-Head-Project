using UnityEngine;
using System;

namespace BombGame.RecordEventSpace;

[CreateAssetMenu(fileName = "BombChannel", menuName = "BombGame/Channels/Bomb Channel")]
public sealed class BombChannelSO : ScriptableObject
{
    // เมื่อระเบิดถูกวาง
    public event Action<Vector2Int, int> OnBombPlanted;
    // เมื่อระเบิดทำงาน
    public event Action<Vector2Int, int> OnBombExploded;

    public void RaiseBombPlanted(Vector2Int pos, int radius) => OnBombPlanted?.Invoke(pos, radius);
    public void RaiseBombExploded(Vector2Int pos, int radius) => OnBombExploded?.Invoke(pos, radius);
}