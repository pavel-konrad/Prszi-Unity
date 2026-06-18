using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Component for cross-platform input using Unity Input System.
/// Automatically supports mouse (web/desktop) and touch (mobile).
/// 
/// BENEFITS of Unity Input System:
/// - Automatic platform detection
/// - Better performance
/// - Modern API
/// - Easy to add new inputs
/// - Built-in support for gamepads, keyboard, etc.
/// </summary>
public class CrossPlatformInput : MonoBehaviour, GameInputActions.IPlayerActions
{
    [Header("Settings")]
    public bool enableHapticFeedback = true; // Vibration on mobile
    public bool enableVisualFeedback = true; // Visual feedback

    private GameInputActions inputActions;
    private bool isMobilePlatform;
    private Vector2 lastInputPosition;
    
    void Awake()
    {
        // Detekovat platformu
        isMobilePlatform = Application.isMobilePlatform;
        
        // Inicializovat Input Actions
        inputActions = new GameInputActions();
        inputActions.Player.SetCallbacks(this);
        
    }
    
    void OnEnable()
    {
        // Povolit input actions
        inputActions.Enable();
    }
    
    void OnDisable()
    {
        // Disable input actions
        inputActions.Disable();
    }
    
    void OnDestroy()
    {
        // Dispose input actions
        inputActions?.Dispose();
    }
    
    // Unity Input System callbacks
    public void OnTap(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Vector2 position = inputActions.Player.Position.ReadValue<Vector2>();
            lastInputPosition = position;
            
            
            UIEvents.TriggerScreenTapped(position);
        }
    }
    
    public void OnPress(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            Vector2 position = inputActions.Player.Position.ReadValue<Vector2>();
            lastInputPosition = position;
            
            
            UIEvents.TriggerScreenPressed(position);
            
            // Haptic feedback on mobile
            if (isMobilePlatform && enableHapticFeedback)
            {
                Handheld.Vibrate();
            }
        }
        else if (context.canceled)
        {
            Vector2 position = inputActions.Player.Position.ReadValue<Vector2>();
            lastInputPosition = position;
            
            
            UIEvents.TriggerScreenReleased(position);
        }
    }
    
    public void OnPosition(InputAction.CallbackContext context)
    {
        // Position is updated automatically on every input event
        if (context.performed)
        {
            lastInputPosition = context.ReadValue<Vector2>();
        }
    }
    
    // Public methods for programmatic control
    public void SimulateTap(Vector2 position)
    {
        UIEvents.TriggerScreenTapped(position);
    }
    
    public void SimulatePress(Vector2 position)
    {
        UIEvents.TriggerScreenPressed(position);
    }
    
    public void SimulateRelease(Vector2 position)
    {
        UIEvents.TriggerScreenReleased(position);
    }
    
    // Utility methods
    public bool IsMobilePlatform()
    {
        return isMobilePlatform;
    }
    
    public Vector2 GetLastInputPosition()
    {
        return lastInputPosition;
    }
    
    public GameInputActions GetInputActions()
    {
        return inputActions;
    }
    
    // Methods for checking input state
    public bool IsPressed()
    {
        return inputActions.Player.Press.IsPressed();
    }
    
    public bool IsTapPressed()
    {
        return inputActions.Player.Tap.IsPressed();
    }
    
    public Vector2 GetCurrentPosition()
    {
        return inputActions.Player.Position.ReadValue<Vector2>();
    }
}
