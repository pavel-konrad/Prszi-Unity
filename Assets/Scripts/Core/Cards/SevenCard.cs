using Prsi.Core.Game;

namespace Prsi.Core.Cards
{
    /// <summary>
    /// Seven — special card with a draw effect
    /// 
    /// Prší rules:
    /// - Next player must draw 2 cards
    /// - May be defended with another Seven (effect stacks)
    /// - If a penalty is active, only another Seven may be played
    /// </summary>
    public class SevenCard : BaseCard
    {
        /// <summary>
        /// How many cards the next player must draw
        /// </summary>
        public const int DRAW_PENALTY = 2;
        
        public override bool IsSpecial => true;
        
        public SevenCard(Suit suit, Rank rank) : base(suit, rank)
        {
        }
        
        /// <summary>
        /// Seven may be played if:
        /// 1. A penalty is active (Seven defence) — only a Seven can defend
        /// 2. Or standard rule: same suit/rank with no forced suit
        /// </summary>
        public override bool CanPlayOn(ICardData topCard, GameContext context)
        {
            // Only an Ace may respond to a pending Ace.
            if (context?.AcePending == true)
            {
                return false;
            }

            // If a penalty is active, only a Seven may be played (defence)
            if (context?.PendingDrawCount > 0)
            {
                // Seven defence — may be played on a Seven
                return topCard?.Rank == Rank.Seven;
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
        /// Seven effect: adds a draw penalty for the next player
        /// Penalizace se kumuluje (2 + 2 + 2...)
        /// </summary>
        public override void OnPlay(GameContext context, IPlayerData player)
        {
            // Clear forced suit
            context?.ClearForcedSuit();
            
            // Add draw penalty
            context?.NotifyDrawPenalty(DRAW_PENALTY);
            
            // Notify that the card was played
            context?.NotifyCardPlayed(this, player);
        }
    }
}
