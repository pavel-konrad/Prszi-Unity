using System.Collections.Generic;
using Prsi.Core.Cards;

namespace Prsi.Core.Game
{
    /// <summary>
    /// Pure economy + elimination rules for the tournament. No UnityEngine, so it
    /// runs in EditMode. Losers pay baseRate × the value of the cards left in hand;
    /// the round winner collects what was actually paid; a player at 0 cash is out;
    /// the last player with money wins the tournament.
    /// </summary>
    public static class TournamentRules
    {
        /// <summary>Penalty for the cards left in a hand: baseRate × Σ card value (Rank as int).</summary>
        public static int PenaltyFor(IReadOnlyList<ICardData> hand, int baseRate)
        {
            if (hand == null) return 0;

            int sum = 0;
            foreach (var card in hand) sum += (int)card.Rank;
            return sum * baseRate;
        }

        /// <summary>
        /// Settles a finished round: every non-winner pays min(penalty, cash); the
        /// winner receives the total actually paid. Returns the total collected.
        /// </summary>
        public static int SettleRound(IReadOnlyList<ITournamentPlayer> players, ITournamentPlayer winner, int baseRate)
        {
            if (players == null) return 0;

            int collected = 0;
            foreach (var p in players)
            {
                if (p == null || p == winner) continue;

                int penalty = PenaltyFor(p.Hand, baseRate);
                int paid = penalty < p.Cash ? penalty : p.Cash; // clamp to what they have
                if (paid > 0) p.Pay(paid);
                collected += paid;
            }

            winner?.Receive(collected);
            return collected;
        }

        /// <summary>A player is out once they hit 0 cash.</summary>
        public static bool IsEliminated(ITournamentPlayer player) => player != null && player.Cash == 0;

        /// <summary>Players still in the tournament (cash &gt; 0).</summary>
        public static List<ITournamentPlayer> RemainingPlayers(IReadOnlyList<ITournamentPlayer> players)
        {
            var remaining = new List<ITournamentPlayer>();
            if (players == null) return remaining;
            foreach (var p in players)
                if (p != null && p.Cash > 0) remaining.Add(p);
            return remaining;
        }

        /// <summary>The overall winner once exactly one player has money left; otherwise null.</summary>
        public static ITournamentPlayer OverallWinner(IReadOnlyList<ITournamentPlayer> players)
        {
            var remaining = RemainingPlayers(players);
            return remaining.Count == 1 ? remaining[0] : null;
        }
    }
}
