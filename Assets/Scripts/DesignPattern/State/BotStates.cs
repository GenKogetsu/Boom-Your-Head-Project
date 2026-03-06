namespace BombGame.StateSpace;

#region Chase State

/// <summary>
/// <para> Summary : </para>
/// <para> (TH) : สถานะไล่ล่าผู้เล่น จะทำการเดินตามกลยุทธ์ที่กำหนด หรือเปลี่ยนไปวางระเบิดถ้าทางตัน </para>
/// <para> (EN) : Chasing state, moves according to the defined strategy or switches to bomb state if blocked. </para>
/// </summary>
public sealed class BotChaseState : StateMachine<BotBrainContext>, IUpdateState
{
    public BotChaseState(BotBrainContext owner) : base(owner) { }

    public void OnUpdate()
    {
        // 1. เรียกใช้กลยุทธ์คำนวณหาทิศทางเดิน
        Vector2 moveDirection = Owner.ChaseStrategy.CalculateMove(Owner);

        if (moveDirection != Vector2.zero)
        {
            // ถ้ามีทางเดิน ให้ห่อทิศทางลงใน MoveInputEvent แล้วส่งให้ Context ยิงสัญญาณ
            Owner.ExecuteAction(ActionType.Move, new MoveInputEvent(moveDirection));
        }
        else
        {
            // ถ้าทางตัน (เดินไม่ได้) ให้หยุดนิ่งก่อนส่งสัญญาณ
            Owner.ExecuteAction(ActionType.Move, new MoveInputEvent(Vector2.zero));

            // สลับไปสถานะวางระเบิดเพื่อเปิดทาง
            Owner.ChangeState<BotBombState>();
        }
    }
}

#endregion //Chase State


#region Bomb State

/// <summary>
/// <para> Summary : </para>
/// <para> (TH) : สถานะวางระเบิด จะทำงานตามกลยุทธ์ที่กำหนดแบบครั้งเดียวจบแล้วสลับไปสถานะอื่น </para>
/// <para> (EN) : Bombing state, executes the defined strategy once and transitions to another state. </para>
/// </summary>
public sealed class BotBombState : StateMachine<BotBrainContext>, IEnterState
{
    public BotBombState(BotBrainContext owner) : base(owner) { }

    public void OnEnter()
    {
        // 1. สั่งทำงานตามกลยุทธ์วางระเบิด (เช่น วางแล้วหนี หรือวางดักทาง)
        Owner.BombStrategy.ExecuteBombLogic(Owner);

        // 2. เมื่อวางเสร็จแล้ว กลับไปสถานะไล่ล่าต่อ (หรือในอนาคตพี่อาจจะเพิ่ม BotFleeState เพื่อให้มันวิ่งหนีไฟระเบิดก่อน)
        Owner.ChangeState<BotChaseState>();
    }
}

#endregion //Bomb State


#region Item State

/// <summary>
/// <para> Summary : </para>
/// <para> (TH) : สถานะเดินเก็บไอเทม จะคำนวณเส้นทางไปหาไอเทมที่ใกล้ที่สุดจนกว่าจะเก็บสำเร็จ </para>
/// <para> (EN) : Item collection state, calculates the path to the nearest item until collected. </para>
/// </summary>
public sealed class BotItemState : StateMachine<BotBrainContext>, IUpdateState
{
    public BotItemState(BotBrainContext owner) : base(owner) { }

    public void OnUpdate()
    {
        // 1. เรียกใช้กลยุทธ์คำนวณหาเส้นทางไปเก็บไอเทม
        Vector2 itemDirection = Owner.ItemStrategy.CalculatePathToItem(Owner);

        if (itemDirection != Vector2.zero)
        {
            // เดินไปหาไอเทม
            Owner.ExecuteAction(ActionType.Move, new MoveInputEvent(itemDirection));
        }
        else
        {
            // ถ้าไอเทมหายไปแล้ว หรือเดินไปถึงแล้ว ให้หยุดเดิน
            Owner.ExecuteAction(ActionType.Move, new MoveInputEvent(Vector2.zero));

            // สลับกลับไปสถานะไล่ล่าตามปกติ
            Owner.ChangeState<BotChaseState>();
        }
    }
}

#endregion //Item State