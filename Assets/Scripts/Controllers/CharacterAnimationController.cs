using NaughtyAttributes;
using UnityEngine;

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
        if (_moveController == null) _moveController = this.GetComponent<MoveController>();
        if (_characterAnimator == null) _characterAnimator = this.GetComponentInChildren<Animator>();
    }
#endif

    private void FixedUpdate()
    {
        var moveXinput = _moveController.LastMoveDirection.x;
        var moveYinput = _moveController.LastMoveDirection.y;

        _characterAnimator.SetBool("IsMoving", _moveController.IsMoving);

        _characterAnimator.SetFloat("MoveX", moveXinput);
        _characterAnimator.SetFloat("MoveY", moveYinput);
    }
}