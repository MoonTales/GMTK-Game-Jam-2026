using System.Collections;
using System.Collections.Generic;
using Managers;
using Rat_P;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

[System.Serializable]
public enum ButtonActivityState
{
    NonSelected,
    NonActivated,
    Activated,
    Inactive
}

[System.Serializable]
public struct WiresVisible
{
    public bool TopWireVisible;
    public bool BottomWireVisible;
    public bool LeftWireVisible;
    public bool RightWireVisible;
}

[System.Serializable]
public enum ButtonOption
{
    W,
    A,
    S,
    D,
    Q,
    E,
    R,
    T,
    Y,
    F,
    G,
    H,
    NONE
}

[System.Serializable]
public struct ButtonIconData
{
    [SerializeField] private ButtonOption buttonOption;
    [SerializeField] private Sprite buttonIconSprite;
    [SerializeField] private Sprite buttonSpriteNone;
    [SerializeField] private Sprite buttonSpriteNonActivated;
    [SerializeField] private Sprite buttonSpriteActivated;

    public ButtonIconData(ButtonOption option, Sprite icon = null, Sprite spriteNone = null, Sprite spriteNonActivated = null, Sprite spriteActivated = null)
    {
        buttonOption = option;
        buttonIconSprite = icon;
        buttonSpriteNone = spriteNone;
        buttonSpriteNonActivated = spriteNonActivated;
        buttonSpriteActivated = spriteActivated;
    }

    public ButtonOption GetButtonOption() => buttonOption;
    public Sprite GetButtonIconSprite() => buttonIconSprite;
    public Sprite GetButtonSpriteNone() => buttonSpriteNone;
    public Sprite GetButtonSpriteNonActivated() => buttonSpriteNonActivated;
    public Sprite GetButtonSpriteActivated() => buttonSpriteActivated;
}

