using UnityEngine;

[System.Serializable]
public class Card
{
    public enum Suit { Hearts, Diamonds, Clubs, Spades }
    public enum Rank { Seven = 7, Eight = 8, Nine = 9, Ten = 10, Jack = 11, Queen = 12, King = 13, Ace = 14 }
    
    public Suit suit;
    public Rank rank;
    public Sprite cardSprite;
    
    public Card(Suit s, Rank r)
    {
        suit = s;
        rank = r;
        Debug.Log($"[Card] Vytvářím kartu: {rank} of {suit}");
        LoadSprite();
    }
    
    void LoadSprite()
    {
        Debug.Log($"[Card] LoadSprite pro {rank} of {suit}");
        
        // Zajistit existenci CardSpriteManager
        CardSpriteManager spriteManager = CardSpriteManager.EnsureInstance();
        
        if (spriteManager != null)
        {
            Debug.Log("[Card] CardSpriteManager nalezen");
            
            // Zkusit načíst hotový sprite
            cardSprite = spriteManager.GetCardSprite(suit, rank);
            
            // Pokud není hotový sprite, vytvořit z symbolů
            if (cardSprite == null)
            {
                Debug.Log("[Card] Hotový sprite nenalezen, zkouším vytvořit z symbolů");
                cardSprite = spriteManager.CreateCardSpriteFromSymbols(suit, rank);
            }
            
            Debug.Log($"[Card] Výsledný sprite: {(cardSprite != null ? cardSprite.name : "null")}");
        }
        else
        {
            Debug.LogError("[Card] CardSpriteManager nelze vytvořit!");
        }
    }
    
    public override string ToString()
    {
        return $"{rank} of {suit}";
    }
}
