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
        
        for (int i = 0; i < GameSession.I.Players.Count; i++)
        {
            var player = GameSession.I.Players[i];
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
            
            
        if (activePlayer != null && !activePlayer.IsHuman && activePlayer.CanPlay)
        {
            _runner.StartCoroutine(ProcessAITurn(activePlayer));
        }
        else if (activePlayer != null && activePlayer.IsHuman)
        {
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
            cardManager.PlayCard(aiPlayer, playableCard);
        }
        else
        {
            // AI nemá platnou kartu - musí si líznout
            cardManager.DrawCardForPlayer(aiPlayer); // Použít synchronní metodu pro AI
            
            // V Prší: po líznutí karty automaticky končí tah, i když je karta hratelná
            yield return new WaitForSeconds(0.5f);
            
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
    
    // Card playability is decided by the Prsi.Core rule engine (special cards included).
    bool IsCardPlayable(Card cardToPlay, Card topCard)
    {
        if (cardToPlay == null || topCard == null) return false;

        return Prsi.Core.Cards.CardRules.CanPlay(cardToPlay, topCard, GameSession.I.Rules);
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
    
    // Volá se když se změní aktivní hráč
    void OnActivePlayerChanged(Player activePlayer, int activeIndex)
    {
        // Zkontrolovat nového aktivního hráče
        CheckActivePlayer();
    }
    
    // Volá se když hráč vyhraje
    void OnPlayerWon(Player winner)
    {
        
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
}
