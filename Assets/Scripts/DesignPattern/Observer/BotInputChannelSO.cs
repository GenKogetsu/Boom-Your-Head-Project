using System;

namespace BombGame.RecordEventSpace;

/// <summary>
/// <para> Summary : </para>
/// <para> (TH) : ????????????????? Observer ???????? BotManager ????????????????? InputManager </para>
/// <para> (EN) : Observer communication channel for BotManager to send raw commands to InputManager. </para>
/// </summary>
[CreateAssetMenu(fileName = "BotInputChannel", menuName = "BombGame/Events/Bot Input Channel")]
public sealed class BotInputChannelSO : ScriptableObject
{
    public event Action<Character, ActionType, IEvent> OnBotActionTriggered;

    public void RaiseEvent(Character target, ActionType action, IEvent subEvent = null)
    {
        OnBotActionTriggered?.Invoke(target, action, subEvent);
    }
}