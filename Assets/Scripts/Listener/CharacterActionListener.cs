using Genoverrei.DesignPattern;

[RequireComponent(typeof(MoveController))]
public sealed class CharacterActionListener : MonoBehaviour, ISignalListener
{
    #region Variable

    [Header("Identity")]
    [SerializeField] private Character _myCharacterId;

    [Header("Local Controllers")]
    [SerializeField] private MoveController _moveController;

    [Header("Event Channels")]
    [SerializeField] private BombSpawnChannelSO _bombSpawnChannel;

    #endregion //Variable

    #region Unity Lifecycle

    private void OnValidate()
    {
        if (_moveController == null) _moveController = GetComponent<MoveController>();
    }

    private void OnEnable()
    {
        EventBus.Instance.Subscribe<ISignal>(OnHandleSignal);
    }

    private void OnDisable()
    {
        if (EventBus.Instance == null) return;
        EventBus.Instance.Unsubscribe<ISignal>(OnHandleSignal);
    }

    #endregion //Unity Lifecycle

    #region Event Handlers


    public void OnHandleSignal(ISignal signal)
    {
        Debug.Log($"[CharacterActionListener] Received signal: {signal.Action} for {_myCharacterId}");
        switch (signal.Action)
        {
            case ActionType.Move:
                if (signal.Event is MoveInputEvent moveData && _moveController != null)
                {
                    // 🚀 โยนเข้าสมองหลัก (TileMoveAbility) ผ่านฟังก์ชัน Move ของจริง! ไม่มีการ Bypass แล้ว!
                    _moveController.Move(moveData.Direction);
                }
                break;

            case ActionType.PlaceBomb:
                ExecuteRequestBombSpawn();
                break;
        }
    }

    #endregion //Event Handlers

    #region Private Logic

    private void ExecuteRequestBombSpawn()
    {
        if (_bombSpawnChannel == null) return;

        Vector2Int gridPos = new Vector2Int(
            Mathf.RoundToInt(transform.position.x),
            Mathf.RoundToInt(transform.position.y)
        );

        Debug.Log($"[CharacterActionListener] Requesting bomb spawn at {gridPos} from {_myCharacterId}");
        _bombSpawnChannel.RaiseEvent(gridPos);
    }

    #endregion //Private Logic
}