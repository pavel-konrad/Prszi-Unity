using System.Collections.Generic;
using UnityEngine;
using Prsi.Core.Cards;

namespace Prsi.Core.Game
{
    /// <summary>
    /// Implementace balíčku karet
    /// Ekvivalent JS deck.js z prszi projektu
    /// 
    /// Používá CardFactory pro vytváření karet správného typu
    /// </summary>
    public class Deck : ICardDeckProvider
    {
        private readonly List<BaseCard> cards = new List<BaseCard>();
        
        /// <summary>
        /// Počet zbývajících karet v balíčku
        /// </summary>
        public int RemainingCards => cards.Count;
        
        /// <summary>
        /// Inicializuje balíček s 32 kartami (německý mariáš)
        /// </summary>
        public void Initialize()
        {
            cards.Clear();
            cards.AddRange(CardFactory.CreateDeck());
            Shuffle();
        }
        
        /// <summary>
        /// Přidá kartu do balíčku (na spodek)
        /// </summary>
        public void AddCard(BaseCard card)
        {
            if (card != null)
            {
                cards.Add(card);
            }
        }
        
        /// <summary>
        /// Přidá kartu do balíčku (na spodek) - pro ICardData
        /// </summary>
        public void AddCard(ICardData card)
        {
            if (card is BaseCard baseCard)
            {
                cards.Add(baseCard);
            }
        }
        
        /// <summary>
        /// Přidá více karet do balíčku
        /// </summary>
        public void AddCards(IEnumerable<ICardData> cardsToAdd)
        {
            foreach (var card in cardsToAdd)
            {
                AddCard(card);
            }
        }
        
        /// <summary>
        /// Lízne kartu z vrcholu balíčku
        /// </summary>
        public ICardData DrawCard()
        {
            if (cards.Count == 0)
            {
                return null;
            }
            
            var card = cards[0];
            cards.RemoveAt(0);
            return card;
        }
        
        /// <summary>
        /// Lízne kartu jako BaseCard (pro polymorfismus)
        /// </summary>
        public BaseCard DrawBaseCard()
        {
            return DrawCard() as BaseCard;
        }
        
        /// <summary>
        /// Zamíchá balíček (Fisher-Yates shuffle)
        /// </summary>
        public void Shuffle()
        {
            for (int i = cards.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                var temp = cards[i];
                cards[i] = cards[j];
                cards[j] = temp;
            }
        }
        
        /// <summary>
        /// Vyčistí balíček
        /// </summary>
        public void Clear()
        {
            cards.Clear();
        }
        
        /// <summary>
        /// Zkontroluje, zda je balíček prázdný
        /// </summary>
        public bool IsEmpty => cards.Count == 0;
        
        /// <summary>
        /// Získá kartu na určité pozici (bez odebrání)
        /// </summary>
        public BaseCard Peek(int index = 0)
        {
            if (index >= 0 && index < cards.Count)
            {
                return cards[index];
            }
            return null;
        }
    }
}
