using System.Collections.Generic;

namespace Prsi.Core.Cards
{
    /// <summary>
    /// Factory pro vytváření karet správného typu
    /// Ekvivalent JS card_factory.js z prszi projektu
    /// 
    /// Centralizuje logiku vytváření karet:
    /// - Sedmy (7) → SevenCard
    /// - Esa (A) → AceCard
    /// - Dámy (Q) → QueenCard
    /// - Ostatní → RegularCard
    /// </summary>
    public static class CardFactory
    {
        /// <summary>
        /// Vytvoří kartu správného typu podle hodnoty
        /// </summary>
        /// <param name="suit">Barva karty</param>
        /// <param name="rank">Hodnota karty</param>
        /// <returns>Instance správného typu karty</returns>
        public static BaseCard Create(Card.Suit suit, Card.Rank rank)
        {
            return rank switch
            {
                Card.Rank.Seven => new SevenCard(suit, rank),
                Card.Rank.Ace => new AceCard(suit, rank),
                Card.Rank.Queen => new QueenCard(suit, rank),
                _ => new RegularCard(suit, rank)
            };
        }
        
        /// <summary>
        /// Vytvoří kompletní balíček 32 karet (německý mariáš)
        /// </summary>
        /// <returns>Seznam všech 32 karet</returns>
        public static List<BaseCard> CreateDeck()
        {
            var deck = new List<BaseCard>(32);
            
            foreach (Card.Suit suit in System.Enum.GetValues(typeof(Card.Suit)))
            {
                foreach (Card.Rank rank in System.Enum.GetValues(typeof(Card.Rank)))
                {
                    deck.Add(Create(suit, rank));
                }
            }
            
            return deck;
        }
        
        /// <summary>
        /// Vytvoří balíček s vlastní konfigurací
        /// Pro budoucí rozšíření (např. různé balíčky)
        /// </summary>
        /// <param name="suits">Seznam barev</param>
        /// <param name="ranks">Seznam hodnot</param>
        /// <returns>Seznam karet</returns>
        public static List<BaseCard> CreateDeck(IEnumerable<Card.Suit> suits, IEnumerable<Card.Rank> ranks)
        {
            var deck = new List<BaseCard>();
            
            foreach (var suit in suits)
            {
                foreach (var rank in ranks)
                {
                    deck.Add(Create(suit, rank));
                }
            }
            
            return deck;
        }
        
        /// <summary>
        /// Zkontroluje, zda je karta speciální (sedma, eso, dáma)
        /// </summary>
        public static bool IsSpecialRank(Card.Rank rank)
        {
            return rank == Card.Rank.Seven 
                || rank == Card.Rank.Ace 
                || rank == Card.Rank.Queen;
        }
    }
}
