using UnityEngine;

namespace BombGame.Logic;

/// <summary>
/// <para> summary_BombBuilder </para>
/// <para> (TH) : ตัวประกอบระเบิด (Fluent Interface) สำหรับตั้งค่าดาเมจ ระยะระเบิด และตำแหน่ง Grid </para>
/// <para> (EN) : Bomb builder (Fluent Interface) for configuring damage, radius, and grid position. </para>
/// </summary>
public class BombBuilder
{
    public int Damage { get; private set; } = 1;
    public int Radius { get; private set; } = 1;

    public float NonCriticalTimer { get; private set; } = 3f;

    public BombBuilder SetDamage(int damage) { Damage = damage; return this; }
    public BombBuilder SetRadius(int radius) { Radius = radius; return this; }

    public BombBuilder SetNonCriticalTimer(float timer) { NonCriticalTimer = timer; return this; }


    public void Build(BombController controller, Vector2Int gridPos, StatsController ownerStats)
    {
        controller.Initialize(this, gridPos, ownerStats);
    }
}