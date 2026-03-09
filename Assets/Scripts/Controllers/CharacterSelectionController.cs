using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using NaughtyAttributes;
using UnityEngine.UI;

public sealed class CharacterSelectionController : MonoBehaviour
{
    [Header("UI Slots")]
    [SerializeField] private List<Image> _characterSlots;

    [Header("Pointers")]
    [SerializeField] private Image _p1Pointer;
    [SerializeField] private Image _p2Pointer;
    [SerializeField] private Vector3 _p1Offset;
    [SerializeField] private Vector3 _p2Offset;

    [Header("Current States")]
    [ReadOnly][SerializeField] private int _p1Index = 0;
    [ReadOnly][SerializeField] private int _p2Index = 1;
    [ReadOnly][SerializeField] private bool _p1Ready = false;
    [ReadOnly][SerializeField] private bool _p2Ready = false;

    [Header("Data & Effects")]
    [SerializeField] private GameSessionDataSO _gameSessionData;

    private void Start()
    {
        if (_gameSessionData != null) _gameSessionData.ResetSession();

        RefreshVisuals(1);
        RefreshVisuals(2);
    }

    #region Input Callbacks

    public void OnP1Move(InputAction.CallbackContext context)
    {
        // ใช้ performed เพื่อให้การขยับแม่นยำ 1 ครั้งต่อ 1 การกด
        if (!context.performed || _p1Ready) return;
        ProcessMove(ref _p1Index, context.ReadValue<Vector2>(), 1);
    }

    public void OnP2Move(InputAction.CallbackContext context)
    {
        if (!context.performed || _p2Ready) return;
        ProcessMove(ref _p2Index, context.ReadValue<Vector2>(), 2);
    }

    public void OnP1Confirm(InputAction.CallbackContext context)
    {
        // 🚀 เปลี่ยนจาก started เป็น performed ป้องกันการรันซ้ำในเฟรมเดียว
        if (!context.performed || _p1Ready) return;

        _p1Ready = true;

        // เช็ค null ป้องกัน IndexOutOfRange ถ้าลืมลากของใน Inspector
        if (_characterSlots != null && _p1Index < _characterSlots.Count)
        {
            _characterSlots[_p1Index].color = Color.gray;
        }

        Debug.Log("<color=cyan>P1 Ready!</color>");
        CheckStartGame();
    }

    public void OnP2Confirm(InputAction.CallbackContext context)
    {
        // 🚀 เปลี่ยนจาก started เป็น performed
        if (!context.performed || _p2Ready) return;

        _p2Ready = true;

        if (_characterSlots != null && _p2Index < _characterSlots.Count)
        {
            _characterSlots[_p2Index].color = Color.gray;
        }

        Debug.Log("<color=magenta>P2 Ready!</color>");
        CheckStartGame();
    }

    #endregion

    private void ProcessMove(ref int index, Vector2 moveInput, int playerNum)
    {
        // ลอจิกตาราง 2x2
        if (moveInput.x > 0.5f && index % 2 == 0) index++;
        else if (moveInput.x < -0.5f && index % 2 != 0) index--;
        else if (moveInput.y < -0.5f && index < 2) index += 2;
        else if (moveInput.y > 0.5f && index >= 2) index -= 2;

        RefreshVisuals(playerNum);
    }

    private void RefreshVisuals(int playerNum)
    {
        // เช็ค null ของ Pointer ก่อนขยับ
        if (playerNum == 1 && _p1Pointer != null)
            _p1Pointer.transform.position = _characterSlots[_p1Index].transform.position + _p1Offset;
        else if (playerNum == 2 && _p2Pointer != null)
            _p2Pointer.transform.position = _characterSlots[_p2Index].transform.position + _p2Offset;
    }

    private void CheckStartGame()
    {
        if (_p1Ready && _p2Ready)
        {
            // เปลี่ยนชื่อตัวแปรเป็น selectedList ไม่ให้ทับกับ Enum Character
            List<Character> selectedList = new List<Character>
            {
                (Character)_p1Index,
                (Character)_p2Index
            };

            if (_gameSessionData != null)
            {
                _gameSessionData.SetupMatch(selectedList);
            }

            if (SceneEffectController.Instance != null)
            {
                GetComponent<PlayerInput>().DeactivateInput();
                SceneEffectController.Instance.LoadSceneAndPlayEffect("Level 1");
            }

            Debug.Log("<b><color=lime>[Game Start]</color></b> All players ready! Transitioning...");
        }
    }
}