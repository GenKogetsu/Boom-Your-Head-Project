using Genoverrei.DesignPattern;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
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

    public void OnPressAnyKey(InputAction.CallbackContext context)
    {
        if (!context.started) return;

        ChangedState("ToMainMenu");
    }

    public void OnClickStartButton()
    {
        ChangedState("ToPlayeMode");
    }

    public void OnClickCreditButton()
    {
        ChangedState("ToCredit");
    }

    public void OnClickExitButton()
    {
        Debug.Log("Exit Game");

        if (SceneEffectController.Instance == null) return;

        SceneEffectController.Instance.QuitGameAfterPlayEffect();
    }

    public void OnClickSiglePlayer()
    {
        if (SceneEffectController.Instance == null) return;
        
        SceneEffectController.Instance.LoadSceneAndPlayEffect("ChooseCharacterScene");
        
    }

    public void OnClickMultiPlayer()
    {
        if (SceneEffectController.Instance == null) return;
        
        SceneEffectController.Instance.LoadSceneAndPlayEffect("ChooseCharacterScene");
    }

    private void ChangedState(string state) 
    {
        if (SceneEffectController.Instance.SceneEffectCoroutine != null) return;

        _animator.SetBool("ToMainMenu", state == "ToMainMenu" ? true : false);
        _animator.SetBool("ToPlayeMode", state == "ToPlayeMode" ? true : false);
        _animator.SetBool("ToCredit", state == "ToCredit" ? true : false);
    }
}
