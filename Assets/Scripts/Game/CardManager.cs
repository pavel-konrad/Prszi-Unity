using UnityEngine;

public class CardManager : MonoBehaviour
{
    [Header("Card Stacks")]
    public CardStack deck;
    public CardStack discard;
    public CardStack playerHand;
    
    [Header("UI Components")]
    public PlayerHand playerHandUI; // UI komponenta pro zobrazení ruky hráče
    public AIHand[] aiHandUIs; // UI komponenty pro zobrazení AI rukou
    
    [Header("AI Hand Stacks")]
    public CardStack[] aiHands; // Pole pro AI hand stacky
    
    [Header("Dealing Settings")]
    public int cardsPerPlayer = 5;
    public float dealDelay = 0.3f;
    
    // Events
    public System.Action<Player> OnPlayerWon;
    
    void Awake()
    {
        // Kontrola referencí
        if (deck == null) Debug.LogError("[CardManager] Deck není přiřazen!");
        if (discard == null) Debug.LogError("[CardManager] Discard není přiřazen!");
        if (playerHand == null) Debug.LogError("[CardManager] PlayerHand není přiřazen!");
        
        // Automaticky najít PlayerHand UI komponentu
        if (playerHandUI == null)
        {
            playerHandUI = FindObjectOfType<PlayerHand>();
            if (playerHandUI != null)
            {
                Debug.Log("[CardManager] PlayerHand UI komponenta nalezena automaticky");
            }
            else
            {
                Debug.LogWarning("[CardManager] PlayerHand UI komponenta nebyla nalezena automaticky");
            }
        }
        
        // Automaticky najít AIHand UI komponenty
        if (aiHandUIs == null || aiHandUIs.Length == 0)
        {
            AIHand[] foundAIHands = FindObjectsOfType<AIHand>();
            if (foundAIHands.Length > 0)
            {
                aiHandUIs = foundAIHands;
                Debug.Log($"[CardManager] Nalezeno {foundAIHands.Length} AIHand UI komponent automaticky");
            }
            else
            {
                Debug.LogWarning("[CardManager] AIHand UI komponenty nebyly nalezeny automaticky");
            }
        }
        
        // Kontrola AI hand stacků
        if (aiHands == null || aiHands.Length == 0)
        {
            Debug.LogError("[CardManager] AI hand stacky nejsou nastaveny!");
        }
        else
        {
            for (int i = 0; i < aiHands.Length; i++)
            {
                if (aiHands[i] == null)
                {
                    Debug.LogError($"[CardManager] AI hand {i} je null! Přiřaďte CardStack objekt do slotu {i}");
                }
            }
        }
        
        InitializeDeck();
    }
    
    void InitializeDeck()
    {
        if (deck == null)
        {
            Debug.LogError("[CardManager] Deck je null, nemohu inicializovat!");
            return;
        }
        
        deck.Clear();
        
        // Vytvořit německý balíček 32 karet (7, 8, 9, 10, J, Q, K, A v každé barvě)
        foreach (Card.Suit suit in System.Enum.GetValues(typeof(Card.Suit)))
        {
            foreach (Card.Rank rank in System.Enum.GetValues(typeof(Card.Rank)))
            {
                Card newCard = new Card(suit, rank);
                deck.AddCard(newCard);
            }
        }
        
        deck.Shuffle();
    }
    
    public void DealCardsToPlayers()
    {
        // Kontrola referencí
        if (deck == null) { Debug.LogError("[CardManager] Deck je null!"); return; }
        if (playerHand == null) { Debug.LogError("[CardManager] PlayerHand je null!"); return; }
        
        // Vyčistit ruce a odhazovací balíček
        playerHand.Clear();
        discard.Clear();
        
        // Vyčistit AI hand stacky
        if (aiHands != null)
        {
            foreach (var aiHand in aiHands)
            {
                if (aiHand != null) aiHand.Clear();
            }
        }
        
        // Vyčistit ruce všech hráčů
        foreach (var player in GameSession.I.Players)
        {
            player.ClearHand();
        }
        
        // Rozdat karty
        for (int i = 0; i < cardsPerPlayer; i++)
        {
            // Karta pro každého hráče (včetně human)
            for (int j = 0; j < GameSession.I.Players.Count; j++)
            {
                Card card = deck.DrawCard();
                if (card != null)
                {
                    var player = GameSession.I.Players[j];
                    player.AddCard(card);
                    
                    // Pro human hráče také přidat do playerHand stacku pro UI
                    if (j == 0) // Human player
                    {
                        playerHand.AddCard(card);
                    }
                    // Pro AI hráče přidat do odpovídajícího AI hand stacku
                    else if (aiHands != null && j-1 < aiHands.Length && aiHands[j-1] != null)
                    {
                        aiHands[j-1].AddCard(card);
                    }
                    else
                    {
                        Debug.LogError($"[CardManager] Nemohu přidat kartu do AI hand {j-1}: aiHands je {(aiHands == null ? "null" : "není null")}, délka je {(aiHands == null ? "N/A" : aiHands.Length.ToString())}");
                    }
                }
                else
                {
                    Debug.LogError($"[CardManager] Chyba: nemohu rozdat kartu pro hráče {j}");
                }
            }
        }
        
        // Po rozdání karet otočit jednu kartu nahoru do odhazovacího balíčku
        Card topCard = deck.DrawCard();
        if (topCard != null)
        {
            discard.AddCard(topCard);
            Debug.Log($"[CardManager] Otočena karta nahoru do discard: {topCard}");
        }
        else
        {
            Debug.LogError("[CardManager] Chyba: nemohu otočit kartu nahoru - balíček je prázdný!");
        }
    }
    
