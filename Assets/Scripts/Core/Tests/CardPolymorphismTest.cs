using UnityEngine;
using Prsi.Core.Cards;
using Prsi.Core.Game;
using System.Collections.Generic;

namespace Prsi.Core.Tests
{
    /// <summary>
    /// Test demonstrující polymorfismus karet
    /// Spustí se po přidání komponenty na GameObject
    /// </summary>
    public class CardPolymorphismTest : MonoBehaviour
    {
        [ContextMenu("Test Card Polymorphism")]
        public void TestPolymorphism()
        {
            Debug.Log("=== TEST POLYMORFISMU KARET ===");
            
            // Vytvoření karet pomocí factory
            var seven = CardFactory.Create(Card.Suit.Hearts, Card.Rank.Seven);
            var ace = CardFactory.Create(Card.Suit.Spades, Card.Rank.Ace);
            var queen = CardFactory.Create(Card.Suit.Diamonds, Card.Rank.Queen);
            var regular = CardFactory.Create(Card.Suit.Clubs, Card.Rank.Ten);
            
            Debug.Log($"Seven type: {seven.GetType().Name}, IsSpecial: {seven.IsSpecial}");
            Debug.Log($"Ace type: {ace.GetType().Name}, IsSpecial: {ace.IsSpecial}");
            Debug.Log($"Queen type: {queen.GetType().Name}, IsSpecial: {queen.IsSpecial}");
            Debug.Log($"Regular type: {regular.GetType().Name}, IsSpecial: {regular.IsSpecial}");
            
            Debug.Log("=== TEST DOKONČEN ===");
        }
        
        [ContextMenu("Test CanPlayOn Rules")]
        public void TestCanPlayOnRules()
        {
            Debug.Log("=== TEST PRAVIDEL HRANÍ ===");
            
            // Vytvoření testovacího kontextu
            var context = new GameContext(null, null, new List<IPlayerData>());
            
            // Vrchní karta
            var topCard = CardFactory.Create(Card.Suit.Hearts, Card.Rank.Eight);
            Debug.Log($"Vrchní karta: {topCard}");
            
            // Test různých karet
            var sameSuit = CardFactory.Create(Card.Suit.Hearts, Card.Rank.Ten);
            var sameRank = CardFactory.Create(Card.Suit.Clubs, Card.Rank.Eight);
            var different = CardFactory.Create(Card.Suit.Spades, Card.Rank.King);
            var queen = CardFactory.Create(Card.Suit.Diamonds, Card.Rank.Queen);
            
            Debug.Log($"Stejná barva ({sameSuit}): {sameSuit.CanPlayOn(topCard, context)}"); // true
            Debug.Log($"Stejná hodnota ({sameRank}): {sameRank.CanPlayOn(topCard, context)}"); // true
            Debug.Log($"Jiná ({different}): {different.CanPlayOn(topCard, context)}"); // false
            Debug.Log($"Dáma ({queen}): {queen.CanPlayOn(topCard, context)}"); // true (dáma na cokoliv)
            
            Debug.Log("=== TEST DOKONČEN ===");
        }
        
        [ContextMenu("Test Seven Card Effect")]
        public void TestSevenEffect()
        {
            Debug.Log("=== TEST EFEKTU SEDMY ===");
            
            var context = new GameContext(null, null, new List<IPlayerData>());
            var seven1 = CardFactory.Create(Card.Suit.Hearts, Card.Rank.Seven);
            var seven2 = CardFactory.Create(Card.Suit.Diamonds, Card.Rank.Seven);
            var regular = CardFactory.Create(Card.Suit.Hearts, Card.Rank.Ten);
            
            Debug.Log($"PendingDrawCount před: {context.PendingDrawCount}");
            
            // Zahrát první sedmu
            seven1.OnPlay(context, null);
            Debug.Log($"Po první sedmě: {context.PendingDrawCount}"); // 2
            
            // Zahrát druhou sedmu (obrana)
            seven2.OnPlay(context, null);
            Debug.Log($"Po druhé sedmě: {context.PendingDrawCount}"); // 4
            
            // Regular karta nelze zahrát při penalizaci
            Debug.Log($"Lze zahrát regular při penalizaci: {regular.CanPlayOn(seven2, context)}"); // false
            Debug.Log($"Lze zahrát další sedmu jako obranu: {CardFactory.Create(Card.Suit.Clubs, Card.Rank.Seven).CanPlayOn(seven2, context)}"); // true
            
            Debug.Log("=== TEST DOKONČEN ===");
        }
        
        [ContextMenu("Test Queen Suit Change")]
        public void TestQueenSuitChange()
        {
            Debug.Log("=== TEST ZMĚNY BARVY DÁMOU ===");
            
            var context = new GameContext(null, null, new List<IPlayerData>());
            var queen = CardFactory.Create(Card.Suit.Hearts, Card.Rank.Queen) as QueenCard;
            
            Debug.Log($"ForcedSuit před: {context.ForcedSuit}");
            
            // Nastavit vybranou barvu
            queen.SelectSuit(Card.Suit.Spades);
            queen.OnPlay(context, null);
            
            Debug.Log($"ForcedSuit po: {context.ForcedSuit}"); // Spades
            
            // Test hratelnosti s vynucenou barvou
            var spadeCard = CardFactory.Create(Card.Suit.Spades, Card.Rank.Ten);
            var heartCard = CardFactory.Create(Card.Suit.Hearts, Card.Rank.Ten);
            
            Debug.Log($"Pik karta s forced Spades: {spadeCard.CanPlayOn(queen, context)}"); // true
            Debug.Log($"Srdce karta s forced Spades: {heartCard.CanPlayOn(queen, context)}"); // false
            
            Debug.Log("=== TEST DOKONČEN ===");
        }
        
        [ContextMenu("Test Ace Skip Effect")]
        public void TestAceSkipEffect()
        {
            Debug.Log("=== TEST EFEKTU ESA ===");
            
            var context = new GameContext(null, null, new List<IPlayerData>());
            var ace = CardFactory.Create(Card.Suit.Hearts, Card.Rank.Ace);
            
            Debug.Log($"SkipNextPlayer před: {context.SkipNextPlayer}");
            
            ace.OnPlay(context, null);
            
            Debug.Log($"SkipNextPlayer po: {context.SkipNextPlayer}"); // true
            
            Debug.Log("=== TEST DOKONČEN ===");
        }
        
        [ContextMenu("Test Full Deck Creation")]
        public void TestFullDeckCreation()
        {
            Debug.Log("=== TEST VYTVOŘENÍ BALÍČKU ===");
            
            var deck = CardFactory.CreateDeck();
            
            int sevens = 0, aces = 0, queens = 0, regulars = 0;
            
            foreach (var card in deck)
            {
                if (card is SevenCard) sevens++;
                else if (card is AceCard) aces++;
                else if (card is QueenCard) queens++;
                else if (card is RegularCard) regulars++;
            }
            
            Debug.Log($"Celkem karet: {deck.Count}");
            Debug.Log($"Sedmy (SevenCard): {sevens}");
            Debug.Log($"Esa (AceCard): {aces}");
            Debug.Log($"Dámy (QueenCard): {queens}");
            Debug.Log($"Běžné (RegularCard): {regulars}");
            
            Debug.Log("=== TEST DOKONČEN ===");
        }
    }
}
