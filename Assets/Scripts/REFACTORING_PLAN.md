# Plán Refaktoringu - SOLID Pattern

## Cíl
Refaktorovat codebase podle SOLID principů s oddělením logiky od UI a dat z ScriptableObjects.

## Architektura

### 1. ScriptableObject Databáze

#### **AvatarDB** (ScriptableObject)
- Seznam dostupných avatárů pro hráče
- Každý avatar: Sprite, ID, jméno
- Použití: výběr avatara pro hráče

#### **EnemyDB** (ScriptableObject)
- Seznam AI nepřátel
- Každý enemy: Sprite (avatar), jméno, ID, výchozí cash, výchozí bet
- Použití: inicializace AI hráčů

#### **CardThemeDB** (ScriptableObject)
- Témata pro karty (např. Classic, Modern, Fantasy)
- Každé téma obsahuje:
  - Sprites pro všechny karty (32 karet)
  - Card back sprite
  - Suit symbols (volitelné)
  - Rank symbols (volitelné)

### 2. Interfaces (Oddělení logiky od UI)

#### **ICardData**
```csharp
public interface ICardData
{
    Card.Suit Suit { get; }
    Card.Rank Rank { get; }
    Sprite GetSprite(ICardThemeProvider themeProvider);
}
```

#### **ICardThemeProvider**
```csharp
public interface ICardThemeProvider
{
    CardThemeDB CurrentTheme { get; }
    Sprite GetCardSprite(Card.Suit suit, Card.Rank rank);
    Sprite GetCardBackSprite();
}
```

#### **IPlayerData**
```csharp
public interface IPlayerData
{
    int Id { get; }
    string Name { get; }
    bool IsHuman { get; }
    Sprite Avatar { get; }
    int Cash { get; }
    int Bet { get; }
    List<ICardData> Hand { get; }
}
```

#### **IPlayerProvider**
```csharp
public interface IPlayerProvider
{
    IPlayerData GetPlayer(int id);
    IPlayerData GetHumanPlayer();
    List<IPlayerData> GetAllPlayers();
}
```

#### **ICardDeckProvider**
```csharp
public interface ICardDeckProvider
{
    ICardData DrawCard();
    void Shuffle();
    int RemainingCards { get; }
}
```

#### **ICardState** (Pro sledování stavu karty)
```csharp
public enum CardLocation
{
    Deck,           // V balíčku
    PlayerHand,     // V ruce hráče
    EnemyHand,      // V ruce nepřítele
    Discard,        // V odhazovacím balíčku
    Drawn,          // Právě líznutá (přechodný stav)
    Playing         // Právě hraná (přechodný stav)
}

public interface ICardState
{
    ICardData Card { get; }
    CardLocation Location { get; }
    int? OwnerId { get; } // ID hráče, který má kartu (null pokud v decku/discard)
    event System.Action<ICardState, CardLocation> LocationChanged;
}
```

#### **ICardStateProvider**
```csharp
public interface ICardStateProvider
{
    ICardState GetCardState(ICardData card);
    void SetCardLocation(ICardData card, CardLocation location, int? ownerId = null);
    System.Collections.Generic.IEnumerable<ICardState> GetCardsInLocation(CardLocation location);
    System.Collections.Generic.IEnumerable<ICardState> GetPlayerCards(int playerId);
}
```

#### **ICardAnimationController** (Pro ovládání animací)
```csharp
public interface ICardAnimationController
{
    void PlayAnimation(string triggerName);
    void SetBool(string parameterName, bool value);
    void SetFloat(string parameterName, float value);
    void SetInteger(string parameterName, int value);
    bool IsAnimationPlaying(string stateName);
    void OnCardStateChanged(ICardState cardState, CardLocation previousLocation);
}
```

### 3. Struktura tříd

#### **Data vrstva (Pure C# classes)**
- `CardData` - implementuje `ICardData` (pouze data, žádná Unity závislost)
- `PlayerData` - implementuje `IPlayerData` (pouze data)
- `CardDeck` - implementuje `ICardDeckProvider` (logika balíčku)

