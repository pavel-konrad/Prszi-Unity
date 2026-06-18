using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Animations; // For AnimatorControllerParameterType

public class AIHand : MonoBehaviour
{
    [Header("AI Hand Display")]
    public Transform cardContainer;
    public GameObject cardBackPrefab; // Prefab for the card back
    
    [Header("Animation")]
    public Animator handAnimator; // Animator for deal animations
    
    [Header("AI Player")]
    public Player aiPlayer;
    public PlayerUI playerUI;
    
    [Header("Card Stack Reference")]
    public CardStack cardStack; // Reference to CardStack from CardManager
    
    void Start()
    {
        if (playerUI != null && aiPlayer != null)
        {
            playerUI.Bind(aiPlayer);
        }
        
        // Subscribe to player-changed events
        if (aiPlayer != null)
        {
            aiPlayer.Changed += OnPlayerChanged;
        }
        
        // Subscribe to CardStack change events
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
        // When CardStack changes, update UI
        if (aiPlayer != null)
        {
            UpdateHand(aiPlayer);
        }
    }
    
    public void UpdateHand(Player player)
    {
        
        if (cardContainer == null)
        {
            Debug.LogError($"[AIHand] {name} - CardContainer is null!");
            return;
        }
        
        if (cardBackPrefab == null)
        {
            Debug.LogError($"[AIHand] {name} - CardBackPrefab is null!");
            return;
        }
        
        // Clear existing cards
        int existingCards = cardContainer.childCount;
        
        // Use DestroyImmediate for immediate removal
        while (cardContainer.childCount > 0)
        {
            Transform child = cardContainer.GetChild(0);
            DestroyImmediate(child.gameObject);
        }
        
        
        // Use card count from CardStack when available
        int cardCount = player.hand.Count;
        if (cardStack != null)
        {
            cardCount = cardStack.Count;
        }
        
        
        // Automatically adjust spacing based on card count
        float cardSpacing = CalculateCardSpacing(cardCount);
        float startX = -(cardCount - 1) * cardSpacing * 0.5f; // Start from centre
        
        for (int i = 0; i < cardCount; i++)
        {
            GameObject cardBackObj = Instantiate(cardBackPrefab, cardContainer);
            
            // Set card position
            RectTransform cardRect = cardBackObj.GetComponent<RectTransform>();
            if (cardRect != null)
            {
                float xPos = startX + i * cardSpacing;
                cardRect.anchoredPosition = new Vector2(xPos, 0);
            }
            
            // Set card back sprite
            var cardBack = cardBackObj.GetComponent<CardBack>();
            if (cardBack != null && CardSpriteManager.Instance != null)
            {
                Sprite backSprite = CardSpriteManager.Instance.GetCardBackSprite();
                cardBack.SetCardBackSprite(backSprite);
            }
            
            // Do NOT trigger CardIn animation here — only AddCard() does for newly added cards
            // UpdateHand is called on all cards, not just new ones
            
            // Ensure it has AnimationEventReceiver
            AnimationEventReceiver.EnsureComponent(cardBackObj);
        }
        
    }
    
    // Start dealing animation (called from DealingState)
    public void StartDealingAnimation()
    {
        if (handAnimator != null)
        {
            handAnimator.SetTrigger("DealCards");
            // Sound plays via Animation Event
        }
    }
    
    // Automatically compute spacing between cards by count
    private float CalculateCardSpacing(int cardCount)
    {
        if (cardCount <= 2) return 120f;     // 1-2 cards — very wide spacing
        if (cardCount <= 3) return 100f;     // 3 cards — wide spacing
        if (cardCount <= 5) return 80f;      // 4-5 cards — normal spacing
        if (cardCount <= 8) return 60f;      // 6-8 cards — tighter spacing
        if (cardCount <= 12) return 45f;     // 9-12 cards — even tighter spacing
        return 35f;                          // 13+ cards — minimum spacing
    }
    
    // Set AI player
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
    
    // Set CardStack reference
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
    
    // Called from Animation Event for card-deal sound
    public void PlayDealSound()
    {
        // Sound plays via AnimationEventReceiver
        // This method is called from Animation Event
    }
    
    // Add new card to AI hand with CardIn animation
    public void AddCard(Card card)
    {
        if (card == null || cardBackPrefab == null || cardContainer == null) return;
        
        // Create new card (back face)
        GameObject cardObj = Instantiate(cardBackPrefab, cardContainer);
        
        // Set card position (append at end)
        RectTransform cardRect = cardObj.GetComponent<RectTransform>();
        if (cardRect != null)
        {
            // Recalculate positions of all cards
            float cardSpacing = 80f; // Fixed spacing for AI cards
            float startX = -(cardContainer.childCount - 1) * cardSpacing * 0.5f;
            
            // Set new card position
            cardRect.anchoredPosition = new Vector2(startX + (cardContainer.childCount - 1) * cardSpacing, 0);
            
            // Recalculate positions of existing cards
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
        
        // Start CardIn animation for newly added card
        Animator cardAnimator = cardObj.GetComponent<Animator>();
        if (cardAnimator == null)
        {
            cardAnimator = cardObj.AddComponent<Animator>();
            Debug.LogWarning($"[AIHand] Animator not found on the new card, creating a new one");
        }
        
        // Ensure it has AnimationEventReceiver
        AnimationEventReceiver.EnsureComponent(cardObj);
        
        // TRIGGER CardIn animation for newly added card
        if (cardAnimator != null && cardAnimator.runtimeAnimatorController != null)
        {
            // Check whether trigger exists
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
            }
            else
            {
                Debug.LogWarning($"[AIHand] CardIn trigger does not exist for AI card {card.rank} of {card.suit}");
            }
        }
        
    }
    
    // Remove card from AI hand
    public void RemoveCard(Card card)
    {
        if (card == null || cardContainer == null) return;
        
        
        // Find and remove card from UI
        for (int i = 0; i < cardContainer.childCount; i++)
        {
            Transform child = cardContainer.GetChild(i);
            if (child != null)
            {
                // Destroy GameObject
                Destroy(child.gameObject);
                
                // Recalculate positions of remaining cards
                RecalculateCardPositions();
                
                return;
            }
        }
        
        Debug.LogWarning($"[AIHand] {name} - Card {card.rank} of {card.suit} not found in UI for removal");
    }
    
    // Recalculate positions of all cards
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
