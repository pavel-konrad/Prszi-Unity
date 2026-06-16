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
        if (cardBackImage == null)
            cardBackImage = GetComponent<Image>();
    }
    
    void Start()
    {
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
        
        // Zkontrolovat, zda je human hráč na tahu
        var human = GameSession.I.Human;
        if (human == null || !human.IsActive || !human.CanPlay)
        {
            return;
        }
        
        // Eso efekt: na čekající eso se NElíže — klik na balíček = „stůj" (přeskok).
        var rules = GameSession.I.Rules;
        if (rules != null && rules.AcePending)
        {
            rules.AcePending = false;
            StartCoroutine(StandThenPass(human));
            return;
        }

        // Najít CardManager
        CardManager cardManager = FindObjectOfType<CardManager>();
        if (cardManager == null)
        {
            Debug.LogError("[CardBack] CardManager nebyl nalezen!");
            return;
        }


        // V Prší si hráč může líznout kartu i když má hratelnou kartu

        // Líznout kartu
        Card drawnCard = cardManager.DrawCardForPlayer(human);
        
        // Exhausted deck and discard is a legal late-game state, not a bug.
        if (drawnCard == null)
            Debug.LogWarning("[CardBack] Žádná karta k líznutí (balíček i odhazovací prázdné), předávám tah");

        // In Prší drawing ends the turn — pass even if no card could be drawn.
        GameSession.I.ActivateNextPlayer();
    }

    // Human „stojí" na eso: krátká hláška v jeho baru, pak předá tah.
    System.Collections.IEnumerator StandThenPass(Player human)
    {
        GameLog.Record("STAND", human.Name);
        PlayerEffectDisplay.Instance?.ShowSkip(human);
        yield return new WaitForSeconds(1.0f);
        PlayerEffectDisplay.Instance?.Hide(human);
        GameSession.I.ActivateNextPlayer();
    }
}