#### **Service vrstva (MonoBehaviour services)**
- `CardThemeService` - implementuje `ICardThemeProvider`, načítá z CardThemeDB
- `PlayerService` - implementuje `IPlayerProvider`, spravuje hráče
- `CardDeckService` - implementuje `ICardDeckProvider`, spravuje balíček
- `CardStateService` - implementuje `ICardStateProvider`, sleduje stavy karet a vyvolává události

#### **UI vrstva (MonoBehaviour UI)**
- `CardUI` - pouze UI, přijímá `ICardData`, `ICardState`, implementuje `ICardAnimationController`
  - Reaguje na změny stavu karty pomocí animací
  - Animace podle stavu: CardIn (při přidání do ruky), CardOut (při odebrání), CardDrawn (líznutí), CardPlayed (odehrání)
- `PlayerUI` - pouze UI, přijímá `IPlayerData`
- `AvatarPicker` - pouze UI, načítá z AvatarDB

### 4. SOLID Principy

#### **Single Responsibility Principle (SRP)**
- `CardData` - pouze data karty
- `CardUI` - pouze zobrazení karty
- `CardThemeService` - pouze poskytování spriteů
- `PlayerData` - pouze data hráče
- `PlayerUI` - pouze zobrazení hráče

#### **Open/Closed Principle (OCP)**
- Nová témata karet přidáme vytvořením nového ScriptableObject
- Nové avatary přidáme do AvatarDB
- Nové enemy přidáme do EnemyDB

#### **Liskov Substitution Principle (LSP)**
- Všechny implementace interfaces jsou zaměnitelné
- `CardData` může být nahrazeno jinou implementací `ICardData`

#### **Interface Segregation Principle (ISP)**
- Malé, specifické interfaces
- UI komponenty závisí pouze na potřebných interfaces

#### **Dependency Inversion Principle (DIP)**
- UI závisí na abstrakcích (interfaces), ne na konkrétních třídách
- Services poskytují data přes interfaces

## Postup refaktoringu

### Fáze 1: Vytvoření ScriptableObject databází
1. Vytvořit `AvatarDB` ScriptableObject
2. Vytvořit `EnemyDB` ScriptableObject
3. Vytvořit `CardThemeDB` ScriptableObject
4. Vytvořit helper třídy pro data (AvatarData, EnemyData, CardThemeData)

### Fáze 2: Vytvoření interfaces
1. Vytvořit `ICardData`, `ICardThemeProvider`
2. Vytvořit `IPlayerData`, `IPlayerProvider`
3. Vytvořit `ICardDeckProvider`

### Fáze 3: Refaktoring Card systému
1. Vytvořit `CardData` (implementuje `ICardData`)
2. Vytvořit `CardState` (implementuje `ICardState`) - sleduje umístění karty
3. Vytvořit `CardThemeService` (implementuje `ICardThemeProvider`)
4. Vytvořit `CardStateService` (implementuje `ICardStateProvider`) - spravuje stavy všech karet
5. Refaktorovat `Card` třídu - použít `CardData` a `CardThemeService`
6. Refaktorovat `CardManager` - použít `CardStateService` pro změny stavů
7. Refaktorovat `CardUI` - implementovat `ICardAnimationController`, reagovat na změny stavů

### Fáze 4: Refaktoring Player systému
1. Vytvořit `PlayerData` (implementuje `IPlayerData`)
2. Vytvořit `PlayerService` (implementuje `IPlayerProvider`)
3. Refaktorovat `Player` třídu - použít `PlayerData`
4. Refaktorovat `GameSession` - použít `PlayerService` a `EnemyDB`

### Fáze 5: Refaktoring UI
1. Refaktorovat `CardUI` - přijímá `ICardData` a `ICardThemeProvider`
2. Refaktorovat `PlayerUI` - přijímá `IPlayerData`
3. Refaktorovat `AvatarPicker` - načítá z `AvatarDB`

