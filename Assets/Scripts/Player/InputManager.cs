using UnityEngine;
using UnityEngine.InputSystem;

// We need this to execute late so its FixedUpdate can clear the recorded inputs
// _after_ other scripts have had the chance to use them.
[DefaultExecutionOrder(1337)]
public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    public Vector2 Move { get; private set; }

    public bool JumpPressedThisFrame { get; private set; }
    public bool JumpHeld { get; private set; }
    public float TimeJumpWasPressed { get; private set; } = Mathf.NegativeInfinity;

    public bool DashPressedThisFrame { get; private set; }
    public bool DashHeld { get; private set; }
    public float TimeDashWasPressed { get; private set; } = Mathf.NegativeInfinity;

    public bool SlashPressedThisFrame { get; private set; }
    public float TimeSlashWasPressed { get; private set; } = Mathf.NegativeInfinity;

    public bool PausePressedThisFrame { get; private set; }

    public bool InvincibilityCheatPressedThisFrame { get; private set; }

    private PlayerInput _playerInput;

    private InputAction _moveAction;
    private InputAction _jumpAction;
    private InputAction _dashAction;
    private InputAction _slashAction;

    private InputAction _pauseAction;
    private InputAction _cheatAction;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);

        _playerInput = GetComponent<PlayerInput>();

        _moveAction = _playerInput.actions["Move"];
        _jumpAction = _playerInput.actions["Jump"];
        _dashAction = _playerInput.actions["Dash"];
        _slashAction = _playerInput.actions["Slash"];

        _pauseAction = _playerInput.actions["Pause"];

        _cheatAction = _playerInput.actions["InvincibilityCheat"];
    }

    private void OnEnable()
    {
        _moveAction?.Enable();
        _jumpAction?.Enable();
        _dashAction?.Enable();
        _slashAction?.Enable();
    }

    private void OnDisable()
    {
        _moveAction?.Disable();
        _jumpAction?.Disable();
        _dashAction?.Disable();
        _slashAction?.Disable();
    }

    private void Update()
    {
        Move = _moveAction.ReadValue<Vector2>();

        if (_jumpAction.WasPerformedThisFrame())
        {
            JumpPressedThisFrame = true;
            TimeJumpWasPressed = Time.time;
        }
        JumpHeld = _jumpAction.IsPressed();


        if (_dashAction.WasPerformedThisFrame())
        {
            DashPressedThisFrame = true;
            TimeDashWasPressed = Time.time;
        }
        DashHeld = _dashAction.IsPressed();

        if (_slashAction.WasPerformedThisFrame())
        {
            SlashPressedThisFrame = true;
            TimeSlashWasPressed = Time.time;
        }

        if (_pauseAction.WasPerformedThisFrame())
            PausePressedThisFrame = true;
        if (_cheatAction.WasPerformedThisFrame())
            InvincibilityCheatPressedThisFrame = true;
    }

    private void FixedUpdate()
    {
        JumpPressedThisFrame = false;
        DashPressedThisFrame = false;
        SlashPressedThisFrame = false;

        PausePressedThisFrame = false;
        InvincibilityCheatPressedThisFrame = false;
    }

    public void ClearJump()
    {
        JumpPressedThisFrame = false;
        TimeJumpWasPressed = Mathf.NegativeInfinity;
    }

    public void ClearDash()
    {
        DashPressedThisFrame = false;
        TimeDashWasPressed = Mathf.NegativeInfinity;
    }

    public void ClearSlash()
    {
        SlashPressedThisFrame = false;
        TimeSlashWasPressed = Mathf.NegativeInfinity;
    }

    public void PressJump(float time)
    {
        JumpPressedThisFrame = true;
        TimeJumpWasPressed = time >= 0 ? time : Time.time;
    }

    public void PressDash(float time)
    {
        DashPressedThisFrame = true;
        TimeDashWasPressed = time >= 0 ? time : Time.time;
    }

    public void PressSlash(float time)
    {
        SlashPressedThisFrame = true;
        TimeSlashWasPressed = time >= 0 ? time : Time.time;
    }
}
