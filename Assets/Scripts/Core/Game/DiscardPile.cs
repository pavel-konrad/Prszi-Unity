using System.Collections.Generic;

namespace Prsi.Core.Game
{
    /// <summary>
    /// Implementace odhazovacího balíčku
    /// Ekvivalent JS discard_pile.js z prszi projektu
    /// </summary>
    public class DiscardPile : IDiscardPile
    {
        private readonly List<ICardData> cards = new List<ICardData>();
        
        /// <summary>
        /// Vrchní karta na odhazovacím balíčku
        /// </summary>
        public ICardData TopCard => cards.Count > 0 ? cards[cards.Count - 1] : null;
        
        /// <summary>
        /// Počet karet v odhazovacím balíčku
        /// </summary>
        public int Count => cards.Count;
        
        /// <summary>
        /// Všechny karty v odhazovacím balíčku (read-only)
        /// </summary>
        public IReadOnlyList<ICardData> Cards => cards;
        
        /// <summary>
        /// Přidá kartu na vrchol odhazovacího balíčku
        /// </summary>
        public void Discard(ICardData card)
        {
            if (card != null)
            {
                cards.Add(card);
            }
        }
        
        /// <summary>
        /// Vyčistí odhazovací balíček
        /// </summary>
        public void Clear()
        {
            cards.Clear();
        }
        
        /// <summary>
        /// Odebere všechny karty kromě vrchní
        /// Používá se pro přehození do balíčku, když dojdou karty
        /// </summary>
        /// <returns>Všechny karty kromě vrchní</returns>
        public IReadOnlyList<ICardData> TakeAllExceptTop()
        {
            if (cards.Count <= 1)
            {
                return new List<ICardData>();
            }
            
            var topCard = cards[cards.Count - 1];
            cards.RemoveAt(cards.Count - 1);
            
            var cardsToReturn = new List<ICardData>(cards);
            cards.Clear();
            cards.Add(topCard);
            
            return cardsToReturn;
        }
    }
}
