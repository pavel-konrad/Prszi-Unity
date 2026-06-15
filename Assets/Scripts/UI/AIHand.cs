using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Animations; // Pro AnimatorControllerParameterType

public class AIHand : MonoBehaviour
{
    [Header("AI Hand Display")]
    public Transform cardContainer;
    public GameObject cardBackPrefab; // Prefab pro zadní stranu karty
    
    [Header("Animation")]
    public Animator handAnimator; // Animator pro animace rozdávání
    
    [Header("AI Player")]
    public Player aiPlayer;
    public PlayerUI playerUI;
    
    [Header("Card Stack Reference")]
    public CardStack cardStack; // Reference na CardStack z CardManager
    
    void Start()
    {
        if (playerUI != null && aiPlayer != null)
        {
            playerUI.Bind(aiPlayer);
        }
        
        // Přihlásit se k událostem změny hráče
        if (aiPlayer != null)
        {
            aiPlayer.Changed += OnPlayerChanged;
        }
        
        // Přihlásit se k událostem změny CardStack
        if (cardStack != null)
        {
            cardStack.OnCardsChanged += OnCardStackChanged;
        }
    }
    
    void OnDestroy()
    {
        if (aiPlayer != null)
        {
            aiPlayer.Changed -= OnPlayerChanged;
        }
        
        if (cardStack != null)
        {
            cardStack.OnCardsChanged -= OnCardStackChanged;
        }
    }
    
    void OnPlayerChanged(Player player)
    {
        UpdateHand(player);
    }
    
    void OnCardStackChanged()
    {
        // Když se změní CardStack, aktualizovat UI
        if (aiPlayer != null)
        {
            UpdateHand(aiPlayer);
        }
    }
    
    public void UpdateHand(Player player)
    {
        Debug.Log($"[AIHand] {name} - UpdateHand voláno pro hráče: {player?.Name}");
        
        if (cardContainer == null)
        {
            Debug.LogError($"[AIHand] {name} - CardContainer je null!");
            return;
        }
        
        if (cardBackPrefab == null)
        {
            Debug.LogError($"[AIHand] {name} - CardBackPrefab je null!");
            return;
        }
        
        // Clear existing cards
        int existingCards = cardContainer.childCount;
        Debug.Log($"[AIHand] {name} - Mažu {existingCards} existujících karet");
        
        // Použít DestroyImmediate pro okamžité odstranění
        while (cardContainer.childCount > 0)
        {
            Transform child = cardContainer.GetChild(0);
            DestroyImmediate(child.gameObject);
        }
        
        Debug.Log($"[AIHand] {name} - Po mazání zbývá {cardContainer.childCount} karet");
        
        // Použít počet karet z CardStack, pokud je dostupný
        int cardCount = player.hand.Count;
        if (cardStack != null)
        {
            cardCount = cardStack.Count;
        }
        
        Debug.Log($"[AIHand] {name} - Vytvářím {cardCount} karet (player.hand.Count={player.hand.Count}, cardStack.Count={cardStack?.Count ?? -1})");
        
        // Automaticky přizpůsobit mezery podle počtu karet
        float cardSpacing = CalculateCardSpacing(cardCount);
        float startX = -(cardCount - 1) * cardSpacing * 0.5f; // Začít od středu
        
        for (int i = 0; i < cardCount; i++)
        {
            GameObject cardBackObj = Instantiate(cardBackPrefab, cardContainer);
            
            // Nastavit pozici karty
            RectTransform cardRect = cardBackObj.GetComponent<RectTransform>();
            if (cardRect != null)
            {
                float xPos = startX + i * cardSpacing;
                cardRect.anchoredPosition = new Vector2(xPos, 0);
            }
            
            // Nastavit sprite zadní strany karty
            var cardBack = cardBackObj.GetComponent<CardBack>();
            if (cardBack != null && CardSpriteManager.Instance != null)
            {
                Sprite backSprite = CardSpriteManager.Instance.GetCardBackSprite();
                cardBack.SetCardBackSprite(backSprite);
            }
            
            // NESPOUŠTĚT CardIn animaci zde - to se spouští jen v AddCard() pro nově přidané karty
            // UpdateHand se volá na všech kartách, ne jen na nových
            
            // Zajistit, že má AnimationEventReceiver
            AnimationEventReceiver.EnsureComponent(cardBackObj);
        }
        
        Debug.Log($"[AIHand] {name} - Vytvořeno {cardCount} karet, aktuální childCount: {cardContainer.childCount}");
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
    
    // Nastavit AI hráče
    public void SetAIPlayer(Player player)
    {
        if (aiPlayer != null)
        {
            aiPlayer.Changed -= OnPlayerChanged;
        }
        
        aiPlayer = player;
        
        if (aiPlayer != null)
        {
            aiPlayer.Changed += OnPlayerChanged;
            
            if (playerUI != null)
            {
                playerUI.Bind(aiPlayer);
            }
        }
    }
    
    // Nastavit CardStack reference
    public void SetCardStack(CardStack stack)
    {
        if (cardStack != null)
        {
            cardStack.OnCardsChanged -= OnCardStackChanged;
        }
        
        cardStack = stack;
        
        if (cardStack != null)
        {
            cardStack.OnCardsChanged += OnCardStackChanged;
        }
    }
    
    // Volané z Animation Event pro zvuk rozdávání karet
    public void PlayDealSound()
    {
        // Zvuk se spustí přes AnimationEventReceiver
        // Tato metoda je volána z Animation Event
    }
    
    // Přidat novou kartu do AI ruky s CardIn animací
    public void AddCard(Card card)
    {
        if (card == null || cardBackPrefab == null || cardContainer == null) return;
        
        // Vytvořit novou kartu (zadní strana)
        GameObject cardObj = Instantiate(cardBackPrefab, cardContainer);
        
        // Nastavit pozici karty (přidat na konec)
        RectTransform cardRect = cardObj.GetComponent<RectTransform>();
        if (cardRect != null)
        {
            // Přepočítat pozice všech karet
            float cardSpacing = 80f; // Fixní mezera pro AI karty
            float startX = -(cardContainer.childCount - 1) * cardSpacing * 0.5f;
            
            // Nastavit pozici nové karty
            cardRect.anchoredPosition = new Vector2(startX + (cardContainer.childCount - 1) * cardSpacing, 0);
            
            // Přepočítat pozice existujících karet
            for (int i = 0; i < cardContainer.childCount - 1; i++)
            {
                Transform child = cardContainer.GetChild(i);
                if (child != null)
                {
                    float xPos = startX + i * cardSpacing;
                    child.GetComponent<RectTransform>().anchoredPosition = new Vector2(xPos, 0);
                }
            }
        }
        
        // Spustit CardIn animaci pro nově přidanou kartu
        Animator cardAnimator = cardObj.GetComponent<Animator>();
        if (cardAnimator == null)
        {
            cardAnimator = cardObj.AddComponent<Animator>();
            Debug.LogWarning($"[AIHand] Animator nebyl nalezen na nové kartě, vytvářím nový");
        }
        
        // Zajistit, že má AnimationEventReceiver
        AnimationEventReceiver.EnsureComponent(cardObj);
        
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
                Debug.Log($"[AIHand] CardIn trigger spuštěn pro nově přidanou AI kartu: {card.rank} of {card.suit}");
            }
            else
            {
                Debug.LogWarning($"[AIHand] CardIn trigger neexistuje pro AI kartu {card.rank} of {card.suit}");
            }
        }
        
