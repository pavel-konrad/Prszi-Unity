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
    public Animator cardAnimator; // Animator pro animace karet
    
    [Header("Card Stack Reference")]
    public CardStack discardStack;
    
    private Card currentTopCard;
    
    void Start()
    {
        // Najít CardManager a jeho discard stack
        if (discardStack == null)
        {
            CardManager cardManager = FindObjectOfType<CardManager>();
            if (cardManager != null)
            {
                discardStack = cardManager.discard;
            }
        }
        
        // Přihlásit se k událostem změny discard stacku
        if (discardStack != null)
        {
            discardStack.OnCardsChanged += OnDiscardChanged;
            UpdateDisplay();
        }
        else
        {
            Debug.LogError("[DiscardPile] Discard stack nebyl nalezen!");
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
            // Prázdný discard pile - skrýt kartu
            if (cardImage != null) cardImage.enabled = false;
            if (cardText != null) cardText.text = "";
            if (valueText != null) valueText.text = "";
            if (suitText != null) suitText.text = "";
            currentTopCard = null;
            return;
        }
        
        // Zobrazit vrchní kartu
        Card topCard = discardStack.cards[discardStack.cards.Count - 1];
        if (topCard == currentTopCard) return; // Stejná karta, není potřeba aktualizovat
        
        currentTopCard = topCard;
        
        
        // Spustit animaci příchodu karty
        if (cardAnimator != null)
        {
            cardAnimator.SetTrigger("CardIn");
            // Zvuk se spustí přes Animation Event
        }
        else
        {
            // Pokud nemáme animátor, nastavit obsah karty přímo
            SetCardContent(topCard);
        }
    }
    
    // Nastavit obsah karty (bez animace)
    private void SetCardContent(Card card)
    {
        
        // Nastavit sprite karty
        if (cardImage != null)
        {
            cardImage.enabled = true;
            if (card.cardSprite != null)
            {
                cardImage.sprite = card.cardSprite;
            }
            else
            {
                Debug.LogWarning("[DiscardPile] Karta nemá sprite!");
            }
        }
        else
        {
            Debug.LogError("[DiscardPile] cardImage je null!");
        }
        
        // Nastavit text karty (fallback pokud nemáme sprite)
        if (cardText != null)
        {
            cardText.text = $"{card.rank}\n{card.suit}";
        }
        
        // Nastavit value text
        if (valueText != null)
        {
            valueText.text = card.rank.ToString();
        }
        
        // Nastavit suit text
        if (suitText != null)
        {
            suitText.text = card.suit.ToString();
        }
    }
    
    // Volané z Animation Event pro nastavení obsahu karty během animace
    public void SetCurrentCardContent()
    {
        if (currentTopCard != null)
        {
            SetCardContent(currentTopCard);
        }
    }
    
    // Volané z Animation Event pro zvuk otočení karty
    public void PlayCardFlipSound()
    {
        // Zvuk se spustí přes AnimationEventReceiver
        // Tato metoda je volána z Animation Event
    }
}
