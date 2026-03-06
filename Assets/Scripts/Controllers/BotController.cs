using UnityEngine;
using System.Collections;
using System.Linq;
using Genoverrei.DesignPattern;

public class BotController : Singleton<BotController>
{/*
    [SerializeField] private PlayerManager _playerManager;
    [SerializeField] private GameSessionData _sessionData;

    [SerializeField] private float _decisionRateStage1 = 0.25f;
    [SerializeField] private float _decisionRateStage2 = 0.12f;

    private float _decisionRate;

    private void Start()
    {
        AdjustDifficulty();
        StartCoroutine(ThinkLoop());
    }

    private void AdjustDifficulty()
    {
        _decisionRate = _sessionData.CurrentStageIndex == 0
            ? _decisionRateStage1
            : _decisionRateStage2;
    }

    private IEnumerator ThinkLoop()
    {
        while (true)
        {
            ProcessAllBots();
            yield return new WaitForSeconds(_decisionRate);
        }
    }

    private void ProcessAllBots()
    {
        if (_playerManager.Bots.Count == 0) return;
        if (_playerManager.Players.Count == 0) return;

        foreach (var bot in _playerManager.Bots)
        {
            HandleBot(bot);
        }
    }

    private void HandleBot(StatsController botStats)
    {
        // หา player ใกล้สุด
        var target = _playerManager.Players
            .OrderBy(p => Vector2.Distance(botStats.transform.position, p.transform.position))
            .FirstOrDefault();

        if (target == null) return;

        Vector2 myPos = botStats.transform.position;
        Vector2 playerPos = target.transform.position;

        Vector2 dir = playerPos - myPos;

        Vector2 moveDir =
            TileMoveAbility<MoveController>.GetDiscreteDirection(dir);

        SendMove(botStats.LivingName, moveDir);

        float distance = Vector2.Distance(myPos, playerPos);

        if (distance <= botStats.CurrentExplosionRange)
        {
            SendBomb(botStats.LivingName);
        }
    }

    private void SendMove(Character target, Vector2 dir)
    {
        CharacterAction action = new()
        {
            SignalTarget = target,
            Action = ActionType.Move,
            Data = dir
        };

        //InputManager.Instance.SendBotSignal(action);
    }

    private void SendBomb(Character target)
    {
        CharacterAction action = new()
        {
            SignalTarget = target,
            Action = ActionType.PlaceBomb,
            Data = true
        };

        //InputManager.Instance.SendBotSignal(action);
    }*/
}