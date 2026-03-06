using UnityEngine;

namespace Genoverrei.DesignPattern;

/// <summary>
/// <para>Summary :</para>
/// <para>(TH) : คลาสฐานสำหรับสถานะต่างๆ ใน StateMachine</para>
/// <para>(EN) : Base class for states in a StateMachine.</para>
/// </summary>

public abstract class StateMachine<T> where T : MonoBehaviour , ISate
{
    #region Fields

    protected readonly T Owner;

    #endregion // Fields

    #region Constructor

    public StateMachine(T owner) => Owner = owner;

    #endregion // Constructor
}