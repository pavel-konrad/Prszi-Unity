# Průvodce Integrací: Polymorfní Karty → UI

Tento dokument popisuje, jak propojit novou polymorfní architekturu karet s UI vrstvou.

## Architektura

```
┌─────────────────────────────────────────────────────────────────┐
│                        GAME LOGIC                                │
│  ┌──────────────┐   ┌──────────────┐   ┌──────────────┐        │
│  │  BaseCard    │   │ GameContext  │   │    Deck      │        │
│  │  SevenCard   │   │  (events)    │   │ DiscardPile  │        │
│  │  AceCard     │   └──────┬───────┘   └──────────────┘        │
│  │  QueenCard   │          │                                    │
│  └──────────────┘          │                                    │
└────────────────────────────┼────────────────────────────────────┘
                             │ Events
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│                        SERVICES                                  │
│  ┌──────────────┐   ┌──────────────┐   ┌──────────────┐        │
│  │CardStateServ.│   │CardThemeServ.│   │CardDeckServ. │        │
│  │ (location)   │   │  (sprites)   │   │  (factory)   │        │
│  └──────┬───────┘   └──────────────┘   └──────────────┘        │
└─────────┼───────────────────────────────────────────────────────┘
          │ LocationChanged event
          ▼
┌─────────────────────────────────────────────────────────────────┐
│                          UI LAYER                                │
│  ┌──────────────┐   ┌──────────────┐   ┌──────────────┐        │
│  │   CardUI     │   │  PlayerUI    │   │AnimEventRecv.│        │
│  │ (vizualizace)│   │              │   │  (zvuky)     │        │
│  └──────────────┘   └──────────────┘   └──────────────┘        │
└─────────────────────────────────────────────────────────────────┘
```

## 1. GameContext Events → UI

`GameContext` vyvolává eventy, na které se UI může přihlásit:

```csharp
// V UI komponenty (např. GameUI.cs)
public class GameUI : MonoBehaviour
{
    private GameContext gameContext;
    
    void Start()
    {
        // Přihlášení k eventům
        gameContext.OnCardPlayed += HandleCardPlayed;
        gameContext.OnSuitChanged += HandleSuitChanged;
        gameContext.OnDrawPenalty += HandleDrawPenalty;
        gameContext.OnPlayerSkipped += HandlePlayerSkipped;
        gameContext.OnPlayerWon += HandlePlayerWon;
    }
    
    void OnDestroy()
    {
        // Odhlášení (důležité!)
        if (gameContext != null)
        {
            gameContext.OnCardPlayed -= HandleCardPlayed;
            gameContext.OnSuitChanged -= HandleSuitChanged;
            // ... atd
        }
    }
    
    // === Event Handlers ===
    
    void HandleCardPlayed(ICardData card, IPlayerData player)
    {
        Debug.Log($"{player.Name} zahrál {card}");
        // Spustit animaci, zvuk, atd.
    }
    
    void HandleSuitChanged(Card.Suit newSuit)
    {
        // Zobrazit UI pro vynucenou barvu (po dámě)
        ShowForcedSuitIndicator(newSuit);
    }
    
    void HandleDrawPenalty(int totalCards)
    {
        // Zobrazit varování "Musíš líznout X karet!"
        ShowDrawPenaltyWarning(totalCards);
    }
    
    void HandlePlayerSkipped()
    {
        // Animace přeskočení (efekt esa)
        PlaySkipAnimation();
    }
    
    void HandlePlayerWon(IPlayerData winner)
    {
        // Zobrazit výherní obrazovku
        ShowWinScreen(winner);
    }
}
```

## 2. CardStateService → CardUI

Karty mění svou pozici (Deck → Hand → Discard). `CardStateService` to sleduje a notifikuje UI:

