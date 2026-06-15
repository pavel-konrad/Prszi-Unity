# Unity Input System Setup

## Instalace Unity Input System

1. **Otevřete Package Manager** (Window > Package Manager)
2. **Klikněte na "+"** v levém horním rohu
3. **Vyberte "Add package by name"**
4. **Zadejte:** `com.unity.inputsystem`
5. **Klikněte na "Add"**

## Konfigurace Input Actions

1. **Vyberte soubor** `GameInputActions.inputactions` v Project window
2. **V Inspector nastavte:**
   - ✅ **Generate C# Class** = true
   - ✅ **C# Class File** = `Assets/Scripts/Core/GameInputActions.cs`
   - ✅ **C# Class Name** = `GameInputActions`
3. **Klikněte na "Apply"**

## Použití v kódu

### Základní použití:
```csharp
public class MyInputHandler : MonoBehaviour, GameInputActions.IPlayerActions
{
    private GameInputActions inputActions;
    
    void Awake()
    {
        inputActions = new GameInputActions();
        inputActions.Player.SetCallbacks(this);
    }
    
    void OnEnable() => inputActions.Enable();
    void OnDisable() => inputActions.Disable();
    void OnDestroy() => inputActions?.Dispose();
    
    public void OnTap(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Vector2 position = inputActions.Player.Position.ReadValue<Vector2>();
            Debug.Log($"Tap at: {position}");
        }
    }
    
    public void OnPress(InputAction.CallbackContext context) { }
    public void OnPosition(InputAction.CallbackContext context) { }
}
```

### Použití s CrossPlatformInput komponentou:
```csharp
// Přidejte CrossPlatformInput komponentu na GameObject
// Automaticky se postará o všechny inputy
```

## Výhody Unity Input System

✅ **Automatická platforma detekce** - mouse/touch  
✅ **Lepší performance** než starý Input Manager  
✅ **Moderní API** s type safety  
✅ **Snadné přidávání** nových inputů  
✅ **Built-in support** pro gamepady, klávesnice  
✅ **Rebinding** v runtime  
✅ **Action-based** design  

## Platformy

- **WebGL**: Mouse input (kliknutí)
- **Mobile**: Touch input (tap)
- **Desktop**: Mouse input
- **Editor**: Oba systémy pro testování

## Troubleshooting

### Input System není nalezen:
- Zkontrolujte, že je Input System nainstalovaný v Package Manager
- Restartujte Unity

### C# třída se negeneruje:
- Zkontrolujte nastavení v Inspector u .inputactions souboru
- Klikněte na "Apply"

### GUID Error (System.FormatException):
- **Problém**: GUID v .inputactions souboru není ve správném formátu
- **Řešení**: GUID musí být ve formátu `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx`
- **Důležité**: GUID musí obsahovat pouze hexadecimální znaky (0-9, a-f)
- **Příklad**: `a1b2c3d4-e5f6-7890-abcd-ef1234567890`
- **Poznámka**: Soubor už má správné GUID, takže by to mělo fungovat

### Input nefunguje na mobilu:
- Zkontrolujte, že máte správně nastavené Touchscreen bindings
- Testujte na skutečném zařízení (ne v editoru)
