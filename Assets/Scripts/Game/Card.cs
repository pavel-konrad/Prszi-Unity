using UnityEngine;
using Prsi.Core;
using Prsi.Core.Cards;

[System.Serializable]
public class Card : ICardData
{
    public Suit suit;
    public Rank rank;
    public Sprite cardSprite;

    // Bridge to Prsi.Core: the legacy card speaks the domain language (ICardData).
    public Suit Suit => suit;
    public Rank Rank => rank;
    
    public Card(Suit s, Rank r)
    {
        suit = s;
        rank = r;
        LoadSprite();
    }
    
    void LoadSprite()
    {
        
        // Zajistit existenci CardSpriteManager
        CardSpriteManager spriteManager = CardSpriteManager.EnsureInstance();
        
        if (spriteManager != null)
        {
            
            // Try loading ready-made sprite
            cardSprite = spriteManager.GetCardSprite(suit, rank);
            
            // If no ready-made sprite, build from symbols
            if (cardSprite == null)
            {
                cardSprite = spriteManager.CreateCardSpriteFromSymbols(suit, rank);
            }
            
        }
        else
        {
            Debug.LogError("[Card] CardSpriteManager cannot be created!");
        }
    }
    
    public override string ToString()
    {
        return $"{rank} of {suit}";
    }
}