```csharp
// CardUI.cs - reaguje na změny stavu karty
public class CardUI : MonoBehaviour
{
    [SerializeField] private Image cardImage;
    [SerializeField] private Animator animator;
    
    private BaseCard card;
    private ICardState cardState;
    private CardStateService stateService;
    private CardThemeService themeService;
    
    public void Initialize(BaseCard card, CardStateService stateService, CardThemeService themeService)
    {
        this.card = card;
        this.stateService = stateService;
        this.themeService = themeService;
        
        // Získat stav karty
        cardState = stateService.GetCardState(card);
        
        // Přihlásit se k změnám
        if (cardState != null)
        {
            cardState.LocationChanged += OnLocationChanged;
        }
        
        // Nastavit sprite
        UpdateSprite();
    }
    
    void OnDestroy()
    {
        if (cardState != null)
        {
            cardState.LocationChanged -= OnLocationChanged;
        }
    }
    
    void UpdateSprite()
    {
        var sprite = themeService.GetCardSprite(card.Suit, card.Rank);
        cardImage.sprite = sprite;
    }
    
    /// <summary>
    /// Reaguje na změnu pozice karty
    /// </summary>
    void OnLocationChanged(ICardState state, CardLocation previousLocation)
    {
        switch (state.Location)
        {
            case CardLocation.PlayerHand:
                if (previousLocation == CardLocation.Deck)
                {
                    // Karta byla líznutá → animace CardIn
                    animator.SetTrigger("CardIn");
                    AudioEvents.TriggerCardDealt();
                }
                break;
                
            case CardLocation.Discard:
                if (previousLocation == CardLocation.PlayerHand)
                {
                    // Karta byla odehrána → animace CardPlayed
                    animator.SetTrigger("CardPlayed");
                    AudioEvents.TriggerCardPlayed();
                }
                break;
                
            case CardLocation.EnemyHand:
                // Zobrazit rubovou stranu
                ShowCardBack();
                break;
        }
    }
    
    void ShowCardBack()
    {
        cardImage.sprite = themeService.GetCardBackSprite();
    }
}
```

## 3. Polymorfní Karty → Speciální UI Efekty

Různé typy karet mohou mít různé vizuální efekty:

```csharp
// V CardUI nebo GameUI
void HandleCardPlayed(ICardData cardData, IPlayerData player)
{
    // Polymorfní kontrola typu karty
    if (cardData is SevenCard)
    {
        // Speciální efekt pro sedmu
        ShowDrawPenaltyEffect();
        PlaySevenSound();
    }
    else if (cardData is AceCard)
    {
        // Efekt pro eso
        ShowSkipEffect();
        PlayAceSound();
    }
    else if (cardData is QueenCard queen)
    {
        // Efekt pro dámu - zobrazit výběr barvy
        ShowSuitSelectionDialog(queen);
    }
}
```

## 4. Dáma: UI Dialog pro Výběr Barvy

Když hráč zahraje dámu, musí vybrat barvu:

```csharp
// SuitSelectionDialog.cs
public class SuitSelectionDialog : MonoBehaviour
{
    public event System.Action<Card.Suit> OnSuitSelected;
    
    [SerializeField] private Button heartsButton;
    [SerializeField] private Button diamondsButton;
    [SerializeField] private Button clubsButton;
    [SerializeField] private Button spadesButton;
    
    void Start()
    {
        heartsButton.onClick.AddListener(() => SelectSuit(Card.Suit.Hearts));
        diamondsButton.onClick.AddListener(() => SelectSuit(Card.Suit.Diamonds));
        clubsButton.onClick.AddListener(() => SelectSuit(Card.Suit.Clubs));
        spadesButton.onClick.AddListener(() => SelectSuit(Card.Suit.Spades));
    }
    
    void SelectSuit(Card.Suit suit)
    {
        OnSuitSelected?.Invoke(suit);
        gameObject.SetActive(false);
    }
}

// Použití v herní logice
async Task PlayQueenCard(QueenCard queen, IPlayerData player, GameContext context)
{
    // 1. Zobrazit dialog
    suitDialog.gameObject.SetActive(true);
    
    // 2. Počkat na výběr
    Card.Suit selectedSuit = await WaitForSuitSelection();
    
    // 3. Nastavit vybranou barvu
    queen.SelectSuit(selectedSuit);
    
    // 4. Zahrát kartu (polymorfní OnPlay nastaví ForcedSuit)
    queen.OnPlay(context, player);
}
```

