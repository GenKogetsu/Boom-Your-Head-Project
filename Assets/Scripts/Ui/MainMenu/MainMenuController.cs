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

    public void OnPassAnyKey(InputAction.CallbackContext context)
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
        Application.Quit();
    }

    public void OnClickSiglePlayer()
    {
        //เขียนค่าคนเดียว
        SceneManager.LoadScene("ChooseCharacterScene");
    }

    public void OnClickMultiPlayer()
    {
        //เขียนค่าเล่นหลายคน
        SceneManager.LoadScene("ChooseCharacterScene");
    }

    private void ChangedState(string state) 
    {
        _animator.SetBool("ToMainMenu", state == "ToMainMenu" ? true : false);
        _animator.SetBool("ToPlayeMode", state == "ToPlayeMode" ? true : false);
        _animator.SetBool("ToCredit", state == "ToCredit" ? true : false);
    }
}
