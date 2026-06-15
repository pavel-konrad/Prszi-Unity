using System.Collections.Generic;
using NUnit.Framework;
using Prsi.Core;
using Prsi.Core.Cards;
using Prsi.Core.Game;

namespace Prsi.Tests.EditMode
{
    /// <summary>
    /// CardRules bridge — evaluating flat ICardData through the polymorphic engine.
    /// Mirrors how the running game (legacy Card : ICardData) will query the rules.
    /// </summary>
    public class CardRulesBridgeTests
    {
        /// <summary>Flat card data, like the legacy UI Card seen as ICardData.</summary>
        private sealed class FlatCard : ICardData
        {
            public FlatCard(Suit s, Rank r) { Suit = s; Rank = r; }
            public Suit Suit { get; }
            public Rank Rank { get; }
        }

        private static GameContext Ctx() => new GameContext(null, null, new List<IPlayerData>());

        [Test] // S2.1 via bridge — same suit playable
        public void Bridge_RegularSameSuit_IsPlayable()
        {
            var top = new FlatCard(Suit.Hearts, Rank.Eight);
            Assert.IsTrue(CardRules.CanPlay(new FlatCard(Suit.Hearts, Rank.Ten), top, Ctx()));
        }

        [Test] // S2.1 via bridge — unrelated card not playable
        public void Bridge_RegularUnrelated_NotPlayable()
        {
            var top = new FlatCard(Suit.Hearts, Rank.Eight);
            Assert.IsFalse(CardRules.CanPlay(new FlatCard(Suit.Spades, Rank.Nine), top, Ctx()));
        }

        [Test] // S3.4 via bridge — the bug fix: Queen plays on anything
        public void Bridge_Queen_IsPlayableOnAnything()
        {
            var top = new FlatCard(Suit.Hearts, Rank.Eight);
            Assert.IsTrue(CardRules.CanPlay(new FlatCard(Suit.Spades, Rank.Queen), top, Ctx()));
        }

        [Test] // S3.2 via bridge — only a Seven defends an active draw penalty
        public void Bridge_DuringPenalty_OnlySevenDefends()
        {
            var ctx = Ctx();
            ctx.NotifyDrawPenalty(2);
            var topSeven = new FlatCard(Suit.Hearts, Rank.Seven);
            Assert.IsTrue(CardRules.CanPlay(new FlatCard(Suit.Spades, Rank.Seven), topSeven, ctx));
            Assert.IsFalse(CardRules.CanPlay(new FlatCard(Suit.Hearts, Rank.Eight), topSeven, ctx));
        }

        [Test] // null card never playable
        public void Bridge_NullCard_NotPlayable()
        {
            Assert.IsFalse(CardRules.CanPlay(null, new FlatCard(Suit.Hearts, Rank.Eight), Ctx()));
        }
    }
}
