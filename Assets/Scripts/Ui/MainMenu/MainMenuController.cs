using Genoverrei.DesignPattern;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator))]
public class MainMenuController : MonoBehaviour
{
    [Header("Data Reference")]
    [SerializeField] private GameSessionDataSO _sessionData; // 🚀 ลาก SO ใส่ตรงนี้

    [ReadOnly]
    [SerializeField] private Animator _animator;

    private bool _isStarting = false;

    private void OnValidate()
    {
        if (_animator == null) _animator = this.GetComponent<Animator>();
    }

    public void OnPressAnyKey(InputAction.CallbackContext context)
    {
        if (!context.started || _isStarting) return;
        ChangedState("ToMainMenu");
    }

    public void OnClickStartButton() => ChangedState("ToPlayeMode");
    public void OnClickCreditButton() => ChangedState("ToCredit");

    public void OnClickExitButton()
    {
        if (_isStarting) return;
        SceneEffectController.Instance.QuitGameAfterPlayEffect();
    }

    // 🚀 เล่นคนเดียว: เซ็ตค่าเป็น 1
    public void OnClickSiglePlayer()
    {
        if (_sessionData != null) _sessionData.PlayerCount = 1;
        StartGameTransition();
    }

    // 🚀 เล่นสองคน: เซ็ตค่าเป็น 2
    public void OnClickMultiPlayer()
    {
        if (_sessionData != null) _sessionData.PlayerCount = 2;
        StartGameTransition();
    }

    private void StartGameTransition()
    {
        if (_isStarting) return;
        _isStarting = true;
        ResetAllBools();
        SceneEffectController.Instance.LoadSceneAndPlayEffect("ChooseCharacterScene");
    }

    private void ChangedState(string state)
    {
        if (SceneEffectController.Instance.HaveSceneEffectCoroutine || _isStarting) return;
        ResetAllBools();
        if (state == "ToMainMenu") _animator.SetBool("ToMainMenu", true);
        if (state == "ToPlayeMode") _animator.SetBool("ToPlayeMode", true);
        if (state == "ToCredit") _animator.SetBool("ToCredit", true);
    }

    private void ResetAllBools()
    {
        _animator.SetBool("ToMainMenu", false);
        _animator.SetBool("ToPlayeMode", false);
        _animator.SetBool("ToCredit", false);
    }
}