using UnityEngine;

namespace Prsi.Data
{
    /// <summary>
    /// Data pro jedno téma karet
    /// </summary>
    [System.Serializable]
    public class CardThemeData
    {
        [Header("Theme Info")]
        public string id;
        public string displayName;
        
        [Header("Card Sprites")]
        [Tooltip("Sprites pro karty v pořadí: Hearts(7,8,9,10,J,Q,K,A), Diamonds(7,8,9,10,J,Q,K,A), Clubs(7,8,9,10,J,Q,K,A), Spades(7,8,9,10,J,Q,K,A)")]
        public Sprite[] cardSprites = new Sprite[32]; // 4 barvy × 8 hodnot
        
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
        /// Získá sprite pro kartu podle barvy a hodnoty
        /// </summary>
        public Sprite GetCardSprite(Card.Suit suit, Card.Rank rank)
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
        
        private int GetRankIndex(Card.Rank rank)
        {
            switch (rank)
            {
                case Card.Rank.Seven: return 0;
                case Card.Rank.Eight: return 1;
                case Card.Rank.Nine: return 2;
                case Card.Rank.Ten: return 3;
                case Card.Rank.Jack: return 4;
                case Card.Rank.Queen: return 5;
                case Card.Rank.King: return 6;
                case Card.Rank.Ace: return 7;
                default: return -1;
            }
        }
    }
}

