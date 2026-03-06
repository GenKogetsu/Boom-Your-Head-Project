using System;

namespace BombGame.RecordEventSpace
{
    /// <summary>
    /// <para> Summary : </para>
    /// <para> (TH) : ช่องทางการสื่อสารสำหรับเหตุการณ์ที่เกี่ยวข้องกับผู้เล่น เช่น การตาย หรือ การได้รับไอเทม </para>
    /// <para> (EN) : Communication channel for player-related events such as death or item acquisition. </para>
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerEventChannel", menuName = "BombGame/Events/Player Event Channel")]
    public sealed class PlayerEventChannelSO : ScriptableObject
    {
        #region Public Events

        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : อีเวนต์ที่ถูกเรียกเมื่อผู้เล่นพ่ายแพ้หรือถูกกำจัด </para>
        /// <para> (EN) : Event invoked when a player is defeated or eliminated. </para>
        /// </summary>
        public event Action<Character> OnPlayerDefeated;

        #endregion //Public Events

        #region Public Methods

        public void RaiseEventDefeated(Character player)
        {
            OnPlayerDefeated?.Invoke(player);
        }

        #endregion //Public Methods
    }
}

