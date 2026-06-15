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

        /// <summary>Balíček s náhodným seedem (běžná hra).</summary>
        public Deck() : this(System.Environment.TickCount) { }

        /// <summary>Balíček s daným seedem (reprodukovatelný — testy, replaye).</summary>
        public Deck(int seed)
        {
            rng = new System.Random(seed);
        }

        public int RemainingCards => cards.Count;
        public bool IsEmpty => cards.Count == 0;

        /// <summary>Naplní balíček 32 polymorfními kartami (mariáš) a zamíchá. Hlavně pro testy.</summary>
        public void Initialize()
        {
            cards.Clear();
            cards.AddRange(CardFactory.CreateDeck());
            Shuffle();
        }

        /// <summary>Přidá kartu na spodek balíčku.</summary>
        public void AddCard(ICardData card)
        {
            if (card != null) cards.Add(card);
        }

        /// <summary>Přidá více karet na spodek balíčku.</summary>
        public void AddCards(IEnumerable<ICardData> cardsToAdd)
        {
            foreach (var card in cardsToAdd) AddCard(card);
        }

        /// <summary>Lízne kartu z vrcholu balíčku (null když je prázdný).</summary>
        public ICardData DrawCard()
        {
            if (cards.Count == 0) return null;

            var card = cards[0];
            cards.RemoveAt(0);
            return card;
        }

        /// <summary>Fisher-Yates shuffle se seedovaným RNG.</summary>
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

        /// <summary>Vyčistí balíček.</summary>
        public void Clear() => cards.Clear();

        /// <summary>Karta na dané pozici bez odebrání (null mimo rozsah).</summary>
        public ICardData Peek(int index = 0)
            => index >= 0 && index < cards.Count ? cards[index] : null;
    }
}
