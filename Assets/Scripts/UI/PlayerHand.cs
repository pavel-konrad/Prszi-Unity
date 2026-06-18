using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Animations; // For AnimatorControllerParameterType

public class PlayerHand : MonoBehaviour
{
    [Header("Hand Display")]
    public Transform cardContainer;
    public GameObject cardPrefab;
    
    [Header("Animation")]
    public Animator handAnimator; // Animator for deal animations
    
    [Header("Player Reference")]
    public PlayerUI playerUI;
    
    private CardManager cardManager;
    private bool isInitialized = false;
    private List<CardUI> cardUIs = new List<CardUI>(); // List of all CardUI components
    
    void Awake()
    {
        // Check that required references are set
        if (cardContainer == null)
        {
            Debug.LogError("[PlayerHand] CardContainer is not set!");
            return;
        }
        
        if (cardPrefab == null)
        {
            Debug.LogError("[PlayerHand] CardPrefab is not set!");
            return;
        }
    }
    
    void Start()
    {
        // Find CardManager
        cardManager = Object.FindFirstObjectByType<CardManager>();
        
        // Subscribe to player-changed events
        if (GameSession.I.Human != null)
        {
            GameSession.I.Human.Changed += OnPlayerChanged;
        }
        else
        {
            Debug.LogError("[PlayerHand] GameSession.I.Human is null!");
        }
        
        // Subscribe to winner event
        if (cardManager != null)
        {
            cardManager.OnPlayerWon += OnPlayerWon;
        }
        
        if (playerUI != null)
        {
            playerUI.Bind(GameSession.I.Human); // Human player
        }
        
        isInitialized = true;
        
        // Update UI next frame once everything is initialised
        StartCoroutine(UpdateHandNextFrame());
    }
    
