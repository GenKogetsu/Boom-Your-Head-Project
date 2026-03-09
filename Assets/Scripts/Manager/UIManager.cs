using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Channel & Data")]
    [SerializeField] private PlayerStatsChannelSO _statsChannel;
    [SerializeField] private GameSessionDataSO _sessionData;

    [Header("UI Slots")]
    [SerializeField] private PlayerUiSlot Ryuwen, Edigan, Baboon, Terbi;

    private void OnEnable()
    {
        if (_statsChannel != null)
            _statsChannel.OnStatsUpdated += HandleStatsUpdated;
    }

    private void OnDisable()
    {
        if (_statsChannel != null)
            _statsChannel.OnStatsUpdated -= HandleStatsUpdated;
    }



    private void HandleStatsUpdated(StatsChangeEvent OnStatsChange)
    {
        switch (OnStatsChange.CharacterName)
        {
            case Character.Ryuwen:
                UpdateCharacterUI(Ryuwen, OnStatsChange);
                break;

            case Character.Edigan:
                UpdateCharacterUI(Edigan, OnStatsChange);
                break;

            case Character.Baboon:
                UpdateCharacterUI(Baboon, OnStatsChange);
                break;

            case Character.Terbi:
                UpdateCharacterUI(Terbi, OnStatsChange);
                break;
        }
    }

    private void UpdateCharacterUI(PlayerUiSlot slot, StatsChangeEvent data)
    {
        if (slot == null) return;

        slot.HpDisplay.text = data.Hp.ToString();
        slot.BombDisplay.text = data.BombAmount.ToString();
        slot.SpeedDisplay.text = data.Speed.ToString("F0");
        slot.RangeDisplay.text = data.Range.ToString();

        if (data.Hp <= 0)
        {
            slot.OutImage.gameObject.SetActive(true);
        }
    }
}