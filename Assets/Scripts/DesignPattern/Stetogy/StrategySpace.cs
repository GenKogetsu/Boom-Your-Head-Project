namespace BombGame.StrategySpace;

#region Interfaces

/// <summary>
/// <para> Summary : </para>
/// <para> (TH) : อินเทอร์เฟซหลักสำหรับกลยุทธ์การไล่ล่าผู้เล่น </para>
/// <para> (EN) : Base interface for player chasing strategies. </para>
/// </summary>
public interface IChaseStrategy
{
    Vector2 CalculateMove(BotBrainContext ctx);
}

/// <summary>
/// <para> Summary : </para>
/// <para> (TH) : อินเทอร์เฟซหลักสำหรับกลยุทธ์การตัดสินใจวางระเบิด </para>
/// <para> (EN) : Base interface for bomb placement decision strategies. </para>
/// </summary>
public interface IBombStrategy
{
    void ExecuteBombLogic(BotBrainContext ctx);
}

/// <summary>
/// <para> Summary : </para>
/// <para> (TH) : อินเทอร์เฟซหลักสำหรับกลยุทธ์การค้นหาและเก็บไอเทม </para>
/// <para> (EN) : Base interface for item searching and collecting strategies. </para>
/// </summary>
public interface IItemStrategy
{
    Vector2 CalculatePathToItem(BotBrainContext ctx);
}

#endregion //Interfaces


#region Concrete Chase Strategies

/// <summary>
/// <para> Summary : </para>
/// <para> (TH) : กลยุทธ์เดินหาผู้เล่นเป้าหมายผ่านเส้นทางที่สั้นที่สุด (เช็คจาก MapManager) </para>
/// <para> (EN) : Strategy to find the target player using the shortest path (checked via MapManager). </para>
/// </summary>
public sealed class ShortestPathChaseStrategy : IChaseStrategy
{
    public Vector2 CalculateMove(BotBrainContext ctx)
    {
        // TODO: 1. ถาม PlayerManager ว่าเป้าหมายอยู่ไหน
        // TODO: 2. ถาม MapManager ว่าช่องไหนเดินได้ (A* Algorithm)

        // จำลองการคืนค่าทิศทาง (พี่ค่อยมาเติม Logic A* ตรงนี้ครับ)
        return Vector2.zero;
    }
}

#endregion //Concrete Chase Strategies


#region Concrete Bomb Strategies

/// <summary>
/// <para> Summary : </para>
/// <para> (TH) : กลยุทธ์วางระเบิดทิ้งไว้แล้วเตรียมตัวเดินหนีออกจากรัศมี </para>
/// <para> (EN) : Strategy to place a bomb and prepare to flee the blast radius. </para>
/// </summary>
public sealed class PlaceAndFleeBombStrategy : IBombStrategy
{
    public void ExecuteBombLogic(BotBrainContext ctx)
    {
        // ส่งสัญญาณวางระเบิด (ไม่ต้องมี subEvent สำหรับ Action นี้)
        ctx.ExecuteAction(ActionType.PlaceBomb, null);

        // หมายเหตุ: หลังจาก Method นี้ทำงานเสร็จ State ของบอทควรสลับไปเป็น FleeState
    }
}

/// <summary>
/// <para> Summary : </para>
/// <para> (TH) : กลยุทธ์วางระเบิดเพื่อดักหน้าผู้เล่นในช่องทางแคบ </para>
/// <para> (EN) : Strategy to place a bomb to trap the player in a narrow path. </para>
/// </summary>
public sealed class TrapBombStrategy : IBombStrategy
{
    public void ExecuteBombLogic(BotBrainContext ctx)
    {
        // TODO: คำนวณหาช่องทางแคบ แล้ววางระเบิดดัก
        ctx.ExecuteAction(ActionType.PlaceBomb, null);
    }
}

#endregion //Concrete Bomb Strategies


#region Concrete Item Strategies

/// <summary>
/// <para> Summary : </para>
/// <para> (TH) : กลยุทธ์ให้ความสำคัญกับการเดินไปเก็บไอเทมบนแผนที่ก่อนสิ่งอื่น </para>
/// <para> (EN) : Strategy prioritizing moving to collect items on the map before anything else. </para>
/// </summary>
public sealed class PriorityItemStrategy : IItemStrategy
{
    public Vector2 CalculatePathToItem(BotBrainContext ctx)
    {
        // TODO: ตรวจสอบตำแหน่งไอเทมจาก MapManager แล้วหาทางเดินไปหาไอเทมชิ้นนั้น
        return Vector2.zero;
    }
}

#endregion //Concrete Item Strategies