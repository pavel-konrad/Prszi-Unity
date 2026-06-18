using UnityEngine;
using Prsi.Core.Cards;

namespace Prsi.Data
{
    /// <summary>
    /// Data for a single card theme
    /// </summary>
    [System.Serializable]
    public class CardThemeData
    {
        [Header("Theme Info")]
        public string id;
        public string displayName;
        
        [Header("Card Sprites")]
        [Tooltip("Sprites for cards in order: Hearts(7,8,9,10,J,Q,K,A), Diamonds(7,8,9,10,J,Q,K,A), Clubs(7,8,9,10,J,Q,K,A), Spades(7,8,9,10,J,Q,K,A)")]
        public Sprite[] cardSprites = new Sprite[32]; // 4 suits × 8 ranks
        
        [Header("Card Back")]
        public Sprite cardBackSprite;
        
        [Header("Symbols (Optional)")]
        public Sprite[] rankSymbols; // 7, 8, 9, 10, J, Q, K, A
        public Sprite heartSymbol;
        public Sprite diamondSymbol;
        public Sprite clubSymbol;
        public Sprite spadeSymbol;
        
        public CardThemeData(string id, string displayName)
        {
            this.id = id;
            this.displayName = displayName;
        }
        
        /// <summary>
        /// Gets the sprite for a card by suit and rank
        /// </summary>
        public Sprite GetCardSprite(Suit suit, Rank rank)
        {
            int suitIndex = (int)suit; // 0=Hearts, 1=Diamonds, 2=Clubs, 3=Spades
            int rankIndex = GetRankIndex(rank);
            
            if (rankIndex < 0) return null;
            
            int spriteIndex = suitIndex * 8 + rankIndex;
            
            if (spriteIndex >= 0 && spriteIndex < cardSprites.Length)
            {
                return cardSprites[spriteIndex];
            }
            
            return null;
        }
        
        private int GetRankIndex(Rank rank)
        {
            switch (rank)
            {
                case Rank.Seven: return 0;
                case Rank.Eight: return 1;
                case Rank.Nine: return 2;
                case Rank.Ten: return 3;
                case Rank.Jack: return 4;
                case Rank.Queen: return 5;
                case Rank.King: return 6;
                case Rank.Ace: return 7;
                default: return -1;
            }
        }
    }
}

