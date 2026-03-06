using NaughtyAttributes;

[RequireComponent(typeof(StatsController))]
[RequireComponent(typeof(MoveController))]
public class CharacterAnimationController : MonoBehaviour
{
    [ReadOnly]
    [SerializeField] private MoveController _moveController;

    [ReadOnly]
    [SerializeField] private Animator _characterAnimator;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (_moveController == null) _moveController = GetComponent<MoveController>();
        if (_characterAnimator == null) _characterAnimator = GetComponentInChildren<Animator>();
    }
#endif

    private void FixedUpdate()
    {
        if (_moveController == null || _characterAnimator == null) return;

        // ดึงทิศทางล่าสุด (เพื่อให้หน้าหันค้างไว้ตอนหยุดเดิน)
        var moveXinput = _moveController.LastMoveDirection.x;
        var moveYinput = _moveController.LastMoveDirection.y;

        // ดึงสถานะการเดินของจริงจาก Interface
        _characterAnimator.SetBool("IsMoving", _moveController.IsMoving);
        _characterAnimator.SetFloat("MoveX", moveXinput);
        _characterAnimator.SetFloat("MoveY", moveYinput);
    }
}