    // Hráč odehrál kartu
    public void PlayCard(Player player, Card card)
    {
        // Kontrola, zda hráč má kartu v ruce
        if (!player.hand.Contains(card))
        {
            Debug.LogWarning($"[CardManager] Hráč {player.Name} nemá kartu {card} v ruce!");
            return;
        }
        
        // Kontrola, zda je karta hratelná
        if (!CanPlayCard(card))
        {
            Debug.LogWarning($"[CardManager] Karta {card} není hratelná!");
            return;
        }
        
        // Pro human hráče nejdřív odebrat z playerHand stacku a UI
        if (player.IsHuman)
        {
            playerHand.RemoveCard(card);
            // Také odebrat z UI komponenty
            if (playerHandUI != null)
            {
                playerHandUI.RemoveCard(card);
            }
            else
            {
                Debug.LogWarning("[CardManager] playerHandUI je null - karta nebyla odebrána z UI!");
            }
        }
        // Pro AI hráče nejdřív odebrat z odpovídajícího AI hand stacku
        else
        {
            int aiIndex = player.Id - 1; // AI hráči mají ID 1, 2, 3...
            if (aiHands != null && aiIndex >= 0 && aiIndex < aiHands.Length && aiHands[aiIndex] != null)
            {
                aiHands[aiIndex].RemoveCard(card);
            }
        }
        
        // Odehrát kartu
        player.RemoveCard(card);
        discard.AddCard(card);
        
        Debug.Log($"[CardManager] Hráč {player.Name} odehrál kartu: {card}");
        
        // Kontrola, zda hráč vyhrál
        if (player.HasWon)
        {
            OnPlayerWon?.Invoke(player);
        }
        else
        {
            // Aktivovat dalšího hráče v pořadí
            GameSession.I.ActivateNextPlayer();
        }
    }
    
    // AI odehrá kartu (deprecated - nyní se používá GameplayState)
    public void PlayAICard(Player aiPlayer)
    {
        if (aiPlayer.hand.Count > 0)
        {
            // Najít platnou kartu
            Card cardToPlay = FindPlayableCard(aiPlayer);
            if (cardToPlay != null)
            {
                PlayCard(aiPlayer, cardToPlay);
            }
            else
            {
                Debug.Log($"[CardManager] AI {aiPlayer.Name} nemá platnou kartu");
            }
        }
    }
    
    // Zkontroluje, zda lze kartu zahrát
    public bool CanPlayCard(Card cardToPlay)
    {
        Card topCard = GetTopDiscardCard();
        if (topCard == null || cardToPlay == null) return false;
        
        // Stejná barva nebo stejná hodnota
        return cardToPlay.suit == topCard.suit || cardToPlay.rank == topCard.rank;
    }
    
    // Získá vrchní kartu z odhazovacího balíčku
    public Card GetTopDiscardCard()
    {
        if (discard?.cards != null && discard.cards.Count > 0)
        {
            return discard.cards[discard.cards.Count - 1];
        }
        return null;
    }
    
    // Najde hratelnou kartu v ruce hráče
    public Card FindPlayableCard(Player player)
    {
        foreach (Card card in player.hand)
        {
            if (CanPlayCard(card))
            {
                return card;
            }
        }
        return null;
    }
    
