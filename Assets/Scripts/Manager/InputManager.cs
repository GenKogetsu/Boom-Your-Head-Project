using UnityEngine;
using UnityEngine.InputSystem;
using Genoverrei.DesignPattern;
using NaughtyAttributes;

namespace BombGame.Manager;

/// <summary>
/// <para> summary : </para>
/// <para> (TH) : ตัวจัดการ Input ที่รอรับสัญญาณจาก Player Input Component เพื่อส่ง CharacterAction (วางระเบิด/เคลื่อนที่) </para>
/// <para> (EN) : Input manager that waits for Player Input Component signals to broadcast CharacterAction (PlaceBomb/Move). </para>
/// </summary>
public sealed class InputManager : Singleton<InputManager>
{
    #region Variable

    [Header("Session Reference")]
    [Required]
    [SerializeField] private GameSessionData _sessionData;

    #endregion //Variable

    #region Public Methods (For Player Input Events)

    /// <summary>
    /// <para> summary : </para>
    /// <para> (TH) : รับค่าจาก Action "Move" เพื่อส่งสัญญาณเคลื่อนที่ </para>
    /// <para> (EN) : Receives value from "Move" action to broadcast move signal. </para>
    /// </summary>
    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        BroadcastAction(ActionType.Move, new MoveInputEvent(input));
    }

    /// <summary>
    /// <para> summary : </para>
    /// <para> (TH) : รับค่าจาก Action "PlaceBomb" เพื่อส่งสัญญาณ "วางระเบิด" </para>
    /// <para> (EN) : Receives value from "PlaceBomb" action to broadcast place bomb signal. </para>
    /// </summary>
    public void OnPlaceBomb(InputAction.CallbackContext context)
    {
        // ทำงานเฉพาะตอนเริ่มกด (Started) หรือ กดเสร็จสิ้น (Performed) ตามความเหมาะสมของ Input System
        if (context.performed)
        {
            BroadcastAction(ActionType.PlaceBomb, null);
        }
    }

    #endregion //Public Methods

    #region Private Logic

    /// <summary>
    /// <para> summary : </para>
    /// <para> (TH) : ห่อหุ้ม IEvent ลงใน CharacterAction แล้วส่งออกไปผ่าน EventBus </para>
    /// <para> (EN) : Wraps IEvent into CharacterAction and publishes via EventBus. </para>
    /// </summary>
    private void BroadcastAction(ActionType actionType, IEvent subEvent)
    {
        if (_sessionData == null) return;

        CharacterAction signal = new CharacterAction(
            _sessionData.FirstPlayerCharacter,
            actionType,
            subEvent
        );

        EventBus.Instance.Publish(signal);

#if UNITY_EDITOR
        LogAction(signal);
#endif
    }

    private void LogAction(CharacterAction action)
    {
        if (action.Event is MoveInputEvent moveData && moveData.Direction == Vector2.zero) return;

        Debug.Log($"<b><color=#4FC3F7>[Input Signal]</color></b> Action: {action.Action} | Target: {action.SignalTarget}");
    }

    #endregion //Private Logic
}