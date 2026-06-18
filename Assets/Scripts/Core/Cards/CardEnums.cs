namespace Prsi.Core.Cards
{
    /// <summary>
    /// Card suit. Pure domain enum — no UnityEngine dependency,
    /// so it can be tested in EditMode without the engine.
    /// </summary>
    public enum Suit { Hearts, Diamonds, Clubs, Spades }

    /// <summary>
    /// Card rank. Explicit numeric values (mariáš deck 7–A).
    /// </summary>
    public enum Rank { Seven = 7, Eight = 8, Nine = 9, Ten = 10, Jack = 11, Queen = 12, King = 13, Ace = 14 }
}