## 5. Propojení AnimationEventReceiver

`AnimationEventReceiver` zůstává nezměněn - funguje na UI vrstvě:

```csharp
// V CardUI prefabu je AnimationEventReceiver
// Animation Clip volá metody jako:
//   - OnCardDealtStart() → AudioEvents.TriggerCardDealt()
//   - OnCardPlayedStart() → AudioEvents.TriggerCardPlayed()
//   - OnCardSwipedUpComplete() → notifikuje CardUI

// Příklad Animation Event v Unity:
// 1. Otevřít Animation window
// 2. Vybrat klíčový snímek
// 3. Add Event → vybrat metodu z AnimationEventReceiver
```

## 6. Kompletní Flow: Zahrání Karty

```
1. Hráč klikne na kartu v UI
        │
        ▼
2. CardUI.OnClick()
        │
        ▼
3. Kontrola: card.CanPlayOn(topCard, context)
        │
        ├─ false → Zobrazit error, zatřást kartou
        │
        └─ true ↓
        
4. Odebrat kartu z ruky hráče
        │
        ▼
5. CardStateService.SetCardLocation(card, Discard)
        │  └─→ CardState.LocationChanged event
        │              │
        │              ▼
        │      CardUI.OnLocationChanged()
        │              │
        │              ▼
        │      Spustit animaci "CardPlayed"
        │              │
        │              ▼
        │      Animation Event → AnimationEventReceiver
        │              │
        │              ▼
        │      AudioEvents.TriggerCardPlayed()
        │
        ▼
6. card.OnPlay(context, player)  // Polymorfní!
        │
        ├─ SevenCard: context.NotifyDrawPenalty(2)
        │                    │
        │                    ▼
        │             GameUI.HandleDrawPenalty()
        │                    │
        │                    ▼
        │             Zobrazit "Líznout 2 karty!"
        │
        ├─ AceCard: context.SkipNextPlayer = true
        │
        └─ QueenCard: context.NotifySuitChanged(suit)
                             │
                             ▼
                      GameUI.HandleSuitChanged()
                             │
                             ▼
                      Zobrazit indikátor barvy
```

## 7. Doporučená Struktura UI Prefabů

```
CardUI (Prefab)
├── CardImage (Image component)
├── Animator (Animation Controller)
├── AnimationEventReceiver (Script)
└── CardUI (Script)
    ├── [SerializeField] cardImage
    ├── [SerializeField] animator
    └── BaseCard card (runtime)

PlayerHandUI
├── HorizontalLayoutGroup
└── PlayerHand (Script)
    ├── cardPrefab reference
    ├── List<CardUI> cards
    └── Methods:
        - AddCard(BaseCard)
        - RemoveCard(BaseCard)
        - RefreshLayout()
```

## 8. Checklist pro Integraci

- [ ] Vytvořit/upravit `CardUI` aby přijímal `BaseCard`
- [ ] Přidat přihlášení k `CardState.LocationChanged`
- [ ] Vytvořit `GameUI` pro přihlášení k `GameContext` eventům
- [ ] Implementovat `SuitSelectionDialog` pro dámy
- [ ] Přidat vizuální indikátor pro `ForcedSuit`
- [ ] Přidat vizuální indikátor pro `PendingDrawCount`
- [ ] Testovat animace a zvuky
- [ ] Ověřit, že `AnimationEventReceiver` funguje správně

## Poznámky

- **Oddělení vrstev**: Herní logika nezná UI, komunikuje pouze přes eventy
- **Polymorfismus**: UI může kontrolovat typ karty pro speciální efekty
- **Téma karet**: `CardThemeService` poskytuje sprity, UI je pouze zobrazuje
- **Zvuky**: Řízeny animacemi přes `AnimationEventReceiver`
