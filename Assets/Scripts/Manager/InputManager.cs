using UnityEngine;
using UnityEngine.InputSystem;
using Genoverrei.DesignPattern;
using NaughtyAttributes;

/// <summary>
/// <para> Summary : </para>
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
    [SerializeField] private GameSessionData _sessionData;

    #endregion //Variable

    #region Unity Lifecycle

    private void OnEnable()
    {
        // ดักฟังคำสั่งจากฝั่งบอท
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

    #region Public Methods (For Player Input)

    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        BroadcastAction(_sessionData.FirstPlayerCharacter, ActionType.Move, new MoveInputEvent(input));
    }

    public void OnPlaceBomb(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            BroadcastAction(_sessionData.FirstPlayerCharacter, ActionType.PlaceBomb, null);
        }
    }

    #endregion //Public Methods

    #region Private Logic

    /// <summary>
    /// <para> Summary : </para>
    /// <para> (TH) : รับคำสั่งที่ผ่านการคิดจากฝั่งบอท แล้วส่งต่อไปยัง EventBus </para>
    /// <para> (EN) : Receives processed commands from bots and forwards them to EventBus. </para>
    /// </summary>
    private void ExecuteHandleBotInput(Character target, ActionType action, IEvent subEvent)
    {
        BroadcastAction(target, action, subEvent);
    }

    /// <summary>
    /// <para> Summary : </para>
    /// <para> (TH) : แปลงข้อมูลเป็น CharacterAction แล้วกระจายผ่าน EventBus ตัวเดียวจบ </para>
    /// <para> (EN) : Converts data into CharacterAction and broadcasts via a single EventBus. </para>
    /// </summary>
    private void BroadcastAction(Character target, ActionType actionType, IEvent subEvent)
    {
        if (_sessionData == null) return;

        // สร้างข้อมูล action
        var signal = new CharacterAction(target, actionType, subEvent);

        // 🚀 ต้องใส่ <ISignal> ตรงนี้เพื่อให้มันไปโผล่ที่ Listener ที่รอฟัง ISignal อยู่ครับ
        EventBus.Instance.Publish<ISignal>(signal);

#if UNITY_EDITOR
        LogAction(signal);
#endif
    }

    private void LogAction(CharacterAction action)
    {
        if (action.Event is MoveInputEvent moveData && moveData.Direction == Vector2.zero) return;
        Debug.Log($"<color=#4FC3F7>[Input Signal]</color> Action: {action.Action} | Target: {action.SignalTarget}");
    }

    #endregion //Private Logic
}