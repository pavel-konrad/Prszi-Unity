using Prsi.Core.Game;

namespace Prsi.Core.Cards
{
    /// <summary>
    /// Queen — special card that changes suit
    /// 
    /// Prší rules:
    /// - May be played on ANYTHING (suit and rank do not matter)
    /// - Player picks a new suit that the next player must follow
    /// </summary>
    public class QueenCard : BaseCard
    {
        public override bool IsSpecial => true;
        
        /// <summary>
        /// Selected suit after a Queen is played
        /// Set before calling OnPlay()
        /// </summary>
        public Suit? SelectedSuit { get; set; }
        
        public QueenCard(Suit suit, Rank rank) : base(suit, rank)
        {
        }
        
        /// <summary>
        /// Queen may ALWAYS be played (on anything)
        /// Exception: cannot be played while a draw penalty is active (Sevens)
        /// </summary>
        public override bool CanPlayOn(ICardData topCard, GameContext context)
        {
            // Only an Ace may respond to a pending Ace.
            if (context?.AcePending == true)
            {
                return false;
            }

            // If a draw penalty is active, Queen cannot be played
            if (context?.PendingDrawCount > 0)
            {
                return false;
            }

            // Queen may be played on anything!
            return true;
        }
        
        /// <summary>
        /// Queen effect: sets forced suit
        /// 
        /// NOTE: SelectedSuit must be set before calling this method
        /// (typically via a UI dialog to pick the suit)
        /// 
        /// If SelectedSuit is not set, the Queen's suit is used
        /// </summary>
        public override void OnPlay(GameContext context, IPlayerData player)
        {
            // Determine new suit — either the selected one or the Queen's suit
            Suit newSuit = SelectedSuit ?? Suit;
            
            // Set forced suit
            context?.NotifySuitChanged(newSuit);
            
            // Notify that the card was played
            context?.NotifyCardPlayed(this, player);
            
            // Reset selected suit for next use
            SelectedSuit = null;
        }
        
        /// <summary>
        /// Sets the selected suit before playing
        /// Called from UI before OnPlay()
        /// </summary>
        public void SelectSuit(Suit suit)
        {
            SelectedSuit = suit;
        }
    }
}
