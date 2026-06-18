using System.Collections.Generic;
using Prsi.Core.Cards;

namespace Prsi.Core.Game
{
    /// <summary>
    /// Draw deck. Holds any <see cref="ICardData"/>, so the running game can put its
    /// UI <c>Card</c> instances straight in while Core still owns the shuffle/draw
    /// logic. Shuffling is deterministic via an injected seed (System.Random) —
    /// same seed = same order → reproducible runs and tests. No UnityEngine
    /// dependency, so it runs in EditMode.
    /// </summary>
    public class Deck
    {
        private readonly List<ICardData> cards = new List<ICardData>();
        private readonly System.Random rng;

        /// <summary>Deck with a random seed (normal play).</summary>
        public Deck() : this(System.Environment.TickCount) { }

        /// <summary>Deck with a given seed (reproducible — tests, replays).</summary>
        public Deck(int seed)
        {
            rng = new System.Random(seed);
        }

        public int RemainingCards => cards.Count;
        public bool IsEmpty => cards.Count == 0;

        /// <summary>Fills the deck with 32 polymorphic cards (mariáš) and shuffles. Mainly for tests.</summary>
        public void Initialize()
        {
            cards.Clear();
            cards.AddRange(CardFactory.CreateDeck());
            Shuffle();
        }

        /// <summary>Adds a card to the bottom of the deck.</summary>
        public void AddCard(ICardData card)
        {
            if (card != null) cards.Add(card);
        }

        /// <summary>Adds multiple cards to the bottom of the deck.</summary>
        public void AddCards(IEnumerable<ICardData> cardsToAdd)
        {
            foreach (var card in cardsToAdd) AddCard(card);
        }

        /// <summary>Draws a card from the top of the deck (null when empty).</summary>
        public ICardData DrawCard()
        {
            if (cards.Count == 0) return null;

            var card = cards[0];
            cards.RemoveAt(0);
            return card;
        }

        /// <summary>Fisher-Yates shuffle with a seeded RNG.</summary>
        public void Shuffle()
        {
            for (int i = cards.Count - 1; i > 0; i--)
            {
                int j = rng.Next(0, i + 1);
                (cards[i], cards[j]) = (cards[j], cards[i]);
            }
        }

        /// <summary>
        /// Recycles the given cards (typically the discard pile minus its top card)
        /// back into the deck and reshuffles. The Prší rule for an exhausted deck (S6.1).
        /// </summary>
        public void ReshuffleFrom(IEnumerable<ICardData> cards)
        {
            AddCards(cards);
            Shuffle();
        }

        /// <summary>Clears the deck.</summary>
        public void Clear() => cards.Clear();

        /// <summary>Card at the given index without removing it (null if out of range).</summary>
        public ICardData Peek(int index = 0)
            => index >= 0 && index < cards.Count ? cards[index] : null;
    }
}
