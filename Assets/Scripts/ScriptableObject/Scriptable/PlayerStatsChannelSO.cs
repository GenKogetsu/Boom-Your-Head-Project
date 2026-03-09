
using System;
using static UnityEngine.CullingGroup;

[CreateAssetMenu(fileName = "PlayerStatsChannel", menuName = "BombGame/Channels/Player Stats Channel")]
public class PlayerStatsChannelSO : ScriptableObject
{
    public event Action<StatsChangeEvent> OnStatsUpdated;

    public void RaiseEvent(StatsChangeEvent payload)
    {
        OnStatsUpdated?.Invoke(payload);
    }
}