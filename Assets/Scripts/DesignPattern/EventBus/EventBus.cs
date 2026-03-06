using System;
using System.Collections.Generic;
using UnityEngine;

namespace Genoverrei.DesignPattern;

/// <summary>
/// <para>Summary :</para>
/// <para>(TH) : ระบบจัดการ Event ส่วนกลางที่ใช้ IEvent เป็นตัวกลางในการสื่อสารแบบ Decoupled</para>
/// <para>(EN) : Centralized event system using IEvent for decoupled communication.</para>
/// </summary>
public class EventBus : Singleton<EventBus>
{
    #region Fields

    private readonly Dictionary<Type, Action<IEvent>> _events = new();
    private readonly Dictionary<Delegate, Action<IEvent>> _lookups = new();

    #endregion // Fields

    #region Public Methods

    /// <summary>
    /// <para>Summary :</para>
    /// <para>(TH) : ลงทะเบียนเพื่อรับฟังเหตุการณ์ (Subscribe)</para>
    /// <para>(EN) : Register a listener (Subscribe).</para>
    /// </summary>
    public void Subscribe<T>(Action<T> listener) where T : IEvent
    {
        Type type = typeof(T);
        Action<IEvent> wrapper = (e) => listener((T)e);
        _lookups[listener] = wrapper;

        if (!_events.ContainsKey(type)) _events[type] = null;
        _events[type] += wrapper;

#if UNITY_EDITOR
        Debug.Log($"<b><color=#4FC3F7>[EventBus]</color></b> " +
            $"Subscribed to: " +
            $"<color=#81C784>{type.Name}</color>");
#endif
    }

    /// <summary>
    /// <para>Summary :</para>
    /// <para>(TH) : ยกเลิกการรับฟังเหตุการณ์ (Unsubscribe)</para>
    /// <para>(EN) : Unregister a listener (Unsubscribe).</para>
    /// </summary>
    public void Unsubscribe<T>(Action<T> listener) where T : IEvent
    {
        Type type = typeof(T);
        if (!_lookups.TryGetValue(listener, out var wrapper)) return;

        if (_events.ContainsKey(type)) _events[type] -= wrapper;
        _lookups.Remove(listener);

#if UNITY_EDITOR
        Debug.Log($"<b><color=#4FC3F7>[EventBus]</color></b> " +
            $"Unsubscribed from: " +
            $"<color=#E57373>{type.Name}</color>");
#endif
    }

    /// <summary>
    /// <para>Summary :</para>
    /// <para>(TH) : กระจายสัญญาณเหตุการณ์ไปยังทุกคนที่ Subscribe ไว้</para>
    /// <para>(EN) : Broadcast an event to all subscribed listeners.</para>
    /// </summary>
    public void Publish<T>(T eventData) where T : IEvent
    {
        Type type = typeof(T);
        if (!_events.TryGetValue(type, out var action)) return;
        action?.Invoke(eventData);

#if UNITY_EDITOR
        Debug.Log($"<b><color=#4FC3F7>[EventBus]</color></b> " +
            $"Published: " +
            $"<color=#FFF176>{type.Name}</color>");
#endif
    }

    #endregion // Public Methods
}