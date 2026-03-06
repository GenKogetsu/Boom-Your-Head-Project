using Genoverrei.Libary;

namespace BombGame.Ability;

/// <summary>
/// <para> summary_IPlaceBombable </para>
/// <para> (TH) : อินเตอร์เฟสสำหรับความสามารถในการจัดการระเบิดบน Grid </para>
/// <para> (EN) : Interface for handling bomb placement on the grid. </para>
/// </summary>
public interface IPlaceBombable : IAbility
{
    /// <summary>
    /// <para> summary : </para>
    /// <para> (TH) : ประมวลผลการระเบิดตามข้อมูลเหตุการณ์ที่ได้รับ </para>
    /// <para> (EN) : Processes explosion based on event data. </para>
    /// </summary>
    void PlaceBomb(Character owner);
}