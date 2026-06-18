using System.Collections.Generic;

namespace Prsi.Core.Cards
{
    /// <summary>
    /// Factory for creating cards of the correct type
    /// Equivalent of JS card_factory.js from the prszi project
    /// 
    /// Centralises card-creation logic:
    /// - Sevens (7) → SevenCard
    /// - Aces (A) → AceCard
    /// - Queens (Q) → QueenCard
    /// - Everything else → RegularCard
    /// </summary>
    public static class CardFactory
    {
        /// <summary>
        /// Creates a card of the correct type based on rank
        /// </summary>
        /// <param name="suit">Card suit</param>
        /// <param name="rank">Card rank</param>
        /// <returns>Instance of the correct card type</returns>
        public static BaseCard Create(Suit suit, Rank rank)
        {
            return rank switch
            {
                Rank.Seven => new SevenCard(suit, rank),
                Rank.Ace => new AceCard(suit, rank),
                Rank.Queen => new QueenCard(suit, rank),
                _ => new RegularCard(suit, rank)
            };
        }
        
        /// <summary>
        /// Creates a full 32-card deck (German mariáš)
        /// </summary>
        /// <returns>List of all 32 cards</returns>
        public static List<BaseCard> CreateDeck()
        {
            var deck = new List<BaseCard>(32);
            
            foreach (Suit suit in System.Enum.GetValues(typeof(Suit)))
            {
                foreach (Rank rank in System.Enum.GetValues(typeof(Rank)))
                {
                    deck.Add(Create(suit, rank));
                }
            }
            
            return deck;
        }
        
        /// <summary>
        /// Creates a deck with a custom configuration
        /// For future extensions (e.g. different decks)
        /// </summary>
        /// <param name="suits">List of suits</param>
        /// <param name="ranks">List of ranks</param>
        /// <returns>List of cards</returns>
        public static List<BaseCard> CreateDeck(IEnumerable<Suit> suits, IEnumerable<Rank> ranks)
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
        /// Checks whether the card is special (Seven, Ace, Queen)
        /// </summary>
        public static bool IsSpecialRank(Rank rank)
        {
            return rank == Rank.Seven 
                || rank == Rank.Ace 
                || rank == Rank.Queen;
        }
    }
}
