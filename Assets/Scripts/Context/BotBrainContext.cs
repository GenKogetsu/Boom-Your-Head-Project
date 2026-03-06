using System;
using System.Collections.Generic;
using UnityEngine;

namespace BombGame.Controller;

/// <summary>
/// <para> Summary : </para>
/// <para> (TH) : บริบทของบอทแต่ละตัว เก็บสถานะ FSM และส่งสัญญาณผ่าน ScriptableObject </para>
/// <para> (EN) : Context for each bot, storing FSM states and sending signals via ScriptableObject. </para>
/// </summary>
public sealed class BotBrainContext
{
    #region Variable

    public Character BotId { get; private set; }

    public Transform BotTransform { get; private set; }

    public IChaseStrategy ChaseStrategy { get; set; }

    public IBombStrategy BombStrategy { get; set; }

    public IItemStrategy ItemStrategy { get; set; }

    private Dictionary<Type, ISate> _stateCache = new();

    private ISate _currentState;

    private readonly CharacterActionChannelSO _actionChannel;

    #endregion //Variable


    #region Constructor

    public BotBrainContext(Character botId, Transform botTransform, CharacterActionChannelSO actionChannel)
    {
        BotId = botId;
        BotTransform = botTransform;
        _actionChannel = actionChannel;
    }

    #endregion //Constructor


    #region Public Methods

    public void ChangeState<T>() where T : class, ISate
    {
        Type stateType = typeof(T);

        if (!_stateCache.ContainsKey(stateType))
        {
            ISate newState = (ISate)Activator.CreateInstance(stateType, this);
            _stateCache.Add(stateType, newState);
        }

        if (_currentState is IExitState exitable) exitable.OnExit();

        _currentState = _stateCache[stateType];

        if (_currentState is IEnterState enterable) enterable.OnEnter();
    }

    public void UpdateTick()
    {
        if (_currentState is IUpdateState updateable) updateable.OnUpdate();
    }

    /// <summary>
    /// <para> Summary : </para>
    /// <para> (TH) : ห่อคำสั่งเป็น CharacterAction แล้วยิงเข้า ScriptableObject Channel </para>
    /// <para> (EN) : Wraps command into CharacterAction and fires it into the ScriptableObject Channel. </para>
    /// </summary>
    public void ExecuteAction(ActionType actionType, IEvent subEvent = null)
    {
        if (_actionChannel != null)
        {
            CharacterAction signal = new CharacterAction(BotId, actionType, subEvent);
            _actionChannel.RaiseEvent(signal);
        }
    }

    #endregion //Public Methods
}