    System.Collections.IEnumerator UpdateHandNextFrame()
    {
        yield return null; // Wait for next frame
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
        
        // Do NOT call UpdateHand() here — only runs when dealing cards at game start
        // AddCardToHand() is used for new cards
        
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
    
    void OnPlayerWon(Player winner)
    {
        // Add a UI winner notification here if desired
    }
    
    public void UpdateHand(Player player)
    {
        if (!isInitialized)
        {
            Debug.LogError("[PlayerHand] UpdateHand called before initialisation!");
            return;
        }
        
        if (player == null)
        {
            Debug.LogError("[PlayerHand] Player is null!");
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
        
        // Automatically adjust spacing based on card count
        float cardSpacing = CalculateCardSpacing(player.hand.Count);
        float startX = -(player.hand.Count - 1) * cardSpacing * 0.5f; // Start from centre
        
        for (int i = 0; i < player.hand.Count; i++)
        {
            var card = player.hand[i];
            
            GameObject cardObj = Instantiate(cardPrefab, cardContainer);
            
            // Set card position
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
                // Ensure it has Animator
                Animator cardAnimator = cardObj.GetComponent<Animator>();
                if (cardAnimator == null)
                {
                    cardAnimator = cardObj.AddComponent<Animator>();
                    Debug.LogWarning($"[PlayerHand] Animator not found on {card.rank} of {card.suit}, creating a new one");
                }

                if (cardAnimator.runtimeAnimatorController == null)
                {
                    Debug.LogWarning($"[PlayerHand] Animator on {card.rank} of {card.suit} has no Animator Controller!");
                }
                
                // Ensure it has AnimationEventReceiver
                AnimationEventReceiver.EnsureComponent(cardObj);
                
                cardUI.SetCard(card);
                
                // Do NOT trigger CardIn animation here — only for newly added cards
                // CardIn animation is triggered in AddCardToHand()
                
                // Subscribe to events
                cardUI.OnCardSwipedUp += OnCardSwipedUp;
                cardUI.OnCardCharged += OnCardCharged;
                // Add to list
                cardUIs.Add(cardUI);
                
            }
            else
            {
                Debug.LogError("[PlayerHand] CardUI component not found on the prefab!");
            }
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
    
    void OnCardSwipedUp(Card card)
    {
        // Check whether player is on turn
        if (GameSession.I.Human.IsActive && GameSession.I.Human.CanPlay)
        {
            // Check whether the card is playable
            if (cardManager != null && cardManager.CanPlayCard(card))
            {
                
                // Find CardUI component for this card
                CardUI swipedCardUI = cardUIs.Find(cui => cui.card == card);
                if (swipedCardUI != null)
                {
                    // Subscribe to animation-complete event
                    System.Action<Card> onComplete = null;
                    onComplete = (completedCard) => {
                        if (completedCard == card)
                        {
                            // Animation complete — play card
                            cardManager.PlayCard(GameSession.I.Human, card);
                            // Unsubscribe from event
                            swipedCardUI.OnCardSwipedUpComplete -= onComplete;
                        }
                    };
                    swipedCardUI.OnCardSwipedUpComplete += onComplete;
                }
                else
                {
                    // Fallback — play card immediately
                    cardManager.PlayCard(GameSession.I.Human, card);
                }
            }
            else
            {
                // TODO: Show visual feedback (e.g. red effect)
            }
        }
    }
    

    
    void OnCardCharged(Card card)
    {
        // Find CardUI component for this card
        CardUI chargedCardUI = cardUIs.Find(cui => cui.card == card);
        if (chargedCardUI == null) return;
        
        
        // Discharge all other cards
        foreach (var cardUI in cardUIs)
        {
            if (cardUI != null && cardUI != chargedCardUI && cardUI.isCharged)
            {
                cardUI.SetCharged(false);
            }
        }
    }
    
    // Called from Animation Event for card-deal sound
    public void PlayDealSound()
    {
        // Sound plays via AnimationEventReceiver
        // This method is called from Animation Event
    }
    
    // Add new card to hand with CardIn animation
    public void AddCardToHand(Card card)
    {
        if (card == null || cardPrefab == null || cardContainer == null) return;
        
        
        // Create a new card
        GameObject cardObj = Instantiate(cardPrefab, cardContainer);
        
        // Set card position (append at end)
        RectTransform cardRect = cardObj.GetComponent<RectTransform>();
        if (cardRect != null)
        {
            // Recalculate positions of all cards
            float cardSpacing = CalculateCardSpacing(cardUIs.Count + 1);
            float startX = -(cardUIs.Count) * cardSpacing * 0.5f;
            
            // Set new card position
            cardRect.anchoredPosition = new Vector2(startX + cardUIs.Count * cardSpacing, 0);
            
            // Recalculate positions of existing cards
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
            // Ensure it has Animator
            Animator cardAnimator = cardObj.GetComponent<Animator>();
            if (cardAnimator == null)
            {
                cardAnimator = cardObj.AddComponent<Animator>();
                Debug.LogWarning($"[PlayerHand] Animator not found on {card.rank} of {card.suit}, creating a new one");
            }

            // Ensure it has AnimationEventReceiver
            AnimationEventReceiver.EnsureComponent(cardObj);
            
            cardUI.SetCard(card);
            
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
                    Debug.LogWarning($"[PlayerHand] CardIn trigger does not exist for {card.rank} of {card.suit}");
                }
            }
            
            // Subscribe to events
            cardUI.OnCardSwipedUp += OnCardSwipedUp;
            cardUI.OnCardCharged += OnCardCharged;
            
            // Add to list
            cardUIs.Add(cardUI);
            
        }
        else
        {
            Debug.LogError("[PlayerHand] CardUI component not found on the prefab!");
        }
    }
    
    // Remove card from hand
    public void RemoveCard(Card card)
    {
        if (card == null) return;
        
        
        // Find and remove CardUI component
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
            // Remove from UI
            cardUIs.Remove(cardUIToRemove);
            
            // Destroy GameObject
            if (cardUIToRemove.gameObject != null)
            {
                Destroy(cardUIToRemove.gameObject);
            }
            
            // Recalculate positions of remaining cards
            RecalculateCardPositions();
            
        }
        else
        {
            Debug.LogWarning($"[PlayerHand] Card {card.rank} of {card.suit} not found in UI for removal");
        }
    }
    
    // Recalculate positions of all cards
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
