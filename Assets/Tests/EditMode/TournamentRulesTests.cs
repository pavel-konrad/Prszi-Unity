using System.Collections.Generic;
using NUnit.Framework;
using Prsi.Core;
using Prsi.Core.Cards;
using Prsi.Core.Game;

namespace Prsi.Tests.EditMode
{
    /// <summary>SPEC T1–T3 — tournament economy and elimination (pure Core).</summary>
    public class TournamentRulesTests
    {
        private sealed class FakeCard : ICardData
        {
            public FakeCard(Rank r) { Rank = r; Suit = Suit.Hearts; }
            public Suit Suit { get; }
            public Rank Rank { get; }
        }

        private sealed class FakePlayer : ITournamentPlayer
        {
            public FakePlayer(string name, int cash, params Rank[] hand)
            {
                Name = name; Cash = cash;
                var list = new List<ICardData>();
                foreach (var r in hand) list.Add(new FakeCard(r));
                Hand = list;
            }
            public string Name { get; }
            public bool IsHuman => false;
            public int Cash { get; private set; }
            public IReadOnlyList<ICardData> Hand { get; }
            public void Pay(int amount) => Cash = System.Math.Max(0, Cash - amount);
            public void Receive(int amount) => Cash += amount;
        }

        // === T1 — penalty ===

        [Test] // T1.1
        public void PenaltyFor_EmptyHand_IsZero()
        {
            Assert.AreEqual(0, TournamentRules.PenaltyFor(new List<ICardData>(), 10));
        }

        [Test] // T1.2 — {Ace=14, Seven=7} × 10 = 210
        public void PenaltyFor_AceAndSeven_At10_Is210()
        {
            var hand = new List<ICardData> { new FakeCard(Rank.Ace), new FakeCard(Rank.Seven) };
            Assert.AreEqual(210, TournamentRules.PenaltyFor(hand, 10));
        }

        // === T2 — settle ===

        [Test] // T2.1 — loser cash drops by exactly its penalty
        public void SettleRound_LoserPaysExactPenalty()
        {
            var winner = new FakePlayer("W", 1000);                       // 0 cards
            var loser = new FakePlayer("L", 1000, Rank.Eight, Rank.Nine); // (8+9)*10 = 170
            TournamentRules.SettleRound(new List<ITournamentPlayer> { winner, loser }, winner, 10);
            Assert.AreEqual(1000 - 170, loser.Cash);
        }

        [Test] // T2.2 — winner receives the sum of penalties
        public void SettleRound_WinnerReceivesSum()
        {
            var winner = new FakePlayer("W", 1000);
            var l1 = new FakePlayer("L1", 1000, Rank.Seven);  // 70
            var l2 = new FakePlayer("L2", 1000, Rank.King);   // 130
            int collected = TournamentRules.SettleRound(new List<ITournamentPlayer> { winner, l1, l2 }, winner, 10);
            Assert.AreEqual(200, collected);
            Assert.AreEqual(1200, winner.Cash);
        }

        [Test] // T2.3 — insufficient cash: pays only what it has, winner gets only that
        public void SettleRound_InsufficientCash_PaysOnlyCash()
        {
            var winner = new FakePlayer("W", 0);
            var broke = new FakePlayer("B", 50, Rank.Ace); // penalty 140 > 50
            int collected = TournamentRules.SettleRound(new List<ITournamentPlayer> { winner, broke }, winner, 10);
            Assert.AreEqual(0, broke.Cash);
            Assert.AreEqual(50, collected);
            Assert.AreEqual(50, winner.Cash);
        }

        // === T3 — elimination / winner ===

        [Test] // T3.1
        public void IsEliminated_TrueAtZeroCash()
        {
            Assert.IsTrue(TournamentRules.IsEliminated(new FakePlayer("X", 0)));
            Assert.IsFalse(TournamentRules.IsEliminated(new FakePlayer("Y", 1)));
        }

        [Test] // T3.2
        public void OverallWinner_OnlyWhenExactlyOneHasMoney()
        {
            var a = new FakePlayer("A", 100);
            var b = new FakePlayer("B", 0);
            var c = new FakePlayer("C", 0);
            Assert.AreSame(a, TournamentRules.OverallWinner(new List<ITournamentPlayer> { a, b, c }));

            var d = new FakePlayer("D", 100);
            Assert.IsNull(TournamentRules.OverallWinner(new List<ITournamentPlayer> { a, d })); // two left
            Assert.IsNull(TournamentRules.OverallWinner(new List<ITournamentPlayer> { b, c })); // none left
        }

        [Test] // T3.3
        public void RemainingPlayers_ExcludesEliminated()
        {
            var a = new FakePlayer("A", 100);
            var b = new FakePlayer("B", 0);
            var remaining = TournamentRules.RemainingPlayers(new List<ITournamentPlayer> { a, b });
            Assert.AreEqual(1, remaining.Count);
            Assert.AreSame(a, remaining[0]);
        }
    }
}
