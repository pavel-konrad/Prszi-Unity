using NUnit.Framework;
using Prsi.Core.Cards;
using Prsi.Core.Game;

namespace Prsi.Tests.EditMode
{
    /// <summary>SPEC S6 — reshuffle the discard pile back into an empty deck.</summary>
    public class DrawServiceTests
    {
        private static BaseCard Card(Suit s, Rank r) => CardFactory.Create(s, r);

        [Test] // S6.1 — empty deck reshuffles discard (minus top) before drawing
        public void DrawWithReshuffle_RecyclesDiscardWhenDeckEmpty()
        {
            var deck = new Deck(1);          // empty (no Initialize)
            var discard = new DiscardPile();
            discard.Discard(Card(Suit.Hearts, Rank.Eight)); // becomes deck fodder
            discard.Discard(Card(Suit.Spades, Rank.Nine));  // becomes deck fodder
            discard.Discard(Card(Suit.Clubs, Rank.Ten));    // top — stays in play

            var drawn = DrawService.DrawWithReshuffle(deck, discard);

            Assert.IsNotNull(drawn, "a card was recycled from the discard");
            Assert.AreEqual(1, discard.Count, "only the top card remains");
            Assert.AreEqual(Rank.Ten, discard.TopCard.Rank, "the top card is untouched");
        }

        [Test] // S6.1 — nothing to recycle (discard has only the top): returns null, no throw
        public void DrawWithReshuffle_EmptyDeckSingleDiscard_ReturnsNull()
        {
            var deck = new Deck(1);
            var discard = new DiscardPile();
            discard.Discard(Card(Suit.Clubs, Rank.Ten));

            Assert.IsNull(DrawService.DrawWithReshuffle(deck, discard));
            Assert.AreEqual(1, discard.Count);
        }

        [Test] // normal draw: non-empty deck is untouched by reshuffle path
        public void DrawWithReshuffle_NonEmptyDeck_DrawsFromDeck()
        {
            var deck = new Deck(1);
            deck.Initialize(); // 32 cards
            var discard = new DiscardPile();

            var drawn = DrawService.DrawWithReshuffle(deck, discard);

            Assert.IsNotNull(drawn);
            Assert.AreEqual(31, deck.RemainingCards);
        }
    }
}
