using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DiscardPile : MonoBehaviour
{
    [Header("Discard Pile Display")]
    public Image cardImage;
    public TextMeshProUGUI cardText;
    public TextMeshProUGUI valueText;
    public TextMeshProUGUI suitText;
    
    [Header("Animation")]
    public Animator cardAnimator; // Animator for card animations
    
    [Header("Card Stack Reference")]
    public CardStack discardStack;
    
    private Card currentTopCard;
    
    void Start()
    {
        // Find CardManager and its discard stack
        if (discardStack == null)
        {
            CardManager cardManager = FindObjectOfType<CardManager>();
            if (cardManager != null)
            {
                discardStack = cardManager.discard;
            }
        }
        
        // Subscribe to discard-stack-changed events
        if (discardStack != null)
        {
            discardStack.OnCardsChanged += OnDiscardChanged;
            UpdateDisplay();
        }
        else
        {
            Debug.LogError("[DiscardPile] Discard stack not found!");
        }
    }
    
    void OnDestroy()
    {
        if (discardStack != null)
        {
            discardStack.OnCardsChanged -= OnDiscardChanged;
        }
    }
    
    void OnDiscardChanged()
    {
        UpdateDisplay();
    }
    
    void UpdateDisplay()
    {
        if (discardStack == null || discardStack.Count == 0)
        {
            // Empty discard pile — hide card
            if (cardImage != null) cardImage.enabled = false;
            if (cardText != null) cardText.text = "";
            if (valueText != null) valueText.text = "";
            if (suitText != null) suitText.text = "";
            currentTopCard = null;
            return;
        }
        
        // Show top card
        Card topCard = discardStack.cards[discardStack.cards.Count - 1];
        if (topCard == currentTopCard) return; // Same card, no update needed
        
        currentTopCard = topCard;
        
        
        // Start card-arrival animation
        if (cardAnimator != null)
        {
            cardAnimator.SetTrigger("CardIn");
            // Sound plays via Animation Event
        }
        else
        {
            // If no animator, set card content directly
            SetCardContent(topCard);
        }
    }
    
    // Set card content (no animation)
    private void SetCardContent(Card card)
    {
        
        // Set card sprite
        if (cardImage != null)
        {
            cardImage.enabled = true;
            if (card.cardSprite != null)
            {
                cardImage.sprite = card.cardSprite;
            }
            else
            {
                Debug.LogWarning("[DiscardPile] Card has no sprite!");
            }
        }
        else
        {
            Debug.LogError("[DiscardPile] cardImage is null!");
        }
        
        // Set card text (fallback when no sprite)
        if (cardText != null)
        {
            cardText.text = $"{card.rank}\n{card.suit}";
        }
        
        // Set value text
        if (valueText != null)
        {
            valueText.text = card.rank.ToString();
        }
        
        // Set suit text
        if (suitText != null)
        {
            suitText.text = card.suit.ToString();
        }
    }
    
    // Called from Animation Event to set card content during animation
    public void SetCurrentCardContent()
    {
        if (currentTopCard != null)
        {
            SetCardContent(currentTopCard);
        }
    }
    
    // Called from Animation Event for card-flip sound
    public void PlayCardFlipSound()
    {
        // Sound plays via AnimationEventReceiver
        // This method is called from Animation Event
    }
}
