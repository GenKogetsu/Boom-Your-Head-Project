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
    [SerializeField] private float _inputDelay = 1f;

    private bool _canControl = false;

    [Header("Data Reference")]
    [SerializeField] private GameSessionDataSO _sessionData;

    private bool IsSinglePlayer => _sessionData != null && _sessionData.PlayerCount == 1;

    private Character GetCharacterFromIndex(int index)
    {
        return (Character)(index + 1);
    }

    private void Start()
    {
        if (_sessionData == null)
        {
            Debug.LogError("GameSessionDataSO is missing!");
            return;
        }

        SetupPointers();
        StartCoroutine(EnableInputRoutine());
    }

    private void SetupPointers()
    {
        if (_p1Pointer != null)
            _p1Pointer.gameObject.SetActive(true);

        if (_p2Pointer != null)
            _p2Pointer.gameObject.SetActive(!IsSinglePlayer);

        if (IsSinglePlayer)
        {
            _p2Ready = true;
            _p2Index = -1;
        }

        RefreshVisuals(1);

        if (!IsSinglePlayer)
            RefreshVisuals(2);
    }

    private IEnumerator EnableInputRoutine()
    {
        _canControl = false;
        yield return new WaitForSeconds(_inputDelay);
        _canControl = true;
    }

    // ✅ รวม P1 และ P2 Move เป็นเมธอด Generic
    public void OnP1Move(InputAction.CallbackContext context)
    {
        if (!_canControl || !context.performed || _p1Ready) return;
        ProcessMove(ref _p1Index, context.ReadValue<Vector2>(), 1);
    }

    public void OnP2Move(InputAction.CallbackContext context)
    {
        if (!_canControl || IsSinglePlayer || !context.performed || _p2Ready) return;
        ProcessMove(ref _p2Index, context.ReadValue<Vector2>(), 2);
    }

    // ✅ รวม P1 และ P2 Confirm เป็นเมธอด Generic
    public void OnP1Confirm(InputAction.CallbackContext context)
    {
        if (!_canControl || !context.performed || _p1Ready) return;

        if (!IsSinglePlayer && _p1Index == _p2Index)
            return;

        ConfirmSelection(1);
    }

    public void OnP2Confirm(InputAction.CallbackContext context)
    {
        if (!_canControl || IsSinglePlayer || !context.performed || _p2Ready) return;

        if (_p1Index == _p2Index)
            return;

        ConfirmSelection(2);
    }

    // ✅ รวม P1 และ P2 Cancel เป็นเมธอด Generic
    public void OnP1Cancel(InputAction.CallbackContext context)
    {
        if (!_canControl || !context.performed) return;
        CancelSelection(1);
    }

    public void OnP2Cancel(InputAction.CallbackContext context)
    {
        if (!_canControl || IsSinglePlayer || !context.performed) return;
        CancelSelection(2);
    }

    private void ProcessMove(ref int index, Vector2 moveInput, int playerNum)
    {
        int newIndex = index;

        if (moveInput.x > 0.5f && index % 2 == 0)
            newIndex++;
        else if (moveInput.x < -0.5f && index % 2 != 0)
            newIndex--;
        else if (moveInput.y < -0.5f && index < 2)
            newIndex += 2;
        else if (moveInput.y > 0.5f && index >= 2)
            newIndex -= 2;

        if (!IsSinglePlayer)
        {
            if (playerNum == 1 && newIndex == _p2Index) return;
            if (playerNum == 2 && newIndex == _p1Index) return;
        }

        index = newIndex;
        RefreshVisuals(playerNum);
    }

    // ✅ Generic ConfirmSelection
    private void ConfirmSelection(int playerNum)
    {
        if (playerNum == 1)
        {
            _p1Ready = true;
            SetSlotLocked(_p1Index);
        }
        else if (playerNum == 2)
        {
            _p2Ready = true;
            SetSlotLocked(_p2Index);
        }

        CheckStartGame();
    }

    // ✅ Generic CancelSelection
    private void CancelSelection(int playerNum)
    {
        if (playerNum == 1 && _p1Ready)
        {
            _p1Ready = false;
            UnlockSlot(_p1Index);
        }
        else if (playerNum == 2 && _p2Ready)
        {
            _p2Ready = false;
            UnlockSlot(_p2Index);
        }
    }

    private void RefreshVisuals(int playerNum)
    {
        if (_characterSlots == null || _characterSlots.Count == 0) return;

        Image pointer = playerNum == 1 ? _p1Pointer : _p2Pointer;
        int index = playerNum == 1 ? _p1Index : _p2Index;
        Vector3 offset = playerNum == 1 ? _p1Offset : _p2Offset;

        if (pointer != null)
        {
            pointer.transform.position = _characterSlots[index].transform.position + offset;
        }
    }

    private void SetSlotLocked(int index)
    {
        if (_characterSlots == null || index < 0 || index >= _characterSlots.Count) return;
        _characterSlots[index].color = Color.gray;
    }

    private void UnlockSlot(int index)
    {
        if (_characterSlots == null || index < 0 || index >= _characterSlots.Count) return;
        _characterSlots[index].color = Color.white;
    }

    private void CheckStartGame()
    {
        if (!_p1Ready || !_p2Ready) return;

        List<Character> selected = new List<Character>();
        List<int> selectedIndices = new List<int>(); // ✅ เก็บ index

        selected.Add(_sessionData.GetCharacterFromLibraryIndex(_p1Index));
        selectedIndices.Add(_p1Index);

        if (_sessionData.PlayerCount > 1)
        {
            selected.Add(_sessionData.GetCharacterFromLibraryIndex(_p2Index));
            selectedIndices.Add(_p2Index);
        }

        // ✅ ส่ง index ไปด้วย
        _sessionData.SetupMatch(selected, selectedIndices);

        if (SceneEffectController.Instance != null)
        {
            if (TryGetComponent<PlayerInput>(out var input))
                input.DeactivateInput();

            SceneEffectController.Instance.LoadSceneAndPlayEffect("Level 1");
        }
    }
}