using System;
using UnityEngine;
using Genoverrei.DesignPattern;

/// <summary>
/// <para> Summary : </para>
/// <para> (TH) : เซนเซอร์ไฟระเบิด แจ้งเหตุการณ์การชนผ่าน BombChannelSO แทนการใช้ Singleton </para>
/// <para> (EN) : Explosion sensor notifying collision events via BombChannelSO instead of Singleton. </para>
/// </summary>
[RequireComponent(typeof(Collider2D))]
public sealed class BombTriggerSensor : MonoBehaviour
{
    #region Variable

    [Header("Observer")]
    [SerializeField] private BombChannelSO _bombChannel;

    [Header("Lifecycle Settings")]
    [SerializeField] private AnimationClip _lifeTime;

    #endregion //Variable

    #region Unity Lifecycle

    private void OnEnable()
    {
        // เริ่มนับถอยหลังการ Release กลับเข้า Pool
        Invoke(nameof(ExecuteRelease), _lifeTime.length);
    }

    private void OnDisable()
    {
        CancelInvoke();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_bombChannel == null) return;

        // คำนวณตำแหน่ง Grid จากตำแหน่งปัจจุบันของ Sensor
        Vector3Int gridPos = new(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));

        Debug.Log($"[BombTriggerSensor] Detected collision at Grid Position: {gridPos} with {other.gameObject.name}");

        // ส่งสัญญาณผ่าน Channel เพื่อให้ Manager ที่เกี่ยวข้องนำไปจัดการต่อ
        _bombChannel.RaiseExplosionHit(gridPos, other);
    }

    #endregion //Unity Lifecycle

    #region Private Logic

    private void ExecuteRelease()
    {
        // ปล่อยตัวเซนเซอร์ (ที่ใช้ Animation ไฟ) กลับเข้า Pool
        ObjectPoolManager.Instance.Release("Explosion", this);
    }

    #endregion //Private Logic
}