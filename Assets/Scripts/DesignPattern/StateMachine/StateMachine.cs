namespace Genoverrei.DesignPattern;

/// <summary>
/// <para> Summary : </para>
/// <para> (TH) : คลาสฐานสำหรับสถานะต่างๆ ใน StateMachine (รองรับทั้ง MonoBehaviour และคลาส C# ทั่วไป) </para>
/// <para> (EN) : Base class for states in a StateMachine (supports both MonoBehaviour and standard C# classes). </para>
/// </summary>
public abstract class StateMachine<T> : ISate where T : class
{
    #region Fields

    protected readonly T Owner;

    #endregion // Fields

    #region Constructor

    public StateMachine(T owner) => Owner = owner;

    #endregion // Constructor
}