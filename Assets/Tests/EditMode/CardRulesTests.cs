using System.Collections.Generic;
using NUnit.Framework;
using Prsi.Core;
using Prsi.Core.Cards;
using Prsi.Core.Game;

namespace Prsi.Tests.EditMode
{
    /// <summary>
    /// Pattern #1 — Strategy / polymorfismus karet.
    /// Každý test mapuje na klauzuli ze SPEC.md. Asserty na exaktní hodnoty.
    /// Doména je čistá (Prsi.Core), takže běží v EditMode bez scény/enginu.
    /// </summary>
    public class CardRulesTests
    {
        // Sdílený herní kontext bez balíčku/discardu — pravidla karet je nepotřebují.
        private static GameContext Ctx() => new GameContext(null, null, new List<IPlayerData>());

        private static BaseCard Card(Suit s, Rank r) => CardFactory.Create(s, r);

        // === S2 — základ hratelnosti ===

        [Test] // S2.1 — stejná barva
        public void Regular_SameSuit_IsPlayable()
        {
            var top = Card(Suit.Hearts, Rank.Eight);
            Assert.IsTrue(Card(Suit.Hearts, Rank.Ten).CanPlayOn(top, Ctx()));
        }

        [Test] // S2.1 — stejná hodnota
        public void Regular_SameRank_IsPlayable()
        {
            var top = Card(Suit.Hearts, Rank.Eight);
            Assert.IsTrue(Card(Suit.Spades, Rank.Eight).CanPlayOn(top, Ctx()));
        }

        [Test] // S2.1 — jiná barva i hodnota → nehratelné
        public void Regular_DifferentSuitAndRank_NotPlayable()
        {
            var top = Card(Suit.Hearts, Rank.Eight);
            Assert.IsFalse(Card(Suit.Spades, Rank.Nine).CanPlayOn(top, Ctx()));
        }

        [Test] // S2.2 — první karta (prázdný discard) → hratelné cokoliv ne-penalizačního
        public void Regular_OnEmptyDiscard_IsPlayable()
        {
            Assert.IsTrue(Card(Suit.Hearts, Rank.Eight).CanPlayOn(null, Ctx()));
        }

        // === S3 — speciální karty ===

        [Test] // S3.1 — sedma přidá +2, kumulativně
        public void Seven_OnPlay_AccumulatesDrawPenalty()
        {
            var ctx = Ctx();
            Card(Suit.Hearts, Rank.Seven).OnPlay(ctx, null);
            Assert.AreEqual(2, ctx.PendingDrawCount);
            Card(Suit.Spades, Rank.Seven).OnPlay(ctx, null);
            Assert.AreEqual(4, ctx.PendingDrawCount);
        }

        [Test] // S3.2 — při penalizaci se brání jen sedma (na sedmu)
        public void DuringPenalty_OnlySeven_CanDefendOnSeven()
        {
            var ctx = Ctx();
            var topSeven = Card(Suit.Hearts, Rank.Seven);
            topSeven.OnPlay(ctx, null); // PendingDrawCount = 2

            Assert.IsTrue(Card(Suit.Diamonds, Rank.Seven).CanPlayOn(topSeven, ctx), "sedma se brání");
            Assert.IsFalse(Card(Suit.Hearts, Rank.Eight).CanPlayOn(topSeven, ctx), "běžná ne");
        }

        [Test] // S3.3 — eso nastaví přeskočení
        public void Ace_OnPlay_SetsSkipNextPlayer()
        {
            var ctx = Ctx();
            Card(Suit.Spades, Rank.Ace).OnPlay(ctx, null);
            Assert.IsTrue(ctx.SkipNextPlayer);
        }

        [Test] // S3.3 — eso nelze hrát při penalizaci
        public void Ace_DuringPenalty_NotPlayable()
        {
            var ctx = Ctx();
            ctx.NotifyDrawPenalty(2);
            var top = Card(Suit.Spades, Rank.Seven);
            Assert.IsFalse(Card(Suit.Spades, Rank.Ace).CanPlayOn(top, ctx));
        }

        [Test] // S3.4 — dáma na cokoliv
        public void Queen_IsPlayable_OnAnything()
        {
            var top = Card(Suit.Hearts, Rank.Eight);
            Assert.IsTrue(Card(Suit.Spades, Rank.Queen).CanPlayOn(top, Ctx()));
        }

        [Test] // S3.5 — dáma OnPlay nastaví vynucenou barvu na vybranou
        public void Queen_OnPlay_SetsForcedSuitToSelected()
        {
            var ctx = Ctx();
            var queen = (QueenCard)Card(Suit.Hearts, Rank.Queen);
            queen.SelectSuit(Suit.Clubs);
            queen.OnPlay(ctx, null);
            Assert.AreEqual(Suit.Clubs, ctx.ForcedSuit.Value);
        }

        [Test] // S3.6 — dáma nelze hrát při penalizaci
        public void Queen_DuringPenalty_NotPlayable()
        {
            var ctx = Ctx();
            ctx.NotifyDrawPenalty(2);
            var top = Card(Suit.Spades, Rank.Seven);
            Assert.IsFalse(Card(Suit.Spades, Rank.Queen).CanPlayOn(top, ctx));
        }

        [Test] // S3.7 — vynucená barva omezuje běžnou kartu
        public void ForcedSuit_RestrictsRegularCard()
        {
            var ctx = Ctx();
            ctx.NotifySuitChanged(Suit.Clubs);
            var top = Card(Suit.Clubs, Rank.Ten); // při forced suit na topu nezáleží
            Assert.IsFalse(Card(Suit.Hearts, Rank.Eight).CanPlayOn(top, ctx), "jiná barva ne");
            Assert.IsTrue(Card(Suit.Clubs, Rank.Nine).CanPlayOn(top, ctx), "shoda barvy ano");
        }

        [Test] // S3.8 — běžná karta zruší vynucenou barvu
        public void Regular_OnPlay_ClearsForcedSuit()
        {
            var ctx = Ctx();
            ctx.NotifySuitChanged(Suit.Clubs);
            Card(Suit.Clubs, Rank.Nine).OnPlay(ctx, null);
            Assert.IsNull(ctx.ForcedSuit);
        }
    }
}
