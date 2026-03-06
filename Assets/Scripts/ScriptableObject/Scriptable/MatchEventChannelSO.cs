using System;

namespace BombGame.RecordEventSpace;

/// <summary>
/// <para> Summary : </para>
/// <para> (TH) : ช่องทางการสื่อสารสำหรับสถานะของการแข่งขัน เช่น เริ่มเกม หรือ จบเกม </para>
/// <para> (EN) : Communication channel for match states such as game start or game over. </para>
/// </summary>
[CreateAssetMenu(fileName = "MatchEventChannel", menuName = "BombGame/Events/Match Event Channel")]
public sealed class MatchEventChannelSO : ScriptableObject
{
    #region Public Events

    /// <summary>
    /// <para> Summary : </para>
    /// <para> (TH) : อีเวนต์ที่ถูกเรียกเมื่อสถานะของแมตช์เปลี่ยนแปลง </para>
    /// <para> (EN) : Event invoked when the match state changes. </para>
    /// </summary>
    public event Action<MatchState> OnMatchStateChanged;

    #endregion //Public Events

    #region Public Methods

    public void RaiseEvent(MatchState state)
    {
        OnMatchStateChanged?.Invoke(state);
    }

    #endregion //Public Methods
}