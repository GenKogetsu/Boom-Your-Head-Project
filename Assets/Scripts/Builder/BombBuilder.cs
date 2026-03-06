using UnityEngine;

/// <summary>
/// <para> Summary : </para>
/// <para> (TH) : ตัวประกอบระเบิด (Fluent Interface) สำหรับตั้งค่าดาเมจ ระยะระเบิด และเวลาถอยหลัง </para>
/// <para> (EN) : Bomb builder (Fluent Interface) for configuring damage, radius, and timer. </para>
/// </summary>
public class BombBuilder
{
    #region Properties

    public int Damage { get; private set; } = 1;
    public int Radius { get; private set; } = 1;
    public float NonCriticalTimer { get; private set; } = 3f;

    #endregion //Properties

    #region Fluent Methods

    public BombBuilder SetDamage(int damage)
    {
        Damage = damage;
        return this;
    }

    public BombBuilder SetRadius(int radius)
    {
        Radius = radius;
        return this;
    }

    public BombBuilder SetNonCriticalTimer(float timer)
    {
        NonCriticalTimer = timer;
        return this;
    }

    #endregion //Fluent Methods

    #region Execution

    /// <summary>
    /// <para> Summary : </para>
    /// <para> (TH) : ประกอบร่างและส่งค่าทั้งหมดเข้าสู่ระบบ Initialize ของ BombController </para>
    /// <para> (EN) : Finalizes building and passes all configurations to BombController.Initialize. </para>
    /// </summary>
    public void Build(BombController controller, Vector2Int gridPos, StatsController ownerStats)
    {
        // 🚀 ส่งตัวแปร Builder (this) ไปให้ Controller ดึงค่า Radius/Timer/Damage ไปใช้งาน
        controller.Initialize(this, gridPos, ownerStats);
    }

    #endregion //Execution
}