    // Lízne kartu pro hráče
    public Card DrawCardForPlayer(Player player)
    {
        Debug.Log($"[CardManager] DrawCardForPlayer voláno pro hráče: {player.Name}");
        Debug.Log($"[CardManager] Stav balíčku před líznutím: deck.cards.Count={deck?.cards?.Count ?? -1}");
        
        // Pokud je balíček prázdný, přehodit odhazovací balíček pozpátku
        if (deck.cards.Count == 0)
        {
            Debug.Log("[CardManager] Balíček je prázdný, přehazuji odhazovací balíček pozpátku");
            ReshuffleDiscardIntoDeck();
        }
        
        Card drawnCard = deck.DrawCard();
        Debug.Log($"[CardManager] Líznutá karta: {drawnCard}");
        
        if (drawnCard != null)
        {
            // Pro human hráče nejdřív přidat do UI s CardIn animací
            if (player.IsHuman)
            {
                // Přidat kartu do UI s CardIn animací DŘÍV než do dat
                if (playerHandUI != null)
                {
                    playerHandUI.AddCardToHand(drawnCard);
                    Debug.Log($"[CardManager] Karta přidána do playerHandUI s CardIn animací");
                }
                else
                {
                    Debug.LogWarning("[CardManager] playerHandUI je null - CardIn animace se nespustí");
                }
                
                // Pak přidat do dat
                playerHand.AddCard(drawnCard);
                Debug.Log($"[CardManager] Karta přidána do playerHand stacku");
            }
            // Pro AI hráče přidat do odpovídajícího AI hand stacku
            else
            {
                int aiIndex = player.Id - 1; // AI hráči mají ID 1, 2, 3...
                if (aiHands != null && aiIndex >= 0 && aiIndex < aiHands.Length && aiHands[aiIndex] != null)
                {
                    aiHands[aiIndex].AddCard(drawnCard);
                }
                
                // Přidat kartu do AI UI s CardIn animací
                if (aiHandUIs != null && aiIndex >= 0 && aiIndex < aiHandUIs.Length && aiHandUIs[aiIndex] != null)
                {
                    aiHandUIs[aiIndex].AddCard(drawnCard);
                    Debug.Log($"[CardManager] AI karta přidána do aiHandUI[{aiIndex}] s CardIn animací");
                }
                else
                {
                    Debug.LogWarning($"[CardManager] aiHandUIs[{aiIndex}] je null - CardIn animace se nespustí");
                }
            }
            
            // Přidat kartu do dat hráče (pro všechny hráče)
            player.AddCard(drawnCard);
            
            Debug.Log($"[CardManager] Hráč {player.Name} si líznul kartu: {drawnCard}");
        }
        else
        {
            Debug.LogError("[CardManager] Nelze líznout kartu - balíček je prázdný!");
        }
        
        return drawnCard;
    }
    
    // Přehodí odhazovací balíček zpět do balíčku pozpátku (kromě vrchní karty) - pravidla Prší
    void ReshuffleDiscardIntoDeck()
    {
        if (discard.cards.Count <= 1)
        {
            Debug.LogWarning("[CardManager] Nelze přehodit - odhazovací balíček má jen jednu nebo žádnou kartu");
            return;
        }
        
        // Ponechat vrchní kartu v discard
        Card topCard = discard.cards[discard.cards.Count - 1];
        discard.cards.RemoveAt(discard.cards.Count - 1);
        
        // Přehodit zbytek do balíčku pozpátku (bez míchání)
        for (int i = discard.cards.Count - 1; i >= 0; i--)
        {
            deck.AddCard(discard.cards[i]);
        }
        
        discard.Clear();
        discard.AddCard(topCard);
        
        Debug.Log($"[CardManager] Odhazovací balíček byl přehozen zpět do balíčku pozpátku (bez míchání). Vrchní karta: {topCard}");
    }
    
    public void DiscardPlayerCard(Card card)
    {
        if (playerHand.cards.Contains(card))
        {
            playerHand.cards.Remove(card);
            GameSession.I.Human.RemoveCard(card);
            discard.AddCard(card);
        }
    }
    
    public void ResetForNewRound()
    {
        // Přesunout všechny karty zpět do balíčku
        deck.AddCards(discard.cards);
        deck.AddCards(playerHand.cards);
        
        // Přesunout karty ze všech hráčů
        foreach (var player in GameSession.I.Players)
        {
            deck.AddCards(player.hand);
        }
        
        discard.Clear();
        playerHand.Clear();
        
        // Vyčistit ruce hráčů
        foreach (var player in GameSession.I.Players)
        {
            player.ClearHand();
        }
        
        // Zamíchat balíček
        deck.Shuffle();
    }
    
    // Kontrola, zda je konec hry
    public bool IsGameOver()
    {
        int playersWithCards = 0;
        foreach (var player in GameSession.I.Players)
        {
            if (player.IsInGame)
            {
                playersWithCards++;
            }
        }
        return playersWithCards <= 1; // Konec hry když zůstane max 1 hráč s kartami
    }
    
    // Najít vítěze
    public Player GetWinner()
    {
        foreach (var player in GameSession.I.Players)
        {
            if (player.HasWon)
            {
                return player;
            }
        }
        return null;
    }
}
