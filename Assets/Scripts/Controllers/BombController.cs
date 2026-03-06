using System;
using UnityEngine;
using Genoverrei.Libary;
using NaughtyAttributes;
using BombGame.RecordEventSpace;
using BombGame.EnumSpace;

namespace BombGame.Controller;

/// <summary>
/// <para> Summary : </para>
/// <para> (TH) : ตัวควบคุมระเบิดรายก้อน จัดการ Lifecycle และแจ้งสถานะผ่าน BombChannelSO </para>
/// <para> (EN) : Individual bomb controller, manages lifecycle and notifies status via BombChannelSO. </para>
/// </summary>
public sealed class BombController : MonoBehaviour
{
    #region Variable

    [Header("Observer")]
    [SerializeField] private BombChannelSO _bombChannel;

    [Header("Runtime Display")]
    [ReadOnly][SerializeField] private Character _ownerName;
    [ReadOnly][SerializeField] private Vector2Int _gridPosition;
    [ReadOnly][SerializeField] private int _radius;
    [ReadOnly][SerializeField] private float _currentTimer;
    [ReadOnly][SerializeField] private bool _isExploded;

    private StatsController _ownerStats;

    #endregion //Variable

    private void Update()
    {
        if (_isExploded) return;

        _currentTimer -= Time.deltaTime;
        if (_currentTimer <= 0) ForceExplode();
    }

    /// <summary>
    /// <para> Summary : </para>
    /// <para> (TH) : กำหนดค่าเริ่มต้นให้กับระเบิด และแจ้ง MapManager ผ่าน Channel ว่ามีการวางระเบิด </para>
    /// <para> (EN) : Initializes the bomb and notifies MapManager via Channel that a bomb is planted. </para>
    /// </summary>
    public void Initialize(BombBuilder builder, Vector2Int gridPos, StatsController ownerStats)
    {
        _ownerStats = ownerStats;
        _ownerName = ownerStats != null ? ownerStats.LivingName : Character.None;
        _gridPosition = gridPos;
        _radius = builder.Radius;
        _currentTimer = 3f;
        _isExploded = false;

        // แจ้งผ่าน Channel (MapManager จะบันทึกตำแหน่งนี้เป็นสิ่งกีดขวางและพื้นที่อันตราย)
        if (_bombChannel != null) _bombChannel.RaiseBombPlanted(_gridPosition, _radius);
    }

    /// <summary>
    /// <para> Summary : </para>
    /// <para> (TH) : สั่งให้ระเบิดทำงานทันที และแจ้งเหตุการณ์ผ่าน Channel </para>
    /// <para> (EN) : Forces the bomb to explode immediately and notifies events via Channel. </para>
    /// </summary>
    public void ForceExplode()
    {
        if (_isExploded) return;
        _isExploded = true;

        // ตะโกนบอกผ่าน Channel ว่าตู้มแล้ว! (MapManager จะลบพิกัดออก, BombManager จะงอกไฟ)
        if (_bombChannel != null) _bombChannel.RaiseBombExploded(_gridPosition, _radius);

        if (_ownerStats != null) _ownerStats.BombsRemaining++;

        // ส่งกลับเข้า Pool (ตามระบบเดิมของพี่)
        ObjectPoolManager.Instance.Release("Bomb", this);
    }
}