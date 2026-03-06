using UnityEngine;
using Genoverrei.DesignPattern;
using Genoverrei.Libary;
using BombGame.Manager;
using BombGame.RecordEventSpace;
using BombGame.EnumSpace;

namespace BombGame.Controller;

/// <summary>
/// <para> Summary : </para>
/// <para> (TH) : สมองบอทที่คอยถาม PlayerSessionChannel ว่าต้องเข้าไปขับตัวละครตัวไหน </para>
/// <para> (EN) : Bot brain that queries PlayerSessionChannel to determine which character to drive. </para>
/// </summary>
public sealed class BotController : MonoBehaviour, IPathfindable
{
    #region Variable

    [Header("Session Observer")]
    [SerializeField] private PlayerSessionChannelSO _sessionChannel;

    [Header("Assigned Target")]
    [SerializeField] private Character _targetToDrive; // ระบุว่าบอทตัวนี้ถูกสร้างมาเพื่อขับ ID ไหน

    private Transform _bodyTransform; // ร่างที่ได้มาจากการถาม Manager

    [Header("Navigation")]
    [SerializeField] private MapManager _mapManager;
    [SerializeField] private Vector2Int _targetGridPos;

    #endregion //Variable

    #region IPathfindable Implementation

    public Vector2Int CurrentGridPosition => _bodyTransform == null ? Vector2Int.zero : new Vector2Int(
        Mathf.RoundToInt(_bodyTransform.position.x),
        Mathf.RoundToInt(_bodyTransform.position.y)
    );

    public IMapProvider MapProvider => _mapManager;

    Vector2Int IPathfindable.GetNextPath(Vector2Int target) => ExecuteGetNextPath(target);

    #endregion //IPathfindable Implementation

    #region Unity Lifecycle

    private void OnEnable()
    {
        if (_sessionChannel != null)
            _sessionChannel.OnSessionUpdated += ExecuteRefreshBodyReference;

        ExecuteRefreshBodyReference();
    }

    private void OnDisable()
    {
        if (_sessionChannel != null)
            _sessionChannel.OnSessionUpdated -= ExecuteRefreshBodyReference;
    }

    private void Update()
    {
        // ถ้ายังไม่มีร่างให้ขับ ก็ไม่ต้องคิดอะไร
        if (_bodyTransform == null) return;

        ExecuteThink();
    }

    #endregion //Unity Lifecycle

    #region Private Logic

    /// <summary>
    /// <para> (TH) : ถาม Manager ผ่าน ScObj ว่าตัวละครที่ฉันต้องขับ พร้อมหรือยัง </para>
    /// </summary>
    private void ExecuteRefreshBodyReference()
    {
        if (_sessionChannel == null) return;
        _bodyTransform = _sessionChannel.GetBody(_targetToDrive);
    }

    private void ExecuteThink()
    {
        Vector2Int nextStep = ((IPathfindable)this).GetNextPath(_targetGridPos);

        // คำนวณทิศทางจากตำแหน่งปัจจุบันของร่างที่ "ยืม" มาขับ
        Vector2 direction = new Vector2(
            nextStep.x - _bodyTransform.position.x,
            nextStep.y - _bodyTransform.position.y
        );

        // ตะโกนสั่งผ่าน EventBus
        EventBus.Instance.Publish(new CharacterAction(
            _targetToDrive,
            ActionType.Move,
            new MoveInputEvent(direction)
        ));
    }

    private Vector2Int ExecuteGetNextPath(Vector2Int target)
    {
        return PathfindAbility<BotController>.Execute(this, target);
    }

    #endregion //Private Logic
}