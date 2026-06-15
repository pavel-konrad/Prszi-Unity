using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CardBack : MonoBehaviour, IPointerClickHandler
{
    [Header("Card Back Display")]
    public Image cardBackImage;
    public Sprite cardBackSprite; // Sprite pro zadní stranu karty
    
    void Awake()
    {
        Debug.Log("[CardBack] Awake voláno");
        if (cardBackImage == null)
            cardBackImage = GetComponent<Image>();
    }
    
    void Start()
    {
        Debug.Log("[CardBack] Start voláno");
        // Zkusit načíst sprite z CardSpriteManager
        if (cardBackSprite == null && CardSpriteManager.Instance != null)
        {
            cardBackSprite = CardSpriteManager.Instance.GetCardBackSprite();
        }
        
        UpdateDisplay();
    }
    
    void UpdateDisplay()
    {
        if (cardBackImage != null && cardBackSprite != null)
        {
            cardBackImage.sprite = cardBackSprite;
        }
    }
    
    // Nastavit sprite pro zadní stranu
    public void SetCardBackSprite(Sprite sprite)
    {
        cardBackSprite = sprite;
        UpdateDisplay();
    }
    
    // Implementace IPointerClickHandler pro kliknutí na balíček
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("[CardBack] Kliknutí na balíček detekováno");
        
        // Zkontrolovat, zda je human hráč na tahu
        var human = GameSession.I.Human;
        if (human == null || !human.IsActive || !human.CanPlay)
        {
            Debug.Log($"[CardBack] Human hráč není na tahu nebo nemůže hrát! human={human?.Name}, IsActive={human?.IsActive}, CanPlay={human?.CanPlay}");
            return;
        }
        
        // Najít CardManager
        CardManager cardManager = FindObjectOfType<CardManager>();
        if (cardManager == null)
        {
            Debug.LogError("[CardBack] CardManager nebyl nalezen!");
            return;
        }
        
        // Debug: zkontrolovat stav balíčku
        Debug.Log($"[CardBack] Stav balíčku: deck.cards.Count={cardManager.deck?.cards?.Count ?? -1}");
        
        // V Prší si hráč může líznout kartu i když má hratelnou kartu
        
        // Líznout kartu
        Debug.Log("[CardBack] Hráč si lízne kartu z balíčku");
        Card drawnCard = cardManager.DrawCardForPlayer(human);
        
        if (drawnCard != null)
            Debug.Log($"[CardBack] Hráč si líznul kartu {drawnCard}, předávám tah");
        else
            // Deck and discard are exhausted (a legal late-game state, not a bug).
            // Pass the turn anyway so the human can't soft-lock on the empty deck.
            Debug.LogWarning("[CardBack] Žádná karta k líznutí (balíček i odhazovací prázdné), předávám tah");

        // In Prší drawing ends the turn — pass even if no card could be drawn.
        GameSession.I.ActivateNextPlayer();
    }
}
