using System;
using UnityEngine;
using Genoverrei.DesignPattern;

namespace BombGame.RecordEventSpace
{
    /// <summary>
    /// <para> (TH) : ช่องทางสื่อสารสำหรับ Bot ทั้งการส่งคำสั่งควบคุม และการรับรู้พฤติกรรมของศัตรู </para>
    /// <para> (EN) : Communication channel for Bots to send commands and perceive enemy actions. </para>
    /// </summary>
    [CreateAssetMenu(fileName = "BotInputChannel", menuName = "BombGame/Events/Bot Input Channel")]
    public sealed class BotInputChannelSO : ScriptableObject
    {
        // 🤖 สำหรับส่งคำสั่งจาก Bot ไปยัง InputManager (เพื่อให้บอทขยับได้)
        public event Action<Character, ActionType, IEvent> OnBotActionTriggered;

        // 👣 สำหรับส่งสัญญาณจากผู้เล่น (Enemy) มาให้ Bot (เพื่อให้บอทฉลาดขึ้น/ดักทางได้)
        public event Action<ISignal> OnEnemyActionTriggered; 

        /// <summary>
        /// (TH) : เรียกใช้เมื่อ Bot ต้องการสั่งให้ตัวละครที่มันคุมขยับหรือวางระเบิด
        /// </summary>
        public void RaiseEvent(Character target, ActionType action, IEvent subEvent = null)
        {
            OnBotActionTriggered?.Invoke(target, action, subEvent);
        }

        /// <summary>
        /// (TH) : เรียกใช้โดย CharacterActionListener เพื่อแจ้งบอทว่าศัตรู (ผู้เล่น) กำลังขยับ
        /// </summary>
        public void RaiseEnemyAction(ISignal signal) // 🚀 ฟังก์ชันที่หายไป เพิ่มให้แล้วครับพี่!
        {
            OnEnemyActionTriggered?.Invoke(signal);
        }
    }
}