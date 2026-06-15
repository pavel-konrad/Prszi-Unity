using System.Collections;
using UnityEngine;

public class GameplayState : IGameState
{
    readonly MonoBehaviour _runner;
    readonly GameStateMachine _fsm;
    
    private CardManager cardManager;
    private float aiThinkTime = 2.0f; // Čas na rozmyšlení pro AI
    
    public GameplayState(MonoBehaviour runner)
    {
        _runner = runner;
        _fsm = runner.GetComponent<GameStateMachine>();
    }
    
    public void Enter()
    {
        Debug.Log("[GameplayState] Entering gameplay state");
        
        // Debug: zkontrolovat všechny hráče při vstupu do gameplay state
        Debug.Log($"[GameplayState] Při vstupu do gameplay state:");
        for (int i = 0; i < GameSession.I.Players.Count; i++)
        {
            var player = GameSession.I.Players[i];
            Debug.Log($"[GameplayState] Hráč {i}: ID={player.Id}, Name={player.Name}, IsHuman={player.IsHuman}, CanPlay={player.CanPlay}");
        }
        
        // Najít CardManager
        cardManager = Object.FindFirstObjectByType<CardManager>();
        if (cardManager == null)
        {
            Debug.LogError("[GameplayState] CardManager nebyl nalezen!");
            return;
        }
        
        // Přihlásit se k událostem
        cardManager.OnPlayerWon += OnPlayerWon;
        GameSession.I.ActivePlayerChanged += OnActivePlayerChanged;
        
        // Zkontrolovat, zda je aktivní hráč AI, a pokud ano, spustit AI tah
        CheckActivePlayer();
    }
    
    public void Exit()
    {
        // Odhlásit se od událostí
        if (cardManager != null)
        {
            cardManager.OnPlayerWon -= OnPlayerWon;
        }
        
        if (GameSession.I != null)
        {
            GameSession.I.ActivePlayerChanged -= OnActivePlayerChanged;
        }
    }
    
    public void Tick(float dt)
    {
        // V tomto stavu není potřeba tick logika
    }
    
    // Zkontroluje aktivního hráče a spustí AI pokud je to AI hráč
    void CheckActivePlayer()
    {
        var activePlayer = GameSession.I.ActiveIndex >= 0 && GameSession.I.ActiveIndex < GameSession.I.Players.Count 
            ? GameSession.I.Players[GameSession.I.ActiveIndex] 
            : null;
            
        Debug.Log($"[GameplayState] CheckActivePlayer: ActiveIndex={GameSession.I.ActiveIndex}, activePlayer={activePlayer?.Name ?? "null"}, IsHuman={activePlayer?.IsHuman}, CanPlay={activePlayer?.CanPlay}");
            
        if (activePlayer != null && !activePlayer.IsHuman && activePlayer.CanPlay)
        {
            Debug.Log($"[GameplayState] AI hráč {activePlayer.Name} je na tahu");
            _runner.StartCoroutine(ProcessAITurn(activePlayer));
        }
        else if (activePlayer != null && activePlayer.IsHuman)
        {
            Debug.Log($"[GameplayState] Human hráč {activePlayer.Name} je na tahu - čekám na UI input");
            // Human hráč hraje přes UI - nic dalšího nedělat
            // Tah se zastaví zde a čeká na UI interakci
        }
        else if (activePlayer == null || !activePlayer.CanPlay)
        {
            Debug.LogWarning($"[GameplayState] Aktivní hráč nemůže hrát nebo neexistuje: activePlayer={activePlayer?.Name ?? "null"}, CanPlay={activePlayer?.CanPlay}");
            // Pokračovat na dalšího hráče
            GameSession.I.ActivateNextPlayer();
            // NEREKURZIVNĚ - nevolat CheckActivePlayer() zde
        }
    }
    
    // Zpracování AI tahu
    IEnumerator ProcessAITurn(Player aiPlayer)
    {
        // Počkat na rozmyšlení AI
        yield return new WaitForSeconds(aiThinkTime);
        
        // Získat vrchní kartu z odhazovacího balíčku
        Card topCard = GetTopDiscardCard();
        if (topCard == null)
        {
            Debug.LogError("[GameplayState] Nelze získat vrchní kartu z discard pile!");
            yield break;
        }
        
        // Najít platnou kartu v ruce AI
        Card playableCard = FindPlayableCard(aiPlayer, topCard);
        
        if (playableCard != null)
        {
            // AI může hrát kartu
            Debug.Log($"[GameplayState] AI {aiPlayer.Name} hraje kartu: {playableCard}");
            cardManager.PlayCard(aiPlayer, playableCard);
        }
        else
        {
            // AI nemá platnou kartu - musí si líznout
            Debug.Log($"[GameplayState] AI {aiPlayer.Name} nemá platnou kartu, lízne si");
            cardManager.DrawCardForPlayer(aiPlayer); // Použít synchronní metodu pro AI
            
            // V Prší: po líznutí karty automaticky končí tah, i když je karta hratelná
            yield return new WaitForSeconds(0.5f);
            
            Debug.Log($"[GameplayState] AI {aiPlayer.Name} si líznul kartu, předávám tah");
            GameSession.I.ActivateNextPlayer();
            // NEREKURZIVNĚ - OnActivePlayerChanged se postará o CheckActivePlayer()
        }
    }
    
