namespace Genoverrei.DesignPattern;

/// <summary>
/// <para> Summary : </para>
/// <para> (TH) : อินเตอร์เฟสสำหรับเหตุการณ์ที่ระบุเป้าหมายได้ </para>
/// <para> (EN) : Interface for events with target signaling. </para>
/// </summary>
public interface ISignal : IEvent
{
    Character SignalTarget { get; }
    ActionType Action { get; }
    IEvent Event { get; }
}

/// <summary>
/// <para> Summary : </para>
/// <para> (TH) : อินเตอร์เฟสสำหรับ Listener ที่สามารถรับสัญญาณจาก ISignal ได้ </para>
/// <para> (EN) : Interface for listeners that can receive signals from ISignal. </para>
/// </summary>
public interface ISignalListener
{
    /// <summary>
    /// <para> Summary : </para>
    /// <para> (TH) : จัดการและประมวลผลสัญญาณที่ได้รับ </para>
    /// <para> (EN) : Handles and processes the received signal. </para>
    /// </summary>
    /// <param name="signal">
    /// <para> param : </para>
    /// <para> (TH) : ข้อมูลสัญญาณที่ส่งผ่านมายัง Listener </para>
    /// <para> (EN) : The signal data passed to the listener. </para>
    /// </param>
    void OnHandleSignal(ISignal signal);
}