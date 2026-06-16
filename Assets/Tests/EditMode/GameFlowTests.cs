using System.Collections.Generic;
using NUnit.Framework;
using Prsi.Core;
using Prsi.Core.Cards;
using Prsi.Core.Game;

namespace Prsi.Tests.EditMode
{
    /// <summary>
    /// SPEC S4 (pořadí tahů) a S5.1 (vítěz) — logika GameContextu.
    /// </summary>
    public class GameFlowTests
    {
        /// <summary>Minimální fake hráče — GameContext potřebuje jen seznam IPlayerData.</summary>
        private sealed class TestPlayer : IPlayerData
        {
            public TestPlayer(int id) { Id = id; }
            public int Id { get; }
            public string Name => $"P{Id}";
            public bool IsHuman => Id == 0;
            public UnityEngine.Sprite Avatar => null;
            public int Cash => 0;
            public int Bet => 0;
            public IReadOnlyList<ICardData> Hand => System.Array.Empty<ICardData>();
            public bool HasWon => false;
            public bool CanPlay => true;
            public bool IsInGame => true;
        }

        private static GameContext CtxWith(int playerCount)
        {
            var players = new List<IPlayerData>();
            for (int i = 0; i < playerCount; i++) players.Add(new TestPlayer(i));
            return new GameContext(players);
        }

        [Test] // S4.1 — normálně posun o 1
        public void Advance_MovesToNextPlayer()
        {
            var ctx = CtxWith(3);
            ctx.CurrentPlayerIndex = 0;
            ctx.AdvanceToNextPlayer();
            Assert.AreEqual(1, ctx.CurrentPlayerIndex);
        }

        [Test] // S4.1 — normální posun wrapuje
        public void Advance_WrapsAroundEnd()
        {
            var ctx = CtxWith(3);
            ctx.CurrentPlayerIndex = 2;
            ctx.AdvanceToNextPlayer();
            Assert.AreEqual(0, ctx.CurrentPlayerIndex); // (2+1)%3
        }

        [Test] // S5.1 — SetWinner nastaví stav a vystřelí událost
        public void SetWinner_SetsStateAndFiresEvent()
        {
            var ctx = CtxWith(2);
            var winner = ctx.Players[1];
            IPlayerData notified = null;
            ctx.OnPlayerWon += p => notified = p;

            ctx.SetWinner(winner);

            Assert.AreSame(winner, ctx.Winner);
            Assert.IsTrue(ctx.IsGameOver);
            Assert.AreSame(winner, notified);
        }

        [Test] // ResetRoundState clears forced suit, draw penalty and ace-pending
        public void ResetRoundState_ClearsTransientEffects()
        {
            var ctx = CtxWith(2);
            ctx.NotifyDrawPenalty(4);
            ctx.NotifySuitChanged(Suit.Clubs);
            ctx.AcePending = true;

            ctx.ResetRoundState();

            Assert.AreEqual(0, ctx.PendingDrawCount);
            Assert.IsNull(ctx.ForcedSuit);
            Assert.IsFalse(ctx.AcePending);
        }
    }
}
