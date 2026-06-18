using Prsi.Core.Game;

namespace Prsi.Core.Cards
{
    /// <summary>
    /// Ace — special card with a skip effect
    /// 
    /// Prší rules:
    /// - Next player is skipped (stands)
    /// - May be played on a card of the same suit or rank
    /// </summary>
    public class AceCard : BaseCard
    {
        public override bool IsSpecial => true;
        
        public AceCard(Suit suit, Rank rank) : base(suit, rank)
        {
        }
        
        /// <summary>
        /// Ace may be played if:
        /// 1. No draw penalty is active (Sevens) — Ace cannot be used to defend
        /// 2. Standard rule: same suit/rank
        /// </summary>
        public override bool CanPlayOn(ICardData topCard, GameContext context)
        {
            // Defence against Ace: a pending Ace may only be countered with another Ace.
            if (context?.AcePending == true)
            {
                return true; // this card is an Ace
            }

            // If a draw penalty is active (Seven), Ace cannot be played
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
        /// Ace effect: next player must counter with an Ace, or stands (skipped, does not draw).
        /// </summary>
        public override void OnPlay(GameContext context, IPlayerData player)
        {
            // Clear forced suit
            context?.ClearForcedSuit();

            // Ace waits for the next player's response
            if (context != null)
            {
                context.AcePending = true;
            }

            // Notify that the card was played
            context?.NotifyCardPlayed(this, player);
        }
    }
}
