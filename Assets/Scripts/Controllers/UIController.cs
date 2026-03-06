/// <summary>
/// <para> Summary : </para>
/// <para> (TH) : ??????????????? UI ???????????? Event Channel ?????? </para>
/// <para> (EN) : Controls UI display by listening directly to the Event Channel. </para>
/// </summary>
public sealed class UIController : MonoBehaviour
{
    [Header("Event Channels (Listeners)")]
    [SerializeField] private MatchEventChannelSO _matchEventChannel;

    private void OnEnable()
    {
        if (_matchEventChannel != null)
        {
            _matchEventChannel.OnMatchStateChanged += ExecuteHandleMatchState;
        }
    }

    private void OnDisable()
    {
        if (_matchEventChannel != null)
        {
            _matchEventChannel.OnMatchStateChanged -= ExecuteHandleMatchState;
        }
    }

    private void ExecuteHandleMatchState(MatchState state)
    {
        if (state == MatchState.GameOver)
        {
            // ?????????? Game Over
        }
    }
}