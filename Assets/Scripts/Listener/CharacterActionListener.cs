using UnityEngine;
using Genoverrei.DesignPattern;
using BombGame.EnumSpace;
using BombGame.RecordEventSpace;

namespace BombGame.Controller;

/// <summary>
/// <para> Summary : </para>
/// <para> (TH) : ตัวดักฟังคำสั่งและเหตุการณ์ โดยรับฟังก์ชันตรงเข้า EventBus แบบไม่ต้องใช้ Binding </para>
/// <para> (EN) : Listener for commands and events, subscribing directly to EventBus without Binding. </para>
/// </summary>
[RequireComponent(typeof(MoveController))]
public sealed class CharacterActionListener : MonoBehaviour, ISignalListener, IEventListener
{
    #region Variable

    [Header("Identity")]
    [SerializeField] private Character _myCharacterId;

    [Header("Local Controllers")]
    [SerializeField] private MoveController _moveController;

    [Header("Event Channels (Broadcaster)")]
    [SerializeField] private BombSpawnChannelSO _bombSpawnChannel;

    // ลบ _eventBinding ทิ้งไปได้เลยครับ ไม่ต้องใช้แล้ว!

    #endregion //Variable

    #region Unity Lifecycle

    private void OnValidate()
    {
        if (_moveController == null)  _moveController = this.GetComponent<MoveController>();


    }

    private void OnEnable()
    {
        // ระบุ Type <IEvent> ให้ชัดเจน แล้วโยนฟังก์ชัน OnHandleEvent เข้าไปตรงๆ เลยครับ กริบ!
        EventBus.Instance.Subscribe<IEvent>(OnHandleEvent);
    }

    private void OnDisable()
    {
        // ยกเลิกก็ใช้การโยนฟังก์ชันเดิมเข้าไปตรงๆ
        EventBus.Instance.Unsubscribe<IEvent>(OnHandleEvent);
    }

    #endregion //Unity Lifecycle

    #region Interface Implementation (IEventListener)

    public void OnHandleEvent(IEvent eventData)
    {
        switch (eventData)
        {
            case ISignal signal:
                OnHandleSignal(signal);
                break;

            case BombPlantedEvent bombEvent:
                // TODO: บอทคำนวณการหนี
                break;
        }
    }

    #endregion //Interface Implementation

    #region Interface Implementation (ISignalListener)

    public void OnHandleSignal(ISignal signal)
    {
        if (signal.SignalTarget != _myCharacterId) return;

        switch (signal.Action)
        {
            case ActionType.Move:
                if (signal.Event is MoveInputEvent moveData && _moveController != null)
                {
                    _moveController.SetMoveDirection(moveData.Direction);
                }
                break;

            case ActionType.PlaceBomb:
                ExecuteRequestBombSpawn();
                break;
        }
    }

    #endregion //Interface Implementation

    #region Private Logic

    private void ExecuteRequestBombSpawn()
    {
        if (_bombSpawnChannel == null) return;

        Vector2Int gridPos = new Vector2Int(
            Mathf.RoundToInt(transform.position.x),
            Mathf.RoundToInt(transform.position.y)
        );

        _bombSpawnChannel.RaiseEvent(gridPos);
    }

    #endregion //Private Logic
}