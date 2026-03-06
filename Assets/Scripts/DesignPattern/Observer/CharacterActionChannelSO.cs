using System;
using UnityEngine;

namespace BombGame.RecordEventSpace
{
    /// <summary>
    /// <para> Summary : </para>
    /// <para> (TH) : ช่องทางส่งสัญญาณการกระทำของตัวละครผ่าน ScriptableObject </para>
    /// <para> (EN) : ScriptableObject channel for broadcasting character actions. </para>
    /// </summary>
    [CreateAssetMenu(fileName = "CharacterActionChannel", menuName = "BombGame/Events/Character Action Channel")]
    public sealed class CharacterActionChannelSO : ScriptableObject
    {
        #region Public Events

        public event Action<CharacterAction> OnActionTriggered;

        #endregion //Public Events


        #region Public Methods

        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : กระจายสัญญาณ CharacterAction ไปยังผู้ที่ดักฟังทั้งหมด </para>
        /// <para> (EN) : Broadcasts the CharacterAction signal to all listeners. </para>
        /// </summary>
        public void RaiseEvent(CharacterAction action)
        {
            OnActionTriggered?.Invoke(action);
        }

        #endregion //Public Methods
    }
}

