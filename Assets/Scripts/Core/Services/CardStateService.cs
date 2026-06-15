using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Prsi.Core;

namespace Prsi.Core.Services
{
    /// <summary>
    /// Service pro správu stavů karet
    /// </summary>
    public class CardStateService : MonoBehaviour, ICardStateProvider
    {
        private Dictionary<ICardData, CardState> cardStates = new Dictionary<ICardData, CardState>();
        
        /// <summary>
        /// Událost vyvolaná při změně umístění karty
        /// </summary>
        public event Action<ICardState, CardLocation> OnCardLocationChanged;
        
        public ICardState GetCardState(ICardData card)
        {
            if (card == null) return null;
            
            if (!cardStates.TryGetValue(card, out CardState state))
            {
                // Vytvořit nový stav s výchozím umístěním v balíčku
                state = new CardState(card, CardLocation.Deck);
                cardStates[card] = state;
                
                // Přihlásit se k události změny stavu
                state.LocationChanged += (cardState, previousLocation) =>
                {
                    OnCardLocationChanged?.Invoke(cardState, previousLocation);
                };
            }
            
            return state;
        }
        
        public void SetCardLocation(ICardData card, CardLocation location, int? ownerId = null)
        {
            if (card == null)
            {
                Debug.LogWarning("[CardStateService] Pokus o nastavení umístění pro null kartu");
                return;
            }
            
            var state = GetCardState(card) as CardState;
            if (state != null)
            {
                state.SetLocation(location, ownerId);
            }
        }
        
        public IEnumerable<ICardState> GetCardsInLocation(CardLocation location)
        {
            return cardStates.Values.Where(s => s.Location == location);
        }
        
        public IEnumerable<ICardState> GetPlayerCards(int playerId)
        {
            return cardStates.Values.Where(s => 
                (s.Location == CardLocation.PlayerHand || s.Location == CardLocation.EnemyHand) &&
                s.OwnerId == playerId);
        }
        
        /// <summary>
        /// Zaregistruje kartu do systému (voláno při vytvoření karty)
        /// </summary>
        public void RegisterCard(ICardData card, CardLocation initialLocation = CardLocation.Deck, int? ownerId = null)
        {
            if (card == null) return;
            
            if (!cardStates.ContainsKey(card))
            {
                var state = new CardState(card, initialLocation, ownerId);
                cardStates[card] = state;
                
                // Přihlásit se k události změny stavu
                state.LocationChanged += (cardState, previousLocation) =>
                {
                    OnCardLocationChanged?.Invoke(cardState, previousLocation);
                };
            }
        }
        
        /// <summary>
        /// Odregistruje kartu ze systému
        /// </summary>
        public void UnregisterCard(ICardData card)
        {
            if (card != null && cardStates.ContainsKey(card))
            {
                cardStates.Remove(card);
            }
        }
        
        /// <summary>
        /// Vyčistí všechny stavy karet
        /// </summary>
        public void ClearAllStates()
        {
            cardStates.Clear();
        }
    }
}

