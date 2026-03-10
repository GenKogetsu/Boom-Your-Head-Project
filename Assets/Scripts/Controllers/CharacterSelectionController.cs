using System.Collections;
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

    [Header("Settings")]
    [SerializeField] private float _inputDelay = 1.0f;
    private bool _canControl = false;

    [Header("Data Reference")]
    [SerializeField] private GameSessionDataSO _sessionData;

    private void Start()
    {
        // 🚀 เช็คโหมดจาก SO: ถ้า PlayerCount เป็น 1 ให้ซ่อน Pointer P2 ทันที
        if (_sessionData != null)
        {
            if (_p1Pointer != null) _p1Pointer.gameObject.SetActive(true);
            if (_p2Pointer != null) _p2Pointer.gameObject.SetActive(_sessionData.PlayerCount > 1);

            // ถ้าเล่นคนเดียว ให้ P2 Ready หลอกๆ ไว้เลย เพื่อให้เข้าเงื่อนไข StartGame
            if (_sessionData.PlayerCount == 1) _p2Ready = true;
        }

        RefreshVisuals(1);
        if (_sessionData.PlayerCount > 1) RefreshVisuals(2);

        StartCoroutine(EnableInputRoutine());
    }

    private IEnumerator EnableInputRoutine()
    {
        _canControl = false;
        yield return new WaitForSeconds(_inputDelay);
        _canControl = true;
    }

    #region Input Callbacks (เช็ค _canControl ทุกอัน)

    public void OnP1Move(InputAction.CallbackContext context)
    {
        if (!_canControl || !context.performed || _p1Ready) return;
        ProcessMove(ref _p1Index, context.ReadValue<Vector2>(), 1);
    }

    public void OnP2Move(InputAction.CallbackContext context)
    {
        // 🚀 ถ้า SO บอกว่าเล่นคนเดียว ไม่ต้องรับ Input P2
        if (!_canControl || _sessionData.PlayerCount <= 1 || !context.performed || _p2Ready) return;
        ProcessMove(ref _p2Index, context.ReadValue<Vector2>(), 2);
    }

    public void OnP1Confirm(InputAction.CallbackContext context)
    {
        if (!_canControl || !context.performed || _p1Ready) return;
        _p1Ready = true;
        if (_characterSlots != null) _characterSlots[_p1Index].color = Color.gray;
        CheckStartGame();
    }

    public void OnP2Confirm(InputAction.CallbackContext context)
    {
        if (!_canControl || _sessionData.PlayerCount <= 1 || !context.performed || _p2Ready) return;
        _p2Ready = true;
        if (_characterSlots != null) _characterSlots[_p2Index].color = Color.gray;
        CheckStartGame();
    }

    #endregion

    private void ProcessMove(ref int index, Vector2 moveInput, int playerNum)
    {
        if (moveInput.x > 0.5f && index % 2 == 0) index++;
        else if (moveInput.x < -0.5f && index % 2 != 0) index--;
        else if (moveInput.y < -0.5f && index < 2) index += 2;
        else if (moveInput.y > 0.5f && index >= 2) index -= 2;
        RefreshVisuals(playerNum);
    }

    private void RefreshVisuals(int playerNum)
    {
        if (playerNum == 1) _p1Pointer.transform.position = _characterSlots[_p1Index].transform.position + _p1Offset;
        else if (playerNum == 2) _p2Pointer.transform.position = _characterSlots[_p2Index].transform.position + _p2Offset;
    }

    private void CheckStartGame()
    {
        if (_p1Ready && _p2Ready)
        {
            List<Character> selected = new List<Character>();
            selected.Add((Character)_p1Index);
            if (_sessionData.PlayerCount > 1) selected.Add((Character)_p2Index);

            // 🚀 เรียก SetupMatch ใน SO เพื่อบันทึกคนเล่นและเติมบอท
            _sessionData.SetupMatch(selected);

            if (SceneEffectController.Instance != null)
            {
                if (TryGetComponent<PlayerInput>(out var input)) input.DeactivateInput();
                SceneEffectController.Instance.LoadSceneAndPlayEffect("Level 1");
            }
        }
    }
}