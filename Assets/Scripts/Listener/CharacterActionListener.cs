using UnityEngine;
using Genoverrei.DesignPattern;
using Genoverrei.Libary;

/// <summary>
/// <para> (TH) : ตัวรับสัญญาณ Action และคัดกรองเป้าหมาย หากไม่ใช่ของตัวเองจะส่งต่อให้บอทเพื่อคำนวณดักทาง </para>
/// <para> (EN) : Character action listener that filters signals; if not the target, forwards to bots for input prediction. </para>
/// </summary>
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

    [Tooltip("(TH) : ช่องทางส่งต่อข้อมูล Input ของคนอื่นให้ Bot เอาไปคำนวณดักทาง")]
    [SerializeField] private BotInputChannelSO _botInputChannel; // 🚀 เพิ่ม Observer สำหรับส่งค่าให้บอท

    #endregion //Variable

    #region Unity Lifecycle

    private void OnValidate()
    {
        if (_moveController == null) _moveController = GetComponent<MoveController>();
    }

    private void OnEnable()
    {
        if (EventBus.Instance != null)
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
        // 🚀 1. ตรวจสอบเป้าหมาย (Identity Check)
        if (signal.SignalTarget != _myCharacterId)
        {
            // 🤖 ถ้าไม่ใช่ของเรา -> ส่งต่อให้บอทเอาไปคำนวณ "ดักทาง"
            ExecuteForwardToBot(signal);
            return;
        }

        // ✅ 2. ถ้าใช่ของเรา -> ทำงานตามปกติ
        switch (signal.Action)
        {
            case ActionType.Move:
                if (signal.Event is MoveInputEvent moveData && _moveController != null)
                {
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

        Debug.Log($"<b><color=#FF5252>[Action]</color></b> Requesting bomb spawn at {gridPos} from {_myCharacterId}");
        _bombSpawnChannel.RaiseEvent(gridPos);
    }

    /// <summary>
    /// <para> (TH) : ส่งต่อข้อมูลการเคลื่อนไหวหรือการกระทำของ "ผู้อื่น" ให้บอทผ่าน Channel </para>
    /// </summary>
    private void ExecuteForwardToBot(ISignal signal)
    {
        if (_botInputChannel == null) return;

        // 🚀 เรียก Event ใน Channel เพื่อแจ้งเตือนบอทว่ามีคนขยับ
        // (พี่ต้องไปเพิ่มฟังก์ชัน RaiseEnemyAction(signal) ใน BotInputChannelSO ด้วยนะครับ)
        _botInputChannel.RaiseEnemyAction(signal);
    }

    #endregion //Private Logic
}