public class ButtonModifier : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject _button;
    [SerializeField] private GameObject _buttonIcon;
    [SerializeField] private GameObject _topWire;
    [SerializeField] private GameObject _bottomWire;
    [SerializeField] private GameObject _leftWire;
    [SerializeField] private GameObject _rightWire;

    public int ButtonIndex = -1;
    public bool IsButtonApartOfThePath = false;
    public ButtonActivityState CurrentButtonState = ButtonActivityState.NonSelected;
    public ButtonIconData CurrentButtonIconData;

    private Button _uiButton;
    private Image _buttonImage;
    private Image _iconImage;

    private InputAction _wKeyAction;
    private InputAction _sKeyAction;
    private InputAction _aKeyAction;
    private InputAction _dKeyAction;
    private InputAction _qKeyAction;
    private InputAction _eKeyAction;
    private InputAction _rKeyAction;
    private InputAction _tKeyAction;
    private InputAction _yKeyAction;
    private InputAction _fKeyAction;
    private InputAction _gKeyAction;
    private InputAction _hKeyAction;
    private InputAction _noneKeyAction;

    public void SetButtonIndex(int index) => ButtonIndex = index;
    public void SetButtonIconData(ButtonIconData data) => CurrentButtonIconData = data;

    private void Awake()
    {
        // Cache UI components to prevent repeated GetComponent calls
        if (_button != null)
        {
            _uiButton = _button.GetComponent<Button>();
            _buttonImage = _button.GetComponent<Image>();
        }

        if (_buttonIcon != null)
        {
            _iconImage = _buttonIcon.GetComponent<Image>();
        }

        // Hide wires by default
        SetWiresVisible(new WiresVisible
        {
            TopWireVisible = false,
            BottomWireVisible = false,
            LeftWireVisible = false,
            RightWireVisible = false
        });

        // Initialize Input Actions
        _wKeyAction = new InputAction("W_Key", binding: "<Keyboard>/w");
        _sKeyAction = new InputAction("S_Key", binding: "<Keyboard>/s");
        _aKeyAction = new InputAction("A_Key", binding: "<Keyboard>/a");
        _dKeyAction = new InputAction("D_Key", binding: "<Keyboard>/d");
        _qKeyAction = new InputAction("Q_Key", binding: "<Keyboard>/q");
        _eKeyAction = new InputAction("E_Key", binding: "<Keyboard>/e");
        _rKeyAction = new InputAction("R_Key", binding: "<Keyboard>/r");
        _tKeyAction = new InputAction("T_Key", binding: "<Keyboard>/t");
        _yKeyAction = new InputAction("Y_Key", binding: "<Keyboard>/y");
        _fKeyAction = new InputAction("F_Key", binding: "<Keyboard>/f");
        _gKeyAction = new InputAction("G_Key", binding: "<Keyboard>/g");
        _hKeyAction = new InputAction("H_Key", binding: "<Keyboard>/h");

        // Bind responses
        _wKeyAction.performed += ctx => OnInputTriggered(ButtonOption.W);
        _sKeyAction.performed += ctx => OnInputTriggered(ButtonOption.S);
        _aKeyAction.performed += ctx => OnInputTriggered(ButtonOption.A);
        _dKeyAction.performed += ctx => OnInputTriggered(ButtonOption.D);
        _qKeyAction.performed += ctx => OnInputTriggered(ButtonOption.Q);
        _eKeyAction.performed += ctx => OnInputTriggered(ButtonOption.E);
        _rKeyAction.performed += ctx => OnInputTriggered(ButtonOption.R);
        _tKeyAction.performed += ctx => OnInputTriggered(ButtonOption.T);
        _yKeyAction.performed += ctx => OnInputTriggered(ButtonOption.Y);
        _fKeyAction.performed += ctx => OnInputTriggered(ButtonOption.F);
        _gKeyAction.performed += ctx => OnInputTriggered(ButtonOption.G);
        _hKeyAction.performed += ctx => OnInputTriggered(ButtonOption.H);
    }

    private void Start()
    {
        if (CurrentButtonState == ButtonActivityState.NonSelected && _buttonImage != null)
        {
            _buttonImage.color = Color.grey;
        }

        if (_uiButton != null)
        {
            _uiButton.onClick.AddListener(OnButtonClicked);
        }

        // Ensure visuals and keybindings are properly applied on start
        UpdateButtonVisuals();
    }

    private void OnInputTriggered(ButtonOption pressedOption)
    {
// Check if this button is currently targeted by the system
        if (RatPSystem.Instance != null && RatPSystem.Instance.GetCurrentButtonInd() == ButtonIndex)
        {
            if (RatPSystem.Instance.IsBacktracking()) return;

            ButtonOption requiredOption = CurrentButtonIconData.GetButtonOption();

            // 1. Correct key pressed!
            if (pressedOption == requiredOption)
            {
                _uiButton?.onClick.Invoke();
            }
            // 2. Wrong key pressed! (e.g. required 'W', but pressed 'S')
            else
            {
                OnWrongKeyPressed(pressedOption, requiredOption);
            }
        }
    }
    
    private void OnWrongKeyPressed(ButtonOption pressed, ButtonOption required)
    {
        print($"Wrong key! Pressed {pressed}, but expected {required} for Button {ButtonIndex}");
        RatPSystem.Instance.GridBacktrack();
    }

    private void OnButtonClicked()
    {
        if (RatPSystem.Instance != null && RatPSystem.Instance.GetCurrentButtonInd() == ButtonIndex)
        {
            if (!RatPSystem.Instance.IsBacktracking())
            {
                UAudio.Instance.PlayRATP_ButtonPressSound();
                SetButtonState(ButtonActivityState.Activated);
                StartCoroutine(DelayedIncrementButtonIndex());
            }
            else
            {
                // weird edge case, if this ever calls we are backtracking and the button is clicked, we should not increment the index
                SetButtonState(ButtonActivityState.NonActivated);
            }

            

        }
        else
        {
            print($"Button {ButtonIndex} clicked, but it's not the current target. Current target is {RatPSystem.Instance.GetCurrentButtonInd()}");
        }
    }
    

    private IEnumerator DelayedIncrementButtonIndex()
    {
        // Wait 4 frames as originally specified
        yield return null;
        yield return null;
        yield return null;
        yield return null;

        if (RatPSystem.Instance != null)
        {
            RatPSystem.Instance.SetCurrentButtonInd(ButtonIndex + 1);
        }
    }

    public void SetButtonState(ButtonActivityState newState)
    {
        CurrentButtonState = newState;
        if (_buttonImage == null) return;

        switch (CurrentButtonState)
        {
            case ButtonActivityState.NonSelected:
                _buttonImage.sprite = CurrentButtonIconData.GetButtonSpriteNone();
                break;
            case ButtonActivityState.Activated:
                _buttonImage.sprite = CurrentButtonIconData.GetButtonSpriteActivated();
                if (_topWire.activeSelf) _topWire.GetComponent<SpriteAnimator>().SetDisabled(false);
                if (_bottomWire.activeSelf) _bottomWire.GetComponent<SpriteAnimator>().SetDisabled(false);
                if (_leftWire.activeSelf) _leftWire.GetComponent<SpriteAnimator>().SetDisabled(false);
                if (_rightWire.activeSelf) _rightWire.GetComponent<SpriteAnimator>().SetDisabled(false);
                break;
            case ButtonActivityState.NonActivated:
                _buttonImage.sprite = CurrentButtonIconData.GetButtonSpriteNonActivated();
                if (_topWire.activeSelf) _topWire.GetComponent<SpriteAnimator>().SetDisabled(true);
                if (_bottomWire.activeSelf) _bottomWire.GetComponent<SpriteAnimator>().SetDisabled(true);
                if (_leftWire.activeSelf) _leftWire.GetComponent<SpriteAnimator>().SetDisabled(true);
                if (_rightWire.activeSelf) _rightWire.GetComponent<SpriteAnimator>().SetDisabled(true);
                
                break;
            case ButtonActivityState.Inactive:
                _buttonImage.sprite = CurrentButtonIconData.GetButtonSpriteNone();
                break;
        }
    }

    public void SetWiresVisible(WiresVisible wiresVisible)
    {
        if (_topWire != null) _topWire.SetActive(wiresVisible.TopWireVisible);
        if (_bottomWire != null) _bottomWire.SetActive(wiresVisible.BottomWireVisible);
        if (_leftWire != null) _leftWire.SetActive(wiresVisible.LeftWireVisible);
        if (_rightWire != null) _rightWire.SetActive(wiresVisible.RightWireVisible);
        SetButtonState(CurrentButtonState); // Update button state to reflect wire visibility changes
    }

    public void UpdateButtonVisuals()
    {
        // First disable all actions to clean up state
        DisableAllActions();

        if (_iconImage != null)
        {
            
            _iconImage.sprite = CurrentButtonIconData.GetButtonIconSprite();
        }
        // Always enable input actions so we can catch both correct AND wrong keys
        EnableAllActions();
    }
    private void EnableAllActions()
    {
        _wKeyAction?.Enable();
        _sKeyAction?.Enable();
        _aKeyAction?.Enable();
        _dKeyAction?.Enable();
        _qKeyAction?.Enable();
        _eKeyAction?.Enable();
        _rKeyAction?.Enable();
        _tKeyAction?.Enable();
        _yKeyAction?.Enable();
        _fKeyAction?.Enable();
        _gKeyAction?.Enable();
        _hKeyAction?.Enable();
    }

    private void DisableAllActions()
    {
        _wKeyAction?.Disable();
        _aKeyAction?.Disable();
        _sKeyAction?.Disable();
        _dKeyAction?.Disable();
        _qKeyAction?.Disable();
        _eKeyAction?.Disable();
        _rKeyAction?.Disable();
        _tKeyAction?.Disable();
        _yKeyAction?.Disable();
        _fKeyAction?.Disable();
        _gKeyAction?.Disable();
        _hKeyAction?.Disable();
    }

    private void OnDisable()
    {
        DisableAllActions();
    }

    private void OnDestroy()
    {
        DisableAllActions();
        _wKeyAction?.Dispose();
        _aKeyAction?.Dispose();
        _aKeyAction?.Dispose();
        _dKeyAction?.Dispose();
        _qKeyAction?.Dispose();
        _eKeyAction?.Dispose();
        _rKeyAction?.Dispose();
        _tKeyAction?.Dispose();
        _yKeyAction?.Dispose();
        _fKeyAction?.Dispose();
        _gKeyAction?.Dispose();
        _hKeyAction?.Dispose();
    }
}
