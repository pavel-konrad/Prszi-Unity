using System;

namespace Prsi.Core
{
    /// <summary>
    /// Umístění karty ve hře
    /// </summary>
    public enum CardLocation
    {
        Deck,           // V balíčku
        PlayerHand,     // V ruce hráče
        EnemyHand,      // V ruce nepřítele
        Discard,        // V odhazovacím balíčku
        Drawn,          // Právě líznutá (přechodný stav)
        Playing         // Právě hraná (přechodný stav)
    }

    /// <summary>
    /// Interface pro sledování stavu karty
    /// </summary>
    public interface ICardState
    {
        ICardData Card { get; }
        CardLocation Location { get; }
        int? OwnerId { get; } // ID hráče, který má kartu (null pokud v decku/discard)
        event Action<ICardState, CardLocation> LocationChanged;
    }
}

