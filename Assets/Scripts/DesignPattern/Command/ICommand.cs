using UnityEngine;

namespace Genoverrei.DesignPattern;

/// <summary>
/// <para>Summary :</para>
/// <para>(TH) : Interface สำหรับ Command Pattern เพื่อใช้ส่งคำสั่งให้ Actor ประมวลผล</para>
/// <para>(EN) : Interface for the Command Pattern to send executable commands to an Actor.</para>
/// </summary>

public interface ICommand
{
    #region Public Methods

    /// <summary>
    /// <para>Summary :</para>
    /// <para>(TH) : รันคำสั่งที่กำหนดให้แก่ Actor เป้าหมาย</para>
    /// <para>(EN) : Executes the specified command on the target actor.</para>
    /// </summary>
    void Execute(GameObject actor);

    #endregion // Public Methods
}