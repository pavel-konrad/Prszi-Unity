using Prsi.Core.Game;

namespace Prsi.Core.Cards
{
    /// <summary>
    /// Abstract base class for all cards
    /// Equivalent of JS base_card.js from the prszi project
    /// 
    /// Implements polymorphism — each card type has its own behaviour:
    /// - CanPlayOn() — whether the card may be played on the top card
    /// - OnPlay() — what happens after the card is played
    /// </summary>
    public abstract class BaseCard : ICardData
    {
        public Suit Suit { get; }
        public Rank Rank { get; }
        
        /// <summary>
        /// Whether the card is special (has a special effect)
        /// Override in subclasses for SevenCard, AceCard, QueenCard
        /// </summary>
        public virtual bool IsSpecial => false;
        
        protected BaseCard(Suit suit, Rank rank)
        {
            Suit = suit;
            Rank = rank;
        }
        
        /// <summary>
        /// Checks whether this card may be played on the top card
        /// 
        /// Basic Prší rules:
        /// - Same suit
        /// - Same rank
        /// - Special cards may have their own rules (e.g. Queen on anything)
        /// </summary>
        /// <param name="topCard">Top card on the discard pile</param>
        /// <param name="context">Game context (for forced-suit checks, etc.)</param>
        /// <returns>True if the card may be played</returns>
        public abstract bool CanPlayOn(ICardData topCard, GameContext context);
        
        /// <summary>
        /// Applies the card effect after it is played
        /// 
        /// Polymorphic method — each card type has its own implementation:
        /// - RegularCard: no special effect
        /// - SevenCard: next player must draw 2 cards
        /// - AceCard: skips the next player
        /// - QueenCard: player picks a new suit
        /// </summary>
        /// <param name="context">Game context for modifying game state</param>
        /// <param name="player">Player who played the card</param>
        public abstract void OnPlay(GameContext context, IPlayerData player);
        
        /// <summary>
        /// Basic playability check — same suit or rank
        /// Helper method for subclasses
        /// </summary>
        protected bool MatchesSuitOrRank(ICardData topCard)
        {
            if (topCard == null) return true; // First card
            return Suit == topCard.Suit || Rank == topCard.Rank;
        }
        
        /// <summary>
        /// Forced-suit check (after a Queen is played)
        /// </summary>
        protected bool MatchesForcedSuit(GameContext context)
        {
            if (context?.ForcedSuit == null) return true;
            return Suit == context.ForcedSuit.Value;
        }
        
        public override string ToString()
        {
            return $"{Rank} of {Suit}";
        }
        
        public override bool Equals(object obj)
        {
            if (obj is BaseCard other)
            {
                return Suit == other.Suit && Rank == other.Rank;
            }
            if (obj is ICardData cardData)
            {
                return Suit == cardData.Suit && Rank == cardData.Rank;
            }
            return false;
        }
        
        public override int GetHashCode()
        {
            return (int)Suit * 100 + (int)Rank;
        }
    }
}
