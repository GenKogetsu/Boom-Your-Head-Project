namespace BombGame.Manager;

/// <summary>
/// <para> Summary : </para>
/// <para> (TH) : จัดการสถานะและกฎกติกาของการแข่งขันแบบไร้ Singleton </para>
/// <para> (EN) : Manages match states and rules without a Singleton. </para>
/// </summary>
public sealed class MatchManager : MonoBehaviour
{
    #region Variable

    [Header("Event Broadcasters")]
    [SerializeField] private MatchEventChannelSO _matchEventChannel;

    private MatchState _currentState;

    #endregion //Variable

    #region Unity Lifecycle

    private void Start()
    {
        ExecuteChangeState(MatchState.Starting);
        // จำลองการเริ่มเกม
        Invoke(nameof(ExecuteStartGame), 3f);
    }

    #endregion //Unity Lifecycle

    #region Private Logic

    private void ExecuteStartGame() => ExecuteChangeState(MatchState.Playing);

    /// <summary>
    /// <para> Summary : </para>
    /// <para> (TH) : เปลี่ยนสถานะและส่งสัญญาณบอกทุกระบบในเกมผ่าน SO </para>
    /// <para> (EN) : Changes state and broadcasts to all systems via SO. </para>
    /// </summary>
    private void ExecuteChangeState(MatchState newState)
    {
        _currentState = newState;

        if (_matchEventChannel != null)
        {
            _matchEventChannel.RaiseEvent(_currentState);
        }
    }

    #endregion //Private Logic
}