### Fáze 6: Integrace a testování
1. Propojit všechny komponenty
2. Otestovat funkčnost
3. Odstranit starý kód

## Struktura složek

```
Scripts/
├── Data/
│   ├── ScriptableObjects/
│   │   ├── AvatarDB.cs
│   │   ├── EnemyDB.cs
│   │   └── CardThemeDB.cs
│   └── Models/
│       ├── AvatarData.cs
│       ├── EnemyData.cs
│       └── CardThemeData.cs
├── Core/
│   ├── Interfaces/
│   │   ├── ICardData.cs
│   │   ├── ICardThemeProvider.cs
│   │   ├── ICardState.cs
│   │   ├── ICardStateProvider.cs
│   │   ├── ICardAnimationController.cs
│   │   ├── IPlayerData.cs
│   │   ├── IPlayerProvider.cs
│   │   └── ICardDeckProvider.cs
│   ├── Services/
│   │   ├── CardThemeService.cs
│   │   ├── CardStateService.cs
│   │   ├── PlayerService.cs
│   │   └── CardDeckService.cs
│   └── Data/
│       ├── CardData.cs
│       ├── CardState.cs
│       └── PlayerData.cs
├── Game/
│   ├── Card.cs (refaktorováno)
│   ├── Player.cs (refaktorováno)
│   └── CardManager.cs (refaktorováno)
└── UI/
    ├── CardUI.cs (refaktorováno)
    ├── PlayerUI.cs (refaktorováno)
    └── AvatarPicker.cs (refaktorováno)
```

## Systém stavů karet a animací

### Stavy karty (CardLocation)
- **Deck** - karta je v balíčku (zadní strana)
- **PlayerHand** - karta je v ruce hráče (viditelná)
- **EnemyHand** - karta je v ruce nepřítele (zadní strana)
- **Discard** - karta je v odhazovacím balíčku (viditelná)
- **Drawn** - karta je právě líznutá (přechodný stav pro animaci)
- **Playing** - karta je právě hraná (přechodný stav pro animaci)

### Animace podle stavu

#### **CardUI reaguje na změny stavu:**
```csharp
// Při změně stavu karty
void OnCardStateChanged(ICardState cardState, CardLocation previousLocation)
{
    switch (cardState.Location)
    {
        case CardLocation.PlayerHand:
            if (previousLocation == CardLocation.Deck)
                PlayAnimation("CardIn"); // Animace přidání do ruky
            break;
            
        case CardLocation.Discard:
            if (previousLocation == CardLocation.PlayerHand)
                PlayAnimation("CardPlayed"); // Animace odehrání
            break;
            
        case CardLocation.Drawn:
            PlayAnimation("CardDrawn"); // Animace líznutí
            break;
            
        case CardLocation.EnemyHand:
            // Karta je otočená zadní stranou
            SetCardBackVisible(true);
            break;
    }
}
```

### Event systém pro animace

```csharp
// CardStateService vyvolá událost při změně stavu
public event System.Action<ICardState, CardLocation> OnCardLocationChanged;

// CardUI se přihlásí k události
cardStateService.OnCardLocationChanged += OnCardStateChanged;
```

### Kontrola animací

- **ICardAnimationController** umožňuje:
  - Spouštět animace podle triggerů
  - Nastavovat parametry animátoru (bool, float, int)
  - Kontrolovat, zda animace právě běží
  - Reagovat na změny stavu karty

- **CardUI implementuje ICardAnimationController:**
  - Automaticky reaguje na změny stavu
  - Spouští příslušné animace
  - Umožňuje manuální ovládání animací

## Poznámky

- **Zachování kompatibility**: Postupně refaktorovat, zachovat funkčnost
- **Testování**: Po každé fázi otestovat
- **Migrace dat**: Vytvořit migrační skripty pro existující data
- **Performance**: ScriptableObjects jsou efektivní pro čtení dat
- **Animace**: Všechny animace jsou řízeny přes CardStateService a ICardAnimationController
- **Stavy karet**: Každá karta má vždy známý stav, který lze sledovat a na který UI reaguje

