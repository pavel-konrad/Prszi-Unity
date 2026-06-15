using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Komponenta pro cross-platform input pomocí Unity Input System.
/// Automaticky podporuje mouse (web/desktop) i touch (mobil).
/// 
/// VÝHODY Unity Input System:
/// - Automatická detekce platformy
/// - Lepší performance
/// - Moderní API
/// - Snadné přidávání nových inputů
/// - Built-in support pro gamepady, klávesnice, atd.
/// </summary>
public class CrossPlatformInput : MonoBehaviour, GameInputActions.IPlayerActions
{
    [Header("Settings")]
    public bool enableHapticFeedback = true; // Vibrace na mobilu
    public bool enableVisualFeedback = true; // Vizuální feedback
    
    [Header("Debug")]
    public bool showDebugLogs = false;
    
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
        
        if (showDebugLogs)
        {
            Debug.Log($"[CrossPlatformInput] Platforma detekována: {(isMobilePlatform ? "Mobile" : "Desktop/Web")}");
            Debug.Log($"[CrossPlatformInput] Unity Input System inicializován");
        }
    }
    
    void OnEnable()
    {
        // Povolit input actions
        inputActions.Enable();
    }
    
    void OnDisable()
    {
        // Zakázat input actions
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
            
            if (showDebugLogs)
            {
                Debug.Log($"[CrossPlatformInput] Tap detected at: {position}");
            }
            
            UIEvents.TriggerScreenTapped(position);
        }
    }
    
    public void OnPress(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            Vector2 position = inputActions.Player.Position.ReadValue<Vector2>();
            lastInputPosition = position;
            
            if (showDebugLogs)
            {
                Debug.Log($"[CrossPlatformInput] Press started at: {position}");
            }
            
            UIEvents.TriggerScreenPressed(position);
            
            // Haptic feedback na mobilu
            if (isMobilePlatform && enableHapticFeedback)
            {
                Handheld.Vibrate();
            }
        }
        else if (context.canceled)
        {
            Vector2 position = inputActions.Player.Position.ReadValue<Vector2>();
            lastInputPosition = position;
            
            if (showDebugLogs)
            {
                Debug.Log($"[CrossPlatformInput] Press ended at: {position}");
            }
            
            UIEvents.TriggerScreenReleased(position);
        }
    }
    
    public void OnPosition(InputAction.CallbackContext context)
    {
        // Position se automaticky aktualizuje při každém input eventu
        if (context.performed)
        {
            lastInputPosition = context.ReadValue<Vector2>();
        }
    }
    
    // Veřejné metody pro programové ovládání
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
    
    // Utility metody
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
    
    // Metody pro kontrolu stavu inputů
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
