using System.Collections.Generic;
using NUnit.Framework;
using Prsi.Core;
using Prsi.Core.Cards;
using Prsi.Core.Game;

namespace Prsi.Tests.EditMode
{
    /// <summary>
    /// SPEC S1 — Deck. Focus on S1.2: deterministic shuffle (seed) =
    /// reproducibility and predictability. RED-first: API new Deck(seed) does not exist yet.
    /// </summary>
    public class DeckTests
    {
        private static List<(Suit, Rank)> DrawAll(Deck deck)
        {
            var order = new List<(Suit, Rank)>();
            while (!deck.IsEmpty)
            {
                var c = deck.DrawCard();
                order.Add((c.Suit, c.Rank));
            }
            return order;
        }

        [Test] // S1.1 — fresh deck = 32 unique cards (4×8)
        public void FreshDeck_Has32UniqueCards()
        {
            var deck = new Deck(1);
            deck.Initialize();

            Assert.AreEqual(32, deck.RemainingCards);
            var all = DrawAll(deck);
            Assert.AreEqual(32, all.Count);
            CollectionAssert.AllItemsAreUnique(all);
        }

        [Test] // S1.2 — same seed → same order (deterministic shuffle)
        public void SameSeed_ProducesSameOrder()
        {
            var a = new Deck(12345); a.Initialize();
            var b = new Deck(12345); b.Initialize();
            CollectionAssert.AreEqual(DrawAll(a), DrawAll(b));
        }

        [Test] // S1.2 — different seed → (practically certainly) different order
        public void DifferentSeed_ProducesDifferentOrder()
        {
            var a = new Deck(1); a.Initialize();
            var b = new Deck(2); b.Initialize();
            CollectionAssert.AreNotEqual(DrawAll(a), DrawAll(b));
        }

        [Test] // S1.3 — drawing reduces count by 1
        public void Draw_ReducesRemainingByOne()
        {
            var deck = new Deck(1);
            deck.Initialize();
            int before = deck.RemainingCards;
            deck.DrawCard();
            Assert.AreEqual(before - 1, deck.RemainingCards);
        }

        [Test] // S1.4 — drawing from an empty deck returns null
        public void DrawFromEmpty_ReturnsNull()
        {
            var deck = new Deck(1); // empty (without Initialize)
            Assert.IsTrue(deck.IsEmpty);
            Assert.IsNull(deck.DrawCard());
        }

        [Test] // S6.1 — exhausted deck is refilled from the discard (reshuffle)
        public void ReshuffleFrom_RecyclesCardsBackIntoDeck()
        {
            var deck = new Deck(1); // empty
            var recycled = new ICardData[]
            {
                CardFactory.Create(Suit.Hearts, Rank.Eight),
                CardFactory.Create(Suit.Spades, Rank.Nine),
                CardFactory.Create(Suit.Clubs, Rank.Ten),
            };

            deck.ReshuffleFrom(recycled);

            Assert.AreEqual(3, deck.RemainingCards);
            Assert.IsNotNull(deck.DrawCard());
        }
    }
}
