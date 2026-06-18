using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CardBack : MonoBehaviour, IPointerClickHandler
{
    [Header("Card Back Display")]
    public Image cardBackImage;
    public Sprite cardBackSprite; // Sprite for card back
    
    void Awake()
    {
        if (cardBackImage == null)
            cardBackImage = GetComponent<Image>();
    }
    
    void Start()
    {
        // Try loading sprite from CardSpriteManager
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
    
    // Set sprite for card back
    public void SetCardBackSprite(Sprite sprite)
    {
        cardBackSprite = sprite;
        UpdateDisplay();
    }
    
    // IPointerClickHandler implementation for clicking the deck
    public void OnPointerClick(PointerEventData eventData)
    {
        
        // Check whether the human player is on turn
        var human = GameSession.I.Human;
        if (human == null || !human.IsActive || !human.CanPlay)
        {
            return;
        }
        
        // Ace effect: do NOT draw on pending Ace — clicking the deck = "stand" (skip).
        var rules = GameSession.I.Rules;
        if (rules != null && rules.AcePending)
        {
            rules.AcePending = false;
            StartCoroutine(StandThenPass(human));
            return;
        }

        // Find CardManager
        CardManager cardManager = FindObjectOfType<CardManager>();
        if (cardManager == null)
        {
            Debug.LogError("[CardBack] CardManager not found!");
            return;
        }


        // In Prší the player may draw even when they have a playable card

        // Draw a card
        Card drawnCard = cardManager.DrawCardForPlayer(human);
        
        // Exhausted deck and discard is a legal late-game state, not a bug.
        if (drawnCard == null)
            Debug.LogWarning("[CardBack] No card to draw (deck and discard empty), passing turn");

        // In Prší drawing ends the turn — pass even if no card could be drawn.
        GameSession.I.ActivateNextPlayer();
    }

    // Human "stands" on Ace: short message in their bar, then pass turn.
    System.Collections.IEnumerator StandThenPass(Player human)
    {
        GameLog.Record("STAND", human.Name);
        PlayerEffectDisplay.Instance?.ShowSkip(human);
        yield return new WaitForSeconds(1.0f);
        PlayerEffectDisplay.Instance?.Hide(human);
        GameSession.I.ActivateNextPlayer();
    }
}
