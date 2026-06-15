using System;

namespace Prsi.Core
{
    /// <summary>
    /// Implementace ICardState - sleduje stav karty
    /// </summary>
    public class CardState : ICardState
    {
        public ICardData Card { get; }
        public CardLocation Location { get; private set; }
        public int? OwnerId { get; private set; }
        
        public event Action<ICardState, CardLocation> LocationChanged;
        
        public CardState(ICardData card, CardLocation initialLocation = CardLocation.Deck, int? ownerId = null)
        {
            Card = card ?? throw new ArgumentNullException(nameof(card));
            Location = initialLocation;
            OwnerId = ownerId;
        }
        
        /// <summary>
        /// Změní umístění karty (voláno z CardStateService).
        /// Public kvůli hranici assembly — CardStateService žije mimo Prsi.Core.
        /// </summary>
        public void SetLocation(CardLocation newLocation, int? newOwnerId = null)
        {
            if (Location == newLocation && OwnerId == newOwnerId)
                return;
            
            var previousLocation = Location;
            Location = newLocation;
            OwnerId = newOwnerId;
            
            LocationChanged?.Invoke(this, previousLocation);
        }
    }
}