    // Najde hratelnou kartu v ruce hráče
    Card FindPlayableCard(Player player, Card topCard)
    {
        foreach (Card card in player.hand)
        {
            if (IsCardPlayable(card, topCard))
            {
                return card;
            }
        }
        return null;
    }
    
    // Zkontroluje, zda lze kartu zahrát (stejná barva nebo hodnota)
    bool IsCardPlayable(Card cardToPlay, Card topCard)
    {
        if (cardToPlay == null || topCard == null) return false;
        
        // Stejná barva nebo stejná hodnota
        return cardToPlay.suit == topCard.suit || cardToPlay.rank == topCard.rank;
    }
    
    // Získá vrchní kartu z odhazovacího balíčku
    Card GetTopDiscardCard()
    {
        if (cardManager?.discard?.cards != null && cardManager.discard.cards.Count > 0)
        {
            return cardManager.discard.cards[cardManager.discard.cards.Count - 1];
        }
        return null;
    }
    
    // Lízne kartu pro hráče
    void DrawCardForPlayer(Player player)
    {
        if (cardManager.deck.cards.Count == 0)
        {
            Debug.LogWarning("[GameplayState] Balíček je prázdný! Přehazujeme odhazovací balíček pozpátku...");
            ReshuffleDiscardIntoDeck();
        }
        
        Card drawnCard = cardManager.deck.DrawCard();
        if (drawnCard != null)
        {
            player.AddCard(drawnCard);
            
            // Pro human hráče také přidat do playerHand stacku pro UI
            if (player.IsHuman)
            {
                cardManager.playerHand.AddCard(drawnCard);
            }
            // Pro AI hráče přidat do odpovídajícího AI hand stacku
            else
            {
                int aiIndex = player.Id - 1; // AI hráči mají ID 1, 2, 3...
                if (cardManager.aiHands != null && aiIndex >= 0 && aiIndex < cardManager.aiHands.Length && cardManager.aiHands[aiIndex] != null)
                {
                    cardManager.aiHands[aiIndex].AddCard(drawnCard);
                }
            }
            
            Debug.Log($"[GameplayState] Hráč {player.Name} si líznul kartu: {drawnCard}");
        }
        else
        {
            Debug.LogError("[GameplayState] Nelze líznout kartu - balíček je prázdný!");
        }
    }
    
    // Přehodí odhazovací balíček zpět do balíčku pozpátku (kromě vrchní karty) - pravidla Prší
    void ReshuffleDiscardIntoDeck()
    {
        if (cardManager.discard.cards.Count <= 1)
        {
            Debug.LogWarning("[GameplayState] Nelze přehodit - odhazovací balíček má jen jednu nebo žádnou kartu");
            return;
        }
        
        // Ponechat vrchní kartu v discard
        Card topCard = cardManager.discard.cards[cardManager.discard.cards.Count - 1];
        cardManager.discard.cards.RemoveAt(cardManager.discard.cards.Count - 1);
        
        // Přehodit zbytek do balíčku pozpátku (bez míchání)
        for (int i = cardManager.discard.cards.Count - 1; i >= 0; i--)
        {
            cardManager.deck.AddCard(cardManager.discard.cards[i]);
        }
        
        cardManager.discard.Clear();
        cardManager.discard.AddCard(topCard);
        
        Debug.Log($"[GameplayState] Odhazovací balíček byl přehozen zpět do balíčku pozpátku (bez míchání). Vrchní karta: {topCard}");
    }
    
    // Volá se když se změní aktivní hráč
    void OnActivePlayerChanged(Player activePlayer, int activeIndex)
    {
        Debug.Log($"[GameplayState] Změna aktivního hráče na: {activePlayer?.Name ?? "null"} (index {activeIndex}), IsHuman={activePlayer?.IsHuman}, CanPlay={activePlayer?.CanPlay}");
        // Zkontrolovat nového aktivního hráče
        CheckActivePlayer();
    }
    
    // Volá se když hráč vyhraje
    void OnPlayerWon(Player winner)
    {
        Debug.Log($"[GameplayState] Hráč {winner.Name} vyhrál!");
        
        // Vyplatit bank vítězi
        GameSession.I.PayoutToWinner(winner.Id);
        
        // Přejít zpět do menu nebo začít nové kolo
        // Pro teď přejdeme zpět do menu
        _fsm.Go<MenuState>();
    }
    
    // Veřejná metoda pro validaci karty (volá se z UI)
    public bool CanPlayCard(Card cardToPlay)
    {
        Card topCard = GetTopDiscardCard();
        return IsCardPlayable(cardToPlay, topCard);
    }
    
    // Veřejná metoda pro líznutí karty (volá se z UI)
    public void DrawCardForCurrentPlayer()
    {
        var activePlayer = GameSession.I.ActiveIndex >= 0 && GameSession.I.ActiveIndex < GameSession.I.Players.Count 
            ? GameSession.I.Players[GameSession.I.ActiveIndex] 
            : null;
            
        if (activePlayer != null && activePlayer.IsHuman)
        {
            DrawCardForPlayer(activePlayer);
            
            // Po líznutí předat tah dalšímu hráči
            GameSession.I.ActivateNextPlayer();
            // NEREKURZIVNĚ - OnActivePlayerChanged se postará o CheckActivePlayer()
        }
    }
}
