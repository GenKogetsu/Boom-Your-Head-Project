using System;
using UnityEngine;
using BombGame.Manager;

namespace Genoverrei.Libary;

/// <summary>
/// <para> summary_BombTriggerSensor </para>
/// <para> (TH) : เซนเซอร์ไฟระเบิดที่ส่งตำแหน่งของตัวเองเพื่อให้ Manager จัดการต่อ </para>
/// <para> (EN) : Explosion sensor that sends its position for the Manager to process. </para>
/// </summary>
[RequireComponent(typeof(Collider2D))]
public sealed class BombTriggerSensor : MonoBehaviour
{
    #region Variable

    [Header("Lifecycle Settings")]
    [SerializeField] private float _lifeTime = 0.5f;

    public event Action<Vector3Int, Collider2D> OnExplodeHit;

    #endregion //Variable

    #region Unity Lifecycle

    private void OnEnable()
    {
        if (BombManager.Instance != null)
        {
            BombManager.Instance.RegisterExplosionSensor(this);
        }

        Invoke(nameof(ExecuteRelease), _lifeTime);
    }

    private void OnDisable()
    {
        if (BombManager.Instance != null)
        {
            BombManager.Instance.UnregisterExplosionSensor(this);
        }

        CancelInvoke();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // คำนวณตำแหน่ง Grid จากตำแหน่งปัจจุบันของ Sensor
        Vector3Int gridPos = new(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
        OnExplodeHit?.Invoke(gridPos, other);
    }

    #endregion //Unity Lifecycle

    #region Private Logic

    private void ExecuteRelease()
    {
        ObjectPoolManager.Instance.Release("Explosion", this);
    }

    #endregion //Private Logic
}