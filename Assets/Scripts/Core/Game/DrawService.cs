using Prsi.Core.Cards;

namespace Prsi.Core.Game
{
    /// <summary>
    /// Draw logic over a deck + discard pile, including the Prší reshuffle rule:
    /// when the deck runs out, the discard pile (all but its top card) is shuffled
    /// back into the deck. Pure Core logic so it can be unit-tested.
    /// </summary>
    public static class DrawService
    {
        /// <summary>
        /// Draws one card. If the deck is empty, first reshuffles the discard pile
        /// (minus its top card) back into the deck (S6.1). Returns null only when
        /// neither source can yield a card.
        /// </summary>
        public static BaseCard DrawWithReshuffle(Deck deck, IDiscardPile discard)
        {
            if (deck == null) return null;

            if (deck.IsEmpty)
                ReshuffleDiscardIntoDeck(deck, discard);

            return deck.DrawBaseCard();
        }

        /// <summary>
        /// Moves the discard pile (all cards except the current top) back into the
        /// deck and shuffles. The top card stays in play. No-op if there is nothing
        /// to recycle.
        /// </summary>
        public static void ReshuffleDiscardIntoDeck(Deck deck, IDiscardPile discard)
        {
            if (deck == null || discard == null) return;

            var recycled = discard.TakeAllExceptTop();
            if (recycled.Count == 0) return;

            foreach (var card in recycled)
                deck.AddCard(card);

            deck.Shuffle();
        }
    }
}
