namespace Prsi.Core.Cards
{
    /// <summary>
    /// Barva karty. Čistý doménový enum — žádná závislost na UnityEngine,
    /// aby šel testovat v EditMode bez enginu.
    /// </summary>
    public enum Suit { Hearts, Diamonds, Clubs, Spades }

    /// <summary>
    /// Hodnota karty. Explicitní číselné hodnoty (mariášový balíček 7–A).
    /// </summary>
    public enum Rank { Seven = 7, Eight = 8, Nine = 9, Ten = 10, Jack = 11, Queen = 12, King = 13, Ace = 14 }
}
