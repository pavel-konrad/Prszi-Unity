using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Prsi.Core;
using Prsi.Core.Cards;

namespace Prsi.Core.Services
{
    /// <summary>
    /// Service pro správu balíčku karet
    /// Refaktorováno pro použití CardFactory a polymorfních karet
    /// </summary>
    public class CardDeckService : MonoBehaviour, ICardDeckProvider
    {
        private List<BaseCard> deck = new List<BaseCard>();
        private ICardStateProvider cardStateProvider;
        
        public int RemainingCards => deck.Count;
        
        /// <summary>
        /// Přístup k balíčku jako seznam BaseCard (pro polymorfismus)
        /// </summary>
        public IReadOnlyList<BaseCard> Cards => deck;
        
        private void Awake()
        {
            cardStateProvider = FindFirstObjectByType<CardStateService>();
            if (cardStateProvider == null)
            {
                Debug.LogWarning("[CardDeckService] CardStateService nebyl nalezen");
            }
        }
        
        /// <summary>
        /// Inicializuje balíček s kartami pomocí CardFactory
        /// Factory vytváří správné typy karet (SevenCard, AceCard, QueenCard, RegularCard)
        /// </summary>
        public void InitializeDeck()
        {
            deck.Clear();
            
            // Použít CardFactory pro vytvoření polymorfních karet
            deck.AddRange(CardFactory.CreateDeck());
            
            // Zaregistrovat karty v CardStateService
            if (cardStateProvider != null)
            {
                foreach (var card in deck)
                {
                    (cardStateProvider as CardStateService)?.RegisterCard(card, CardLocation.Deck);
                }
            }
            
            Shuffle();
            
            Debug.Log($"[CardDeckService] Balíček inicializován s {deck.Count} kartami (polymorfní typy)");
        }
        
        /// <summary>
        /// Přidá kartu do balíčku (BaseCard)
        /// </summary>
        public void AddCard(BaseCard card)
        {
            if (card == null) return;
            
            deck.Add(card);
            
            // Aktualizovat stav karty
            if (cardStateProvider != null)
            {
                cardStateProvider.SetCardLocation(card, CardLocation.Deck);
            }
        }
        
        /// <summary>
        /// Přidá kartu do balíčku (ICardData - pro kompatibilitu)
        /// </summary>
        public void AddCard(ICardData card)
        {
            if (card == null) return;
            
            // Pokud je to BaseCard, přidat přímo
            if (card is BaseCard baseCard)
            {
                AddCard(baseCard);
                return;
            }
            
            // Pokud není BaseCard, vytvořit novou kartu pomocí factory
            var newCard = CardFactory.Create(card.Suit, card.Rank);
            deck.Add(newCard);
            
            // Aktualizovat stav karty
            if (cardStateProvider != null)
            {
                cardStateProvider.SetCardLocation(newCard, CardLocation.Deck);
            }
        }
        
        /// <summary>
        /// Přidá více karet do balíčku
        /// </summary>
        public void AddCards(IEnumerable<ICardData> cards)
        {
            if (cards == null) return;
            
            foreach (var card in cards)
            {
                AddCard(card);
            }
        }
        
        /// <summary>
        /// Lízne kartu jako BaseCard (pro polymorfismus)
        /// </summary>
        public BaseCard DrawBaseCard()
        {
            return DrawCard() as BaseCard;
        }
        
        public ICardData DrawCard()
        {
            if (deck.Count == 0)
            {
                Debug.LogWarning("[CardDeckService] Balíček je prázdný, nelze líznout kartu");
                return null;
            }
            
            var card = deck[0];
            deck.RemoveAt(0);
            
            // Aktualizovat stav karty na Drawn (přechodný stav)
            if (cardStateProvider != null)
            {
                cardStateProvider.SetCardLocation(card, CardLocation.Drawn);
            }
            
            return card;
        }
        
        public void Shuffle()
        {
            // Fisher-Yates shuffle
            for (int i = deck.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                var temp = deck[i];
                deck[i] = deck[j];
                deck[j] = temp;
            }
        }
        
        /// <summary>
        /// Vyčistí balíček
        /// </summary>
        public void Clear()
        {
            deck.Clear();
        }
    }
}

