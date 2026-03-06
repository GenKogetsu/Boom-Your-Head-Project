using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class MainMenuController : MonoBehaviour
{
    [ReadOnly]
    [SerializeField] private Animator _animator;

    private void OnValidate()
    {
        if (_animator == null) _animator = this.GetComponent<Animator>();

    }

    public void OnPassAnyKey(InputAction.CallbackContext context)
    {
        if (!context.started) return;

        _animator.SetTrigger("ToMainMenu");
    }

    public void OnClickStartButton()
    {
        _animator.SetTrigger("ToPlayeMode");
    }

    public void OnClickCreditButton()
    {
        _animator.SetTrigger("ToCredit");
    }

    public void OnClickExitButton()
    {
        Debug.Log("Exit Game");
        Application.Quit();
    }


}
