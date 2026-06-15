using UnityEngine;
using Prsi.Data;
using Prsi.Core;
using Prsi.Core.Cards;
using Prsi.Core.Services;

namespace Prsi.Tests
{
    /// <summary>
    /// Testovací skript pro ověření refaktoringu
    /// </summary>
    public class RefactoringTest : MonoBehaviour
    {
        [Header("Databases")]
        [SerializeField] private AvatarDB avatarDB;
        [SerializeField] private EnemyDB enemyDB;
        [SerializeField] private CardThemeDB cardThemeDB;
        
        [Header("Services")]
        [SerializeField] private CardThemeService cardThemeService;
        [SerializeField] private CardStateService cardStateService;
        [SerializeField] private CardDeckService cardDeckService;
        
        [Header("Test Options")]
        [SerializeField] private bool runTestsOnStart = false;
        [SerializeField] private bool logResults = true;
        
        void Start()
        {
            if (runTestsOnStart)
            {
                RunAllTests();
            }
        }
        
        [ContextMenu("Run All Tests")]
        public void RunAllTests()
        {
            Debug.Log("=== REFACTORING TESTS ===");
            
            TestAvatarDB();
            TestEnemyDB();
            TestCardThemeDB();
            TestCardThemeService();
            TestCardStateService();
            TestCardDeckService();
            
            Debug.Log("=== TESTS COMPLETE ===");
        }
        
        void TestAvatarDB()
        {
            Debug.Log("\n--- Testing AvatarDB ---");
            
            if (avatarDB == null)
            {
                Debug.LogError("AvatarDB is null!");
                return;
            }
            
            Debug.Log($"AvatarDB count: {avatarDB.Count}");
            
            for (int i = 0; i < avatarDB.Count; i++)
            {
                var avatar = avatarDB.GetAvatarByIndex(i);
                if (avatar != null)
                {
                    Debug.Log($"  Avatar {i}: {avatar.id} - {avatar.displayName}");
                }
            }
        }
        
        void TestEnemyDB()
        {
            Debug.Log("\n--- Testing EnemyDB ---");
            
            if (enemyDB == null)
            {
                Debug.LogError("EnemyDB is null!");
                return;
            }
            
            Debug.Log($"EnemyDB count: {enemyDB.Count}");
            
            for (int i = 0; i < enemyDB.Count; i++)
            {
                var enemy = enemyDB.GetEnemyByIndex(i);
                if (enemy != null)
                {
                    Debug.Log($"  Enemy {i}: {enemy.id} - {enemy.displayName} (Cash: {enemy.startingCash}, Bet: {enemy.defaultBet})");
                }
            }
            
            // Test náhodného výběru
            var randomEnemies = enemyDB.GetRandomEnemies(3, allowDuplicates: false);
            Debug.Log($"Random enemies (3): {randomEnemies.Count}");
        }
        
        void TestCardThemeDB()
        {
            Debug.Log("\n--- Testing CardThemeDB ---");
            
            if (cardThemeDB == null)
            {
                Debug.LogError("CardThemeDB is null!");
                return;
            }
            
            Debug.Log($"CardThemeDB count: {cardThemeDB.Count}");
            
            var defaultTheme = cardThemeDB.GetDefaultTheme();
            if (defaultTheme != null)
            {
                Debug.Log($"  Default theme: {defaultTheme.id} - {defaultTheme.displayName}");
                
                // Test získání sprite pro kartu
                var sprite = defaultTheme.GetCardSprite(Card.Suit.Hearts, Card.Rank.Ace);
                Debug.Log($"  Test sprite (Hearts Ace): {(sprite != null ? sprite.name : "null")}");
            }
        }
        
        void TestCardThemeService()
        {
            Debug.Log("\n--- Testing CardThemeService ---");
            
            if (cardThemeService == null)
            {
                Debug.LogError("CardThemeService is null!");
                return;
            }
            
            if (cardThemeDB != null)
            {
                cardThemeService.SetTheme(cardThemeDB);
                Debug.Log($"CardThemeService theme set: {cardThemeService.CurrentTheme?.displayName ?? "null"}");
                
                // Test získání sprite
                var sprite = cardThemeService.GetCardSprite(Card.Suit.Clubs, Card.Rank.King);
                Debug.Log($"  Test sprite (Clubs King): {(sprite != null ? sprite.name : "null")}");
                
                var backSprite = cardThemeService.GetCardBackSprite();
                Debug.Log($"  Card back sprite: {(backSprite != null ? backSprite.name : "null")}");
            }
        }
        
        void TestCardStateService()
        {
            Debug.Log("\n--- Testing CardStateService ---");
            
            if (cardStateService == null)
            {
                Debug.LogError("CardStateService is null!");
                return;
            }
            
            // Vytvořit testovací kartu pomocí CardFactory (polymorfní!)
            var testCard = CardFactory.Create(Card.Suit.Diamonds, Card.Rank.Queen);
            Debug.Log($"  Created card type: {testCard.GetType().Name}, IsSpecial: {testCard.IsSpecial}");
            
            // Zaregistrovat kartu
            cardStateService.RegisterCard(testCard, CardLocation.Deck);
            
            // Získat stav
            var state = cardStateService.GetCardState(testCard);
            Debug.Log($"  Card state: {state?.Location} (Owner: {state?.OwnerId})");
            
            // Změnit umístění
            cardStateService.SetCardLocation(testCard, CardLocation.PlayerHand, 0);
            Debug.Log($"  Card state after move: {state?.Location} (Owner: {state?.OwnerId})");
            
            // Test získání karet v umístění
            var cardsInHand = cardStateService.GetCardsInLocation(CardLocation.PlayerHand);
            int count = 0;
            foreach (var cardState in cardsInHand) count++;
            Debug.Log($"  Cards in PlayerHand: {count}");
        }
        
        void TestCardDeckService()
        {
            Debug.Log("\n--- Testing CardDeckService ---");
            
            if (cardDeckService == null)
            {
                Debug.LogError("CardDeckService is null!");
                return;
            }
            
            // Inicializovat balíček (používá CardFactory pro polymorfní karty)
            cardDeckService.InitializeDeck();
            Debug.Log($"  Deck initialized: {cardDeckService.RemainingCards} polymorphic cards");
            
            // Líznout několik karet a ukázat jejich typy
            for (int i = 0; i < 5; i++)
            {
                var card = cardDeckService.DrawBaseCard(); // Polymorfní líznutí
                if (card != null)
                {
                    Debug.Log($"  Drawn card {i + 1}: {card} ({card.GetType().Name}, IsSpecial: {card.IsSpecial})");
                }
            }
            
            Debug.Log($"  Remaining cards: {cardDeckService.RemainingCards}");
        }
    }
}

