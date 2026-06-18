using System.Collections.Generic;
using Prsi.Core.Cards;

namespace Prsi.Core.Game
{
    /// <summary>
    /// Shared rule state for a round of Prší (forced suit, draw penalty, skip,
    /// winner). The cards/UI live in the running game; this holds only what the
    /// card rules and the turn loop need to reason about.
    /// </summary>
    public class GameContext
    {
        // === Players ===
        private readonly List<IPlayerData> players;
        public IReadOnlyList<IPlayerData> Players => players;
        public int CurrentPlayerIndex { get; set; }
        
        public IPlayerData CurrentPlayer => 
            players.Count > 0 && CurrentPlayerIndex >= 0 && CurrentPlayerIndex < players.Count 
                ? players[CurrentPlayerIndex] 
                : null;
        
        // === Special Card Effects ===

        /// <summary>
        /// Ace effect: an Ace is on top; the next player must counter with their own Ace,
        /// or stands (skips turn) — but does NOT draw. True until someone stands.
        /// </summary>
        public bool AcePending { get; set; }

        /// <summary>
        /// Number of cards to draw (Seven effect, cumulative)
        /// </summary>
        public int PendingDrawCount { get; set; }
        
        /// <summary>
        /// Forced suit (Queen effect)
        /// Null = no forced suit
        /// </summary>
        public Suit? ForcedSuit { get; set; }
        
        // === Game State ===
        public bool IsGameOver { get; set; }
        public IPlayerData Winner { get; set; }
        
        // === Events ===
        public event System.Action<IPlayerData> OnPlayerWon;
        public event System.Action<ICardData, IPlayerData> OnCardPlayed;
        public event System.Action<ICardData, IPlayerData> OnCardDrawn;
        public event System.Action<Suit> OnSuitChanged;
        public event System.Action OnForcedSuitCleared;
        public event System.Action<int> OnDrawPenalty;
        
        public GameContext(List<IPlayerData> players)
        {
            this.players = players ?? new List<IPlayerData>();
            CurrentPlayerIndex = 0;
        }
        
        // === Helper Methods ===
        
        /// <summary>
        /// Advances to the next player in order.
        /// </summary>
        public void AdvanceToNextPlayer()
        {
            CurrentPlayerIndex = (CurrentPlayerIndex + 1) % players.Count;
        }
        
        /// <summary>
        /// Raises suit-changed event
        /// </summary>
        public void NotifySuitChanged(Suit suit)
        {
            ForcedSuit = suit;
            OnSuitChanged?.Invoke(suit);
        }
        
        /// <summary>
        /// Raises draw-penalty event
        /// </summary>
        public void NotifyDrawPenalty(int count)
        {
            PendingDrawCount += count;
            OnDrawPenalty?.Invoke(PendingDrawCount);
        }
        
        /// <summary>
        /// Raises card-played event
        /// </summary>
        public void NotifyCardPlayed(ICardData card, IPlayerData player)
        {
            OnCardPlayed?.Invoke(card, player);
        }
        
        /// <summary>
        /// Raises card-drawn event
        /// </summary>
        public void NotifyCardDrawn(ICardData card, IPlayerData player)
        {
            OnCardDrawn?.Invoke(card, player);
        }
        
        /// <summary>
        /// Sets the winner and ends the game
        /// </summary>
        public void SetWinner(IPlayerData player)
        {
            Winner = player;
            IsGameOver = true;
            OnPlayerWon?.Invoke(player);
        }
        
        /// <summary>
        /// Resets card effects (forced suit)
        /// Called after a regular card is played
        /// </summary>
        public void ClearForcedSuit()
        {
            if (ForcedSuit == null) return;
            ForcedSuit = null;
            OnForcedSuitCleared?.Invoke();
        }
        
        /// <summary>
        /// Resets draw penalty
        /// Called after drawing cards or playing a Seven
        /// </summary>
        public void ClearDrawPenalty()
        {
            PendingDrawCount = 0;
        }

        /// <summary>
        /// Clears all transient round state (forced suit, draw penalty, skip).
        /// Call when a fresh hand is dealt so effects don't leak across rounds.
        /// </summary>
        public void ResetRoundState()
        {
            PendingDrawCount = 0;
            AcePending = false;
            ClearForcedSuit();
        }
    }
}
