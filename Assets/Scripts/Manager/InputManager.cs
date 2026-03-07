using UnityEngine;
using UnityEngine.InputSystem;
using Genoverrei.DesignPattern;
using NaughtyAttributes;

/// <summary>
/// <para> (TH) : ศูนย์กลางรวบรวมคำสั่งทั้งหมด (ผู้เล่นและบอท) เพื่อกระจาย CharacterAction เข้าสู่ EventBus </para>
/// <para> (EN) : Central hub aggregating all commands (player and bot) to broadcast CharacterAction to EventBus. </para>
/// </summary>
public sealed class InputManager : MonoBehaviour
{
    #region Variable

    [Header("Bot Communication")]
    [SerializeField] private BotInputChannelSO _botInputChannel;

    [Header("Session Reference")]
    [Required]
    [SerializeField] private GameSessionDataSO _sessionData;

    #endregion //Variable

    #region Unity Lifecycle

    private void OnEnable()
    {
        // 🤖 ดักฟังคำสั่งจากฝั่งบอท
        if (_botInputChannel != null)
        {
            _botInputChannel.OnBotActionTriggered += ExecuteHandleBotInput;
        }
    }

    private void OnDisable()
    {
        if (_botInputChannel != null)
        {
            _botInputChannel.OnBotActionTriggered -= ExecuteHandleBotInput;
        }
    }

    #endregion //Unity Lifecycle

    #region Public Methods (For Player Input System)

    /// <summary>
    /// <para> (TH) : รับ Input การเคลื่อนที่จาก Player 1 </para>
    /// </summary>
    public void OnMoveP1(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        BroadcastAction(GetCharacterFromSession(0), ActionType.Move, new MoveInputEvent(input));
    }

    /// <summary>
    /// <para> (TH) : รับ Input การวางระเบิดจาก Player 1 </para>
    /// </summary>
    public void OnPlaceBombP1(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            BroadcastAction(GetCharacterFromSession(0), ActionType.PlaceBomb, null);
        }
    }

    /// <summary>
    /// <para> (TH) : รับ Input การเคลื่อนที่จาก Player 2 (ถ้ามี) </para>
    /// </summary>
    public void OnMoveP2(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        BroadcastAction(GetCharacterFromSession(1), ActionType.Move, new MoveInputEvent(input));
    }

    #endregion // Public Methods

    #region Private Logic

    /// <summary>
    /// <para> (TH) : ดึง Character Enum จาก Session Data ตามลำดับ index เพื่อป้องกัน Error </para>
    /// </summary>
    private Character GetCharacterFromSession(int index)
    {
        if (_sessionData == null || _sessionData.SelectedCharacters.Count <= index)
            return Character.None; // หรือ Default ที่พี่ตั้งไว้

        return _sessionData.SelectedCharacters[index];
    }

    /// <summary>
    /// <para> (TH) : รับคำสั่งที่ผ่านการคิดจากฝั่งบอท แล้วส่งต่อไปยัง EventBus </para>
    /// </summary>
    private void ExecuteHandleBotInput(Character target, ActionType action, IEvent subEvent)
    {
        BroadcastAction(target, action, subEvent);
    }

    /// <summary>
    /// <para> (TH) : แปลงข้อมูลเป็น CharacterAction แล้วกระจายผ่าน EventBus ตัวเดียวจบ </para>
    /// </summary>
    private void BroadcastAction(Character target, ActionType actionType, IEvent subEvent)
    {
        if (target == Character.None) return;

        // สร้างข้อมูล action (Signal)
        var signal = new CharacterAction(target, actionType, subEvent);

        // 🚀 ส่งสัญญาณผ่าน EventBus โดยใช้ Interface ISignal เพื่อให้คลาสอื่นๆ (เช่น StatsController/BombManager) รอรับได้
        if (EventBus.Instance != null)
        {
            EventBus.Instance.Publish<ISignal>(signal);
        }

#if UNITY_EDITOR
        LogAction(signal);
#endif
    }

    private void LogAction(CharacterAction action)
    {
        // ไม่ Log ตอนหยุดเดินเพื่อลดขยะใน Console
        if (action.Event is MoveInputEvent moveData && moveData.Direction == Vector2.zero) return;

        Debug.Log($"<b><color=#4FC3F7>[Input Signal]</color></b> Action: <color=#FFEB3B>{action.Action}</color> | Target: <color=#69F0AE>{action.SignalTarget}</color>");
    }

    #endregion // Private Logic
}