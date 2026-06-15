# Průvodce testováním refaktoringu

## Krok 1: Vytvoření ScriptableObject instancí

V Unity Editoru:

1. **Vytvořit AvatarDB:**
   - Right-click v Project window → Create → Prsi → Avatar Database
   - Pojmenovat např. "AvatarDB_Default"
   - Přidat avatary do seznamu (sprite, id, displayName)

2. **Vytvořit EnemyDB:**
   - Right-click v Project window → Create → Prsi → Enemy Database
   - Pojmenovat např. "EnemyDB_Default"
   - Přidat nepřátele do seznamu (sprite, jméno, cash, bet)

3. **Vytvořit CardThemeDB:**
   - Right-click v Project window → Create → Prsi → Card Theme Database
   - Pojmenovat např. "CardThemeDB_Default"
   - Přidat témata karet
   - Pro každé téma nastavit 32 spriteů karet (4 barvy × 8 hodnot)
   - Nastavit card back sprite

## Krok 2: Nastavení Services

1. **Vytvořit GameObject pro Services:**
   - V Hierarchy vytvořit prázdný GameObject "GameServices"
   - Přidat komponenty:
     - `CardThemeService`
     - `CardStateService`
     - `CardDeckService`

2. **Nastavit reference:**
   - V `CardThemeService` přiřadit `CardThemeDB` asset
   - Services se automaticky najdou navzájem

## Krok 3: Spuštění testů

1. **Přidat testovací skript:**
   - Vytvořit GameObject "RefactoringTest"
   - Přidat komponentu `RefactoringTest`
   - Nastavit reference na všechny DB a Services
   - Zaškrtnout "Run Tests On Start" nebo použít Context Menu "Run All Tests"

2. **Zkontrolovat Console:**
   - Měly by se zobrazit výsledky všech testů
   - Pokud jsou nějaké chyby, zkontrolovat reference

## Očekávané výsledky

### AvatarDB Test
- Zobrazí počet avatárů
- Zobrazí seznam všech avatárů s ID a jmény

### EnemyDB Test
- Zobrazí počet nepřátel
- Zobrazí seznam všech nepřátel s detaily
- Otestuje náhodný výběr

### CardThemeDB Test
- Zobrazí počet témat
- Zobrazí výchozí téma
- Otestuje získání sprite pro kartu

### CardThemeService Test
- Nastaví téma
- Otestuje získání sprite podle barvy a hodnoty
- Otestuje získání card back sprite

### CardStateService Test
- Vytvoří testovací kartu
- Otestuje změnu umístění karty
- Otestuje získání karet v umístění

### CardDeckService Test
- Inicializuje balíček (32 karet)
- Lízne 5 karet
- Zobrazí zbývající počet karet

## Řešení problémů

### "DB is null"
- Zkontrolovat, že jsou vytvořené ScriptableObject instance
- Zkontrolovat reference v testovacím skriptu

### "Service is null"
- Zkontrolovat, že jsou Services přidané do scény
- Zkontrolovat reference v testovacím skriptu

### "Sprite is null"
- Zkontrolovat, že jsou sprites přiřazené v CardThemeDB
- Zkontrolovat pořadí spriteů (4 barvy × 8 hodnot = 32 spriteů)

### "Card state not found"
- Zkontrolovat, že je CardStateService ve scéně
- Zkontrolovat, že je karta zaregistrovaná

## Další kroky

Po úspěšném testování:
1. Refaktorovat Card třídu
2. Refaktorovat CardManager
3. Refaktorovat GameSession
4. Integrovat do existujícího kódu

