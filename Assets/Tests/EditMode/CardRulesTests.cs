using System.Collections.Generic;
using NUnit.Framework;
using Prsi.Core;
using Prsi.Core.Cards;
using Prsi.Core.Game;

namespace Prsi.Tests.EditMode
{
    /// <summary>
    /// Pattern #1 — Strategy / card polymorphism.
    /// Each test maps to a clause in SPEC.md. Asserts exact values.
    /// Domain is pure (Prsi.Core), so it runs in EditMode without a scene/engine.
    /// </summary>
    public class CardRulesTests
    {
        // Shared game context without deck/discard — card rules do not need them.
        private static GameContext Ctx() => new GameContext(new List<IPlayerData>());

        private static BaseCard Card(Suit s, Rank r) => CardFactory.Create(s, r);

        // === S2 — basic playability ===

        [Test] // S2.1 — same suit
        public void Regular_SameSuit_IsPlayable()
        {
            var top = Card(Suit.Hearts, Rank.Eight);
            Assert.IsTrue(Card(Suit.Hearts, Rank.Ten).CanPlayOn(top, Ctx()));
        }

        [Test] // S2.1 — same rank
        public void Regular_SameRank_IsPlayable()
        {
            var top = Card(Suit.Hearts, Rank.Eight);
            Assert.IsTrue(Card(Suit.Spades, Rank.Eight).CanPlayOn(top, Ctx()));
        }

        [Test] // S2.1 — different suit and rank → not playable
        public void Regular_DifferentSuitAndRank_NotPlayable()
        {
            var top = Card(Suit.Hearts, Rank.Eight);
            Assert.IsFalse(Card(Suit.Spades, Rank.Nine).CanPlayOn(top, Ctx()));
        }

        [Test] // S2.2 — first card (empty discard) → any non-penalty card is playable
        public void Regular_OnEmptyDiscard_IsPlayable()
        {
            Assert.IsTrue(Card(Suit.Hearts, Rank.Eight).CanPlayOn(null, Ctx()));
        }

        // === S3 — special cards ===

        [Test] // S3.1 — Seven adds +2, cumulatively
        public void Seven_OnPlay_AccumulatesDrawPenalty()
        {
            var ctx = Ctx();
            Card(Suit.Hearts, Rank.Seven).OnPlay(ctx, null);
            Assert.AreEqual(2, ctx.PendingDrawCount);
            Card(Suit.Spades, Rank.Seven).OnPlay(ctx, null);
            Assert.AreEqual(4, ctx.PendingDrawCount);
        }

        [Test] // S3.2 — during a penalty only a Seven can defend (on a Seven)
        public void DuringPenalty_OnlySeven_CanDefendOnSeven()
        {
            var ctx = Ctx();
            var topSeven = Card(Suit.Hearts, Rank.Seven);
            topSeven.OnPlay(ctx, null); // PendingDrawCount = 2

            Assert.IsTrue(Card(Suit.Diamonds, Rank.Seven).CanPlayOn(topSeven, ctx), "a Seven defends");
            Assert.IsFalse(Card(Suit.Hearts, Rank.Eight).CanPlayOn(topSeven, ctx), "a regular card cannot");
        }

        [Test] // S3.3 — Ace sets AcePending (next must beat it or stand)
        public void Ace_OnPlay_SetsAcePending()
        {
            var ctx = Ctx();
            Card(Suit.Spades, Rank.Ace).OnPlay(ctx, null);
            Assert.IsTrue(ctx.AcePending);
        }

        [Test] // S3.3 — Ace cannot be played during a penalty (Seven)
        public void Ace_DuringPenalty_NotPlayable()
        {
            var ctx = Ctx();
            ctx.NotifyDrawPenalty(2);
            var top = Card(Suit.Spades, Rank.Seven);
            Assert.IsFalse(Card(Suit.Spades, Rank.Ace).CanPlayOn(top, ctx));
        }

        [Test] // S3.9 — a pending Ace can only be beaten by an Ace (no other card)
        public void AcePending_OnlyAce_CanRespond()
        {
            var ctx = Ctx();
            ctx.AcePending = true;
            var top = Card(Suit.Spades, Rank.Ace);
            Assert.IsTrue(Card(Suit.Hearts, Rank.Ace).CanPlayOn(top, ctx), "Ace beats Ace");
            Assert.IsFalse(Card(Suit.Spades, Rank.Eight).CanPlayOn(top, ctx), "a regular card cannot");
            Assert.IsFalse(Card(Suit.Spades, Rank.Seven).CanPlayOn(top, ctx), "a Seven cannot");
            Assert.IsFalse(Card(Suit.Spades, Rank.Queen).CanPlayOn(top, ctx), "a Queen cannot");
        }

        [Test] // S3.4 — Queen on anything
        public void Queen_IsPlayable_OnAnything()
        {
            var top = Card(Suit.Hearts, Rank.Eight);
            Assert.IsTrue(Card(Suit.Spades, Rank.Queen).CanPlayOn(top, Ctx()));
        }

        [Test] // S3.5 — Queen OnPlay sets the forced suit to the selected one
        public void Queen_OnPlay_SetsForcedSuitToSelected()
        {
            var ctx = Ctx();
            var queen = (QueenCard)Card(Suit.Hearts, Rank.Queen);
            queen.SelectSuit(Suit.Clubs);
            queen.OnPlay(ctx, null);
            Assert.AreEqual(Suit.Clubs, ctx.ForcedSuit.Value);
        }

        [Test] // S3.6 — Queen cannot be played during a penalty
        public void Queen_DuringPenalty_NotPlayable()
        {
            var ctx = Ctx();
            ctx.NotifyDrawPenalty(2);
            var top = Card(Suit.Spades, Rank.Seven);
            Assert.IsFalse(Card(Suit.Spades, Rank.Queen).CanPlayOn(top, ctx));
        }

        [Test] // S3.7 — forced suit restricts a regular card
        public void ForcedSuit_RestrictsRegularCard()
        {
            var ctx = Ctx();
            ctx.NotifySuitChanged(Suit.Clubs);
            var top = Card(Suit.Clubs, Rank.Ten); // with a forced suit the top card doesn't matter
            Assert.IsFalse(Card(Suit.Hearts, Rank.Eight).CanPlayOn(top, ctx), "different suit cannot");
            Assert.IsTrue(Card(Suit.Clubs, Rank.Nine).CanPlayOn(top, ctx), "matching suit can");
        }

        [Test] // S3.8 — a regular card clears the forced suit
        public void Regular_OnPlay_ClearsForcedSuit()
        {
            var ctx = Ctx();
            ctx.NotifySuitChanged(Suit.Clubs);
            Card(Suit.Clubs, Rank.Nine).OnPlay(ctx, null);
            Assert.IsNull(ctx.ForcedSuit);
        }
    }
}
