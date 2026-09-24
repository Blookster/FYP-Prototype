using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    [Header("Input Action References (Meta Quest 3)")]
    [Tooltip("Assigned to controller grips for raising a block.")]
    public InputActionReference blockAction;

    [Tooltip("Assigned to thumbstick clicks or primary buttons for dodging.")]
    public InputActionReference dodgeAction;

    [Header("Editor Debug / PC Testing")]
    [Tooltip("Allows testing controls on a flat screen using Spacebar and Left Shift.")]
    public bool enableKeyboardDebug = true;

    // Public properties read by PlayerController
    public bool IsBlocking { get; private set; }
    public bool IsDodge { get; private set; }

    void OnEnable()
    {
        // Safely enable actions when the object wakes up
        if (blockAction != null && blockAction.action != null) blockAction.action.Enable();
        if (dodgeAction != null && dodgeAction.action != null) dodgeAction.action.Enable();
    }

    void OnDisable()
    {
        // Cleanly disable actions when the object turns off
        if (blockAction != null && blockAction.action != null) blockAction.action.Disable();
        if (dodgeAction != null && dodgeAction.action != null) dodgeAction.action.Disable();
    }

    void Update()
    {
        // 1. Read actual VR inputs from the Meta Quest controllers
        ReadVRInputs();

        // 2. Read fallback keyboard inputs if enabled
        if (enableKeyboardDebug)
        {
            ReadDebugKeyboardInputs();
        }
    }

    void ReadVRInputs()
    {
        // Check block action (e.g., holding the grip button)
        if (blockAction != null && blockAction.action != null)
        {
            IsBlocking = blockAction.action.IsPressed();
        }

        // Check dodge action (e.g., clicking the thumbstick)
        if (dodgeAction != null && dodgeAction.action != null)
        {
            IsDodge = dodgeAction.action.IsPressed();
        }
    }

    void ReadDebugKeyboardInputs()
    {
        // Spacebar acts as Block for quick PC testing
        if (Input.GetKey(KeyCode.Space))
        {
            IsBlocking = true;
        }

        // Left Shift acts as Dodge for quick PC testing
        if (Input.GetKey(KeyCode.LeftShift))
        {
            IsDodge = true;
        }
    }
}