        Debug.Log($"[AIHand] Nová AI karta {card.rank} of {card.suit} přidána s CardIn animací");
    }
    
    // Odebrat kartu z AI ruky
    public void RemoveCard(Card card)
    {
        if (card == null || cardContainer == null) return;
        
        Debug.Log($"[AIHand] {name} - RemoveCard voláno pro kartu: {card.rank} of {card.suit}");
        
        // Najít a odebrat kartu z UI
        for (int i = 0; i < cardContainer.childCount; i++)
        {
            Transform child = cardContainer.GetChild(i);
            if (child != null)
            {
                // Zničit GameObject
                Destroy(child.gameObject);
                
                // Přepočítat pozice zbývajících karet
                RecalculateCardPositions();
                
                Debug.Log($"[AIHand] {name} - Karta {card.rank} of {card.suit} odebrána z UI, zbývá {cardContainer.childCount - 1} karet");
                return;
            }
        }
        
        Debug.LogWarning($"[AIHand] {name} - Karta {card.rank} of {card.suit} nebyla nalezena v UI pro odebrání");
    }
    
    // Přepočítat pozice všech karet
    private void RecalculateCardPositions()
    {
        if (cardContainer == null || cardContainer.childCount == 0) return;
        
        float cardSpacing = CalculateCardSpacing(cardContainer.childCount);
        float startX = -(cardContainer.childCount - 1) * cardSpacing * 0.5f;
        
        for (int i = 0; i < cardContainer.childCount; i++)
        {
            Transform child = cardContainer.GetChild(i);
            if (child != null)
            {
                float xPos = startX + i * cardSpacing;
                child.GetComponent<RectTransform>().anchoredPosition = new Vector2(xPos, 0);
            }
        }
    }
}
