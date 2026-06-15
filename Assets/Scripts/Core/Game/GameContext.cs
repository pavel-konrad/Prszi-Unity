using System.Collections.Generic;
using Prsi.Core.Cards;

namespace Prsi.Core.Game
{
    /// <summary>
    /// Sdílený herní kontext - ekvivalent JS game objektu
    /// Obsahuje stav hry, který karty a hráči potřebují pro své akce
    /// </summary>
    public class GameContext
    {
        // === Deck & Discard ===
        public ICardDeckProvider Deck { get; }
        public IDiscardPile DiscardPile { get; }
        
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
        /// Přeskočit dalšího hráče (efekt esa)
        /// </summary>
        public bool SkipNextPlayer { get; set; }
        
        /// <summary>
        /// Počet karet k líznutí (efekt sedmy, kumulativní)
        /// </summary>
        public int PendingDrawCount { get; set; }
        
        /// <summary>
        /// Vynucená barva (efekt dámy)
        /// Null = žádná vynucená barva
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
        public event System.Action OnPlayerSkipped;
        
        public GameContext(ICardDeckProvider deck, IDiscardPile discardPile, List<IPlayerData> players)
        {
            Deck = deck;
            DiscardPile = discardPile;
            this.players = players ?? new List<IPlayerData>();
            CurrentPlayerIndex = 0;
        }
        
        // === Helper Methods ===
        
        /// <summary>
        /// Posune na dalšího hráče
        /// </summary>
        public void AdvanceToNextPlayer()
        {
            if (SkipNextPlayer)
            {
                SkipNextPlayer = false;
                CurrentPlayerIndex = (CurrentPlayerIndex + 2) % players.Count;
                OnPlayerSkipped?.Invoke();
            }
            else
            {
                CurrentPlayerIndex = (CurrentPlayerIndex + 1) % players.Count;
            }
        }
        
        /// <summary>
        /// Vyvolá událost změny barvy
        /// </summary>
        public void NotifySuitChanged(Suit suit)
        {
            ForcedSuit = suit;
            OnSuitChanged?.Invoke(suit);
        }
        
        /// <summary>
        /// Vyvolá událost penalizace líznutí
        /// </summary>
        public void NotifyDrawPenalty(int count)
        {
            PendingDrawCount += count;
            OnDrawPenalty?.Invoke(PendingDrawCount);
        }
        
        /// <summary>
        /// Vyvolá událost odehrání karty
        /// </summary>
        public void NotifyCardPlayed(ICardData card, IPlayerData player)
        {
            OnCardPlayed?.Invoke(card, player);
        }
        
        /// <summary>
        /// Vyvolá událost líznutí karty
        /// </summary>
        public void NotifyCardDrawn(ICardData card, IPlayerData player)
        {
            OnCardDrawn?.Invoke(card, player);
        }
        
        /// <summary>
        /// Nastaví vítěze a ukončí hru
        /// </summary>
        public void SetWinner(IPlayerData player)
        {
            Winner = player;
            IsGameOver = true;
            OnPlayerWon?.Invoke(player);
        }
        
        /// <summary>
        /// Resetuje efekty karet (vynucenou barvu)
        /// Volá se po odehrání běžné karty
        /// </summary>
        public void ClearForcedSuit()
        {
            if (ForcedSuit == null) return;
            ForcedSuit = null;
            OnForcedSuitCleared?.Invoke();
        }
        
        /// <summary>
        /// Resetuje penalizaci líznutí
        /// Volá se po líznutí karet nebo odehrání sedmy
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
            SkipNextPlayer = false;
            ClearForcedSuit();
        }
    }
}
