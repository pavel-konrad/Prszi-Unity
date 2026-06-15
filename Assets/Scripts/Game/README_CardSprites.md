# Card Sprites Setup Guide

## Systematické řešení problému s načítáním assetů

### Krok 1: Kontrola současného stavu

1. **Spusťte hru** a sledujte Console
2. **Vyberte CardSpriteManager** v Inspector
3. **Klikněte pravým tlačítkem** na CardSpriteManager komponentu
4. **Vyberte "Debug Settings"** - zkontrolujte stav

### Krok 2: Kontrola Resources složky

1. **Vyberte CardSpriteManager** v Inspector
2. **Klikněte pravým tlačítkem** na CardSpriteManager komponentu
3. **Vyberte "Check Resources Folder"** - zjistí co je v Resources

### Krok 3: Možnosti řešení

#### Varianta A: Sprites v Inspector (Doporučeno)
1. **Vytvořte CardSpriteManager GameObject** ve scéně
2. **Přiřaďte sprites** do polí v Inspector:
   - Heart Sprites (pole 8 spriteů)
   - Diamond Sprites (pole 8 spriteů)
   - Club Sprites (pole 8 spriteů)
   - Spade Sprites (pole 8 spriteů)

#### Varianta B: Sprites v Resources složce
1. **Vytvořte složku** `Assets/Resources/Cards/`
2. **Vytvořte podsložky** pro každou barvu:
   - `Assets/Resources/Cards/Hearts/`
   - `Assets/Resources/Cards/Diamonds/`
   - `Assets/Resources/Cards/Clubs/`
   - `Assets/Resources/Cards/Spades/`
3. **Umístěte sprites** s názvy:
   - `Seven.png`, `Eight.png`, `Nine.png`, `Ten.png`
   - `Jack.png`, `Queen.png`, `King.png`, `Ace.png`

#### Varianta C: Flexibilní názvy
CardSpriteManager automaticky hledá sprites s názvy:
- `hearts_7`, `hearts_8`, `hearts_9`, `hearts_10`, `hearts_j`, `hearts_q`, `hearts_k`, `hearts_a`
- `diamonds_7`, `diamonds_8`, `diamonds_9`, `diamonds_10`, `diamonds_j`, `diamonds_q`, `diamonds_k`, `diamonds_a`
- `clubs_7`, `clubs_8`, `clubs_9`, `clubs_10`, `clubs_j`, `clubs_q`, `clubs_k`, `clubs_a`
- `spades_7`, `spades_8`, `spades_9`, `spades_10`, `spades_j`, `spades_q`, `spades_k`, `spades_a`

**Doporučený formát:** `{suit}_{rank}` (např. `clubs_7`, `diamonds_q`, `hearts_a`)

### Krok 4: Testování

1. **Spusťte hru** a sledujte Console
2. **Zkontrolujte** zda se načítají sprites
3. **Pokud ne**, použijte fallback sprites (bílé karty s textem)

### Krok 5: Debug příkazy

V Unity Editor (pravé tlačítko na CardSpriteManager):
- **"Debug Settings"** - zobrazí stav nastavení
- **"Check Resources Folder"** - zkontroluje Resources složku
- **"Load Sprites from Resources"** - načte sprites z Resources
- **"Load Sprites Flexibly"** - flexibilní načítání
- **"Test Name Format"** - zobrazí očekávané názvy pro váš formát
- **"Show All Resources Sprites"** - zobrazí všechny sprites v Resources

### Časté problémy

#### Problém: Žádné sprites se nenačítají
**Řešení:**
1. Zkontrolujte "Debug Settings"
2. Zkontrolujte "Check Resources Folder"
3. Ujistěte se, že máte sprites v Inspector nebo Resources

#### Problém: Sprites se nenačítají z Resources
**Řešení:**
1. Ujistěte se, že sprites jsou v `Assets/Resources/` složce
2. Zkontrolujte názvy souborů
3. Použijte "Load Sprites Flexibly"

#### Problém: Fallback sprites se zobrazují
**Řešení:**
- To je normální, pokud nemáte skutečné sprites
- Fallback sprites jsou bílé karty s textem
- Pro lepší vzhled přidejte skutečné sprites

### Automatické řešení

CardSpriteManager automaticky:
1. ✅ Načte sprites z Inspector
2. ✅ Zkusí flexibilní načítání z Resources
3. ✅ Vytvoří fallback sprites pokud nic nenajde
4. ✅ Zobrazí debug informace v Console
