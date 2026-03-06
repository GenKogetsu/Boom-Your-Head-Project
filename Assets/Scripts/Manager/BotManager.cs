namespace BombGame.Manager;

/// <summary>
/// <para> Summary : </para>
/// <para> (TH) : ผู้จัดการสมองบอท รัน FSM และส่งคำสั่งควบคุมบอทแต่ละตัวผ่าน Event Channel </para>
/// <para> (EN) : Bot brain manager running FSM and sending bot control commands via Event Channel. </para>
/// </summary>
public sealed class BotManager : MonoBehaviour
{
    #region Variable

    [Header("Event Channels")]
    [SerializeField] private CharacterActionChannelSO _actionChannelSO;

    [SerializeField] private MatchEventChannelSO _matchEventChannelSO;

    private List<BotBrainContext> _activeBots = new();

    private bool _canBotsThink = false;

    #endregion //Variable

    #region Unity Lifecycle

    private void OnEnable()
    {
        if (_matchEventChannelSO != null)
        {
            _matchEventChannelSO.OnMatchStateChanged += ExecuteHandleMatchState;
        }
    }

    private void OnDisable()
    {
        if (_matchEventChannelSO != null)
        {
            _matchEventChannelSO.OnMatchStateChanged -= ExecuteHandleMatchState;
        }
    }

    private void Update()
    {
        if (!_canBotsThink) return;

        foreach (var bot in _activeBots)
        {
            bot.UpdateTick();
        }
    }

    #endregion //Unity Lifecycle

    #region Public Methods

    /// <summary>
    /// <para> Summary : </para>
    /// <para> (TH) : สร้างและลงทะเบียนสมองบอทเข้าสู่ระบบส่วนกลาง </para>
    /// <para> (EN) : Creates and registers a bot brain into the central system. </para>
    /// </summary>
    public void RegisterBot(Character botId, Transform botTransform)
    {
        if (_activeBots.Count >= 3) return;

        var brain = new BotBrainContext(botId, botTransform, _actionChannelSO);
        _activeBots.Add(brain);
    }

    #endregion //Public Methods

    #region Private Logic

    /// <summary>
    /// <para> Summary : </para>
    /// <para> (TH) : เปิดปิดการทำงานของบอทตามสถานะของการแข่งขัน </para>
    /// <para> (EN) : Toggles bot operation based on the match state. </para>
    /// </summary>
    private void ExecuteHandleMatchState(MatchState state)
    {
        _canBotsThink = (state == MatchState.Playing);
    }

    #endregion //Private Logic
}