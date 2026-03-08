using System;

[Serializable]

public class BotBrainState
{
    public Character TargetPlayer = Character.None;
    public BotFullStateMachine Behavior = BotFullStateMachine.Idle;
    public Vector2Int TargetGridPos = new Vector2Int(-9999, -9999);
    public Vector2Int SafeTarget = new Vector2Int(-9999, -9999); // 🚀 ล็อกเป้าหมายการหนีตาย
    public Vector2Int WanderTarget = new Vector2Int(-9999, -9999);
    public Vector2Int TargetItemPos = new Vector2Int(-9999, -9999);
    public float LastWanderTime;
    public float LastBombTime;
    public List<Vector2Int> CurrentFullPath = new List<Vector2Int>();
    public Vector2Int TargetBombTile = new Vector2Int(-9999, -9999);
}