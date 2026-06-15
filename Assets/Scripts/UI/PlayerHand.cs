using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Animations; // Pro AnimatorControllerParameterType

public class PlayerHand : MonoBehaviour
{
    [Header("Hand Display")]
    public Transform cardContainer;
    public GameObject cardPrefab;
    
    [Header("Animation")]
    public Animator handAnimator; // Animator pro animace rozdávání
    
    [Header("Player Reference")]
    public PlayerUI playerUI;
    
    private CardManager cardManager;
    private bool isInitialized = false;
    private List<CardUI> cardUIs = new List<CardUI>(); // Seznam všech CardUI komponent
    
    void Awake()
    {
        // Kontrola, zda máme potřebné reference
        if (cardContainer == null)
        {
            Debug.LogError("[PlayerHand] CardContainer není nastaven!");
            return;
        }
        
        if (cardPrefab == null)
        {
            Debug.LogError("[PlayerHand] CardPrefab není nastaven!");
            return;
        }
    }
    
    void Start()
    {
        // Najít CardManager
        cardManager = Object.FindFirstObjectByType<CardManager>();
        
        // Přihlásit se k událostem změny hráče
        if (GameSession.I.Human != null)
        {
            GameSession.I.Human.Changed += OnPlayerChanged;
        }
        else
        {
            Debug.LogError("[PlayerHand] GameSession.I.Human je null!");
        }
        
        // Přihlásit se k události vítěze
        if (cardManager != null)
        {
            cardManager.OnPlayerWon += OnPlayerWon;
        }
        
        if (playerUI != null)
        {
            playerUI.Bind(GameSession.I.Human); // Human player
        }
        
        isInitialized = true;
        
        // Aktualizovat UI v příštím frame, až bude vše inicializováno
        StartCoroutine(UpdateHandNextFrame());
    }
    
    System.Collections.IEnumerator UpdateHandNextFrame()
    {
        yield return null; // Počkat na příští frame
        if (GameSession.I.Human != null)
        {
            UpdateHand(GameSession.I.Human);
        }
    }
    
    void OnDestroy()
    {
        if (GameSession.I.Human != null)
        {
            GameSession.I.Human.Changed -= OnPlayerChanged;
        }
        
        if (cardManager != null)
        {
            cardManager.OnPlayerWon -= OnPlayerWon;
        }
    }
    
    void OnPlayerChanged(Player player)
    {
        if (!isInitialized) return;
        
        // NESPOUŠTĚT UpdateHand() zde - to se spouští jen při rozdávání karet na začátku hry
        // Pro nové karty se používá AddCardToHand()
        
        Debug.Log($"[PlayerHand] PlayerChanged event - NEspouštím UpdateHand pro {player.Name}");
    }
    
    // Spustit animaci rozdávání (volané z DealingState)
    public void StartDealingAnimation()
    {
        if (handAnimator != null)
        {
            handAnimator.SetTrigger("DealCards");
            // Zvuk se spustí přes Animation Event
        }
    }
    
    void OnPlayerWon(Player winner)
    {
        Debug.Log($"Hráč {winner.Name} vyhrál!");
        // Zde můžete přidat UI notifikaci o vítězi
    }
    
    public void UpdateHand(Player player)
    {
        if (!isInitialized)
        {
            Debug.LogError("[PlayerHand] UpdateHand volán před inicializací!");
            return;
        }
        
        if (player == null)
        {
            Debug.LogError("[PlayerHand] Player je null!");
            return;
        }
        
        // Clear existing cards and unsubscribe from events
        foreach (var cardUI in cardUIs)
        {
            if (cardUI != null)
            {
                cardUI.OnCardSwipedUp -= OnCardSwipedUp;
                cardUI.OnCardCharged -= OnCardCharged;
            }
        }
        cardUIs.Clear();
        
        foreach (Transform child in cardContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Automaticky přizpůsobit mezery podle počtu karet
        float cardSpacing = CalculateCardSpacing(player.hand.Count);
        float startX = -(player.hand.Count - 1) * cardSpacing * 0.5f; // Začít od středu
        
        for (int i = 0; i < player.hand.Count; i++)
        {
            var card = player.hand[i];
            
            GameObject cardObj = Instantiate(cardPrefab, cardContainer);
            
            // Nastavit pozici karty
            RectTransform cardRect = cardObj.GetComponent<RectTransform>();
            if (cardRect != null)
            {
                float xPos = startX + i * cardSpacing;
                cardRect.anchoredPosition = new Vector2(xPos, 0);
            }
            
            // Setup card visual
            var cardUI = cardObj.GetComponent<CardUI>();
            if (cardUI != null)
            {
                // Zajistit, že má Animator
                Animator cardAnimator = cardObj.GetComponent<Animator>();
                if (cardAnimator == null)
                {
                    cardAnimator = cardObj.AddComponent<Animator>();
                    Debug.LogWarning($"[PlayerHand] Animator nebyl nalezen na {card.rank} of {card.suit}, vytvářím nový");
                }
                
                // Debug: zkontrolovat Animator Controller
                if (cardAnimator.runtimeAnimatorController == null)
                {
                    Debug.LogWarning($"[PlayerHand] Animator na {card.rank} of {card.suit} nemá Animator Controller!");
                }
                else
                {
                    Debug.Log($"[PlayerHand] Animator Controller: {cardAnimator.runtimeAnimatorController.name}");
                }
                
                // Zajistit, že má AnimationEventReceiver
                AnimationEventReceiver.EnsureComponent(cardObj);
                
                cardUI.SetCard(card);
                
                // NESPOUŠTĚT CardIn animaci zde - to se spouští jen na nově přidané karty
                // CardIn animace se spouští v AddCardToHand() metodě
                
                // Přihlásit se k událostem
                cardUI.OnCardSwipedUp += OnCardSwipedUp;
                cardUI.OnCardCharged += OnCardCharged;
                // Přidat do seznamu
                cardUIs.Add(cardUI);
                
                Debug.Log($"[PlayerHand] Karta {card.rank} of {card.suit} vytvořena s AnimationEventReceiver a Animator");
            }
            else
            {
                Debug.LogError("[PlayerHand] CardUI komponenta nebyla nalezena na prefabu!");
            }
        }
    }
    
    // Automaticky vypočítat mezery mezi kartami podle počtu karet
    private float CalculateCardSpacing(int cardCount)
    {
        if (cardCount <= 2) return 120f;     // 1-2 karty - velmi široké mezery
        if (cardCount <= 3) return 100f;     // 3 karty - široké mezery
        if (cardCount <= 5) return 80f;      // 4-5 karet - normální mezery
        if (cardCount <= 8) return 60f;      // 6-8 karet - menší mezery
        if (cardCount <= 12) return 45f;     // 9-12 karet - ještě menší mezery
        return 35f;                          // 13+ karet - minimální mezery
    }
    
    void OnCardSwipedUp(Card card)
    {
        // Kontrola, zda je hráč na tahu
        if (GameSession.I.Human.IsActive && GameSession.I.Human.CanPlay)
        {
            // Kontrola, zda lze kartu zahrát
            if (cardManager != null && cardManager.CanPlayCard(card))
            {
                Debug.Log($"[PlayerHand] Hráč hraje kartu: {card.rank} of {card.suit}");
                
                // Najít CardUI komponentu pro tuto kartu
                CardUI swipedCardUI = cardUIs.Find(cui => cui.card == card);
                if (swipedCardUI != null)
                {
                    // Přihlásit se k události dokončení animace
                    System.Action<Card> onComplete = null;
                    onComplete = (completedCard) => {
                        if (completedCard == card)
                        {
                            // Animace dokončena - hrát kartu
                            cardManager.PlayCard(GameSession.I.Human, card);
                            // Odhlásit se od události
                            swipedCardUI.OnCardSwipedUpComplete -= onComplete;
                        }
                    };
                    swipedCardUI.OnCardSwipedUpComplete += onComplete;
                }
                else
                {
                    // Fallback - hrát kartu okamžitě
                    cardManager.PlayCard(GameSession.I.Human, card);
                }
            }
            else
            {
                Debug.Log($"[PlayerHand] Karta {card.rank} of {card.suit} není hratelná!");
                // TODO: Zobrazit vizuální feedback (např. červený efekt)
            }
        }
        else
        {
            Debug.Log("[PlayerHand] Nejste na tahu nebo nemůžete hrát!");
        }
    }
    

    
    void OnCardCharged(Card card)
    {
        // Najít CardUI komponentu pro tuto kartu
        CardUI chargedCardUI = cardUIs.Find(cui => cui.card == card);
        if (chargedCardUI == null) return;
        
        Debug.Log($"[PlayerHand] Karta {card.rank} of {card.suit} se nabila - vybíjím ostatní karty");
        
        // Vybit všechny ostatní karty
        foreach (var cardUI in cardUIs)
        {
            if (cardUI != null && cardUI != chargedCardUI && cardUI.isCharged)
            {
                cardUI.SetCharged(false);
                Debug.Log($"[PlayerHand] Vybita karta: {cardUI.card?.rank} of {cardUI.card?.suit}");
            }
        }
    }
    
    // Volané z Animation Event pro zvuk rozdávání karet
    public void PlayDealSound()
    {
        // Zvuk se spustí přes AnimationEventReceiver
        // Tato metoda je volána z Animation Event
    }
    
    // Přidat novou kartu do ruky s CardIn animací
    public void AddCardToHand(Card card)
    {
        if (card == null || cardPrefab == null || cardContainer == null) return;
        
        Debug.Log($"[PlayerHand] AddCardToHand voláno pro kartu: {card.rank} of {card.suit}");
        Debug.Log($"[PlayerHand] cardPrefab: {(cardPrefab != null ? cardPrefab.name : "NULL")}");
        Debug.Log($"[PlayerHand] cardContainer: {(cardContainer != null ? cardContainer.name : "NULL")}");
        Debug.Log($"[PlayerHand] isInitialized: {isInitialized}");
        
        // Vytvořit novou kartu
        GameObject cardObj = Instantiate(cardPrefab, cardContainer);
        
        // Nastavit pozici karty (přidat na konec)
        RectTransform cardRect = cardObj.GetComponent<RectTransform>();
        if (cardRect != null)
        {
            // Přepočítat pozice všech karet
            float cardSpacing = CalculateCardSpacing(cardUIs.Count + 1);
            float startX = -(cardUIs.Count) * cardSpacing * 0.5f;
            
            // Nastavit pozici nové karty
            cardRect.anchoredPosition = new Vector2(startX + cardUIs.Count * cardSpacing, 0);
            
            // Přepočítat pozice existujících karet
            for (int i = 0; i < cardUIs.Count; i++)
            {
                if (cardUIs[i] != null)
                {
                    float xPos = startX + i * cardSpacing;
                    cardUIs[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(xPos, 0);
                }
            }
        }
        
        // Setup card visual
        var cardUI = cardObj.GetComponent<CardUI>();
        if (cardUI != null)
        {
            // Zajistit, že má Animator
            Animator cardAnimator = cardObj.GetComponent<Animator>();
            if (cardAnimator == null)
            {
                cardAnimator = cardObj.AddComponent<Animator>();
                Debug.LogWarning($"[PlayerHand] Animator nebyl nalezen na {card.rank} of {card.suit}, vytvářím nový");
            }
            
            // Zajistit, že má AnimationEventReceiver
            AnimationEventReceiver.EnsureComponent(cardObj);
            
            cardUI.SetCard(card);
            
            // SPUSTIT CardIn animaci pro nově přidanou kartu
            if (cardAnimator != null && cardAnimator.runtimeAnimatorController != null)
            {
                // Zkontrolovat, jestli trigger existuje
                bool hasCardInTrigger = false;
                foreach (var param in cardAnimator.parameters)
                {
                    if (param.name == "CardIn" && param.type == AnimatorControllerParameterType.Trigger)
                    {
                        hasCardInTrigger = true;
                        break;
                    }
                }
                
                if (hasCardInTrigger)
                {
                    cardAnimator.SetTrigger("CardIn");
                    Debug.Log($"[PlayerHand] CardIn trigger spuštěn pro nově přidanou kartu: {card.rank} of {card.suit}");
                }
                else
                {
                    Debug.LogWarning($"[PlayerHand] CardIn trigger neexistuje pro {card.rank} of {card.suit}");
                }
            }
            
            // Přihlásit se k událostem
            cardUI.OnCardSwipedUp += OnCardSwipedUp;
            cardUI.OnCardCharged += OnCardCharged;
            
            // Přidat do seznamu
            cardUIs.Add(cardUI);
            
            Debug.Log($"[PlayerHand] Nová karta {card.rank} of {card.suit} přidána s CardIn animací");
        }
        else
        {
            Debug.LogError("[PlayerHand] CardUI komponenta nebyla nalezena na prefabu!");
        }
    }
    
    // Odebrat kartu z ruky
    public void RemoveCard(Card card)
    {
        if (card == null) return;
        
        Debug.Log($"[PlayerHand] RemoveCard voláno pro kartu: {card.rank} of {card.suit}");
        
        // Najít a odebrat CardUI komponentu
        CardUI cardUIToRemove = null;
        for (int i = 0; i < cardUIs.Count; i++)
        {
            if (cardUIs[i] != null && cardUIs[i].card == card)
            {
                cardUIToRemove = cardUIs[i];
                break;
            }
        }
        
        if (cardUIToRemove != null)
        {
            // Odebrat z UI
            cardUIs.Remove(cardUIToRemove);
            
            // Zničit GameObject
            if (cardUIToRemove.gameObject != null)
            {
                Destroy(cardUIToRemove.gameObject);
            }
            
            // Přepočítat pozice zbývajících karet
            RecalculateCardPositions();
            
            Debug.Log($"[PlayerHand] Karta {card.rank} of {card.suit} odebrána z UI, zbývá {cardUIs.Count} karet");
        }
        else
        {
            Debug.LogWarning($"[PlayerHand] Karta {card.rank} of {card.suit} nebyla nalezena v UI pro odebrání");
        }
    }
    
    // Přepočítat pozice všech karet
    private void RecalculateCardPositions()
    {
        if (cardUIs.Count == 0) return;
        
        float cardSpacing = CalculateCardSpacing(cardUIs.Count);
        float startX = -(cardUIs.Count - 1) * cardSpacing * 0.5f;
        
        for (int i = 0; i < cardUIs.Count; i++)
        {
            if (cardUIs[i] != null)
            {
                float xPos = startX + i * cardSpacing;
                cardUIs[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(xPos, 0);
            }
        }
    }
}
