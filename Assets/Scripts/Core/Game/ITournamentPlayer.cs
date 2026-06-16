using System.Collections.Generic;

namespace Prsi.Core.Game
{
    /// <summary>
    /// Player view the tournament economy reads and writes: money in/out and the
    /// hand it still holds. Implemented by the running game's Player; faked in tests.
    /// Keeps TournamentRules pure (no UnityEngine, no concrete Player).
    /// </summary>
    public interface ITournamentPlayer
    {
        string Name { get; }
        bool IsHuman { get; }
        int Cash { get; }
        IReadOnlyList<ICardData> Hand { get; }

        /// <summary>Takes money from the player (clamped at 0 by the implementation).</summary>
        void Pay(int amount);

        /// <summary>Gives money to the player.</summary>
        void Receive(int amount);
    }
}
