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
        // 🤖 ดักฟังคำสั่งจากฝั่งบอท (สำหรับโหมดเล่นกับ AI)
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

    // ------------------ PLAYER 1 ------------------

    public void OnMoveP1(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        BroadcastAction(GetCharacterFromSession(0), ActionType.Move, new MoveInputEvent(input));
    }

    public void OnPlaceBombP1(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            BroadcastAction(GetCharacterFromSession(0), ActionType.PlaceBomb, null);
        }
    }

    // ------------------ PLAYER 2 ------------------

    public void OnMoveP2(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        BroadcastAction(GetCharacterFromSession(1), ActionType.Move, new MoveInputEvent(input));
    }

    public void OnPlaceBombP2(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            BroadcastAction(GetCharacterFromSession(1), ActionType.PlaceBomb, null);
        }
    }

    #endregion // Public Methods

    #region Private Logic

    /// <summary>
    /// <para> (TH) : ดึง Character Enum จาก SelectedPlayers ใน Session Data </para>
    /// </summary>
    private Character GetCharacterFromSession(int index)
    {
        // 🚀 เปลี่ยนจาก SelectedCharacters มาเป็น SelectedPlayers
        if (_sessionData == null || _sessionData.SelectedPlayers == null) return Character.None;

        if (index >= _sessionData.SelectedPlayers.Count) return Character.None;

        return _sessionData.SelectedPlayers[index];
    }

    private void ExecuteHandleBotInput(Character target, ActionType action, IEvent subEvent)
    {
        BroadcastAction(target, action, subEvent);
    }

    private void BroadcastAction(Character target, ActionType actionType, IEvent subEvent)
    {
        if (target == Character.None) return;

        var signal = new CharacterAction(target, actionType, subEvent);

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
        if (action.Event is MoveInputEvent moveData && moveData.Direction == Vector2.zero) return;

        Debug.Log($"<b><color=#4FC3F7>[Input Signal]</color></b> Action: <color=#FFEB3B>{action.Action}</color> | Target: <color=#69F0AE>{action.SignalTarget}</color>");
    }

    #endregion // Private Logic
}