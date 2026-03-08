using System;

[Serializable]

public class BotBrainState
{
    public Character TargetPlayer = Character.None;
    public BotFullStateMachine Behavior = BotFullStateMachine.Idle;
    public Vector2Int TargetGridPos;
    public Vector2Int SafeTarget;
    public Vector2Int WanderTarget;
    public Vector2Int TargetItemPos; // 🚀 เป้าหมายการเก็บไอเทม
    public float LastWanderTime;
    public float LastBombTime;
    public List<Vector2Int> CurrentFullPath = new();
    public Vector2Int TargetBombTile;
}