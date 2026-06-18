using Prsi.Core.Game;

namespace Prsi.Core.Cards
{
    /// <summary>
    /// Regular card with no special effect
    /// Ranks: 8, 9, 10, J (Jack), K (King)
    /// 
    /// Rules:
    /// - May be played on a card of the same suit or rank
    /// - Respects forced suit after a Queen
    /// - Has no special effect
    /// </summary>
    public class RegularCard : BaseCard
    {
        public override bool IsSpecial => false;
        
        public RegularCard(Suit suit, Rank rank) : base(suit, rank)
        {
        }
        
        /// <summary>
        /// Regular card may be played if:
        /// 1. Suit or rank matches the top card
        /// 2. No forced suit is active, or suit matches the forced suit
        /// 3. If a draw penalty is active (Sevens), regular cards cannot be played
        /// </summary>
        public override bool CanPlayOn(ICardData topCard, GameContext context)
        {
            // Only an Ace may respond to a pending Ace.
            if (context?.AcePending == true)
            {
                return false;
            }

            // If a draw penalty is active, regular cards cannot be played
            if (context?.PendingDrawCount > 0)
            {
                return false;
            }

            // If a suit is forced (after a Queen), suit must match
            if (context?.ForcedSuit != null)
            {
                return Suit == context.ForcedSuit.Value;
            }

            // Standard rule: same suit or rank
            return MatchesSuitOrRank(topCard);
        }
        
        /// <summary>
        /// Regular card has no special effect
        /// Only clears forced suit (if it was active)
        /// </summary>
        public override void OnPlay(GameContext context, IPlayerData player)
        {
            // Clear forced suit after Queen
            context?.ClearForcedSuit();
            
            // Notify that the card was played
            context?.NotifyCardPlayed(this, player);
        }
    }
}
