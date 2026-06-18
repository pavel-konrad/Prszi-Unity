using System.Collections;
using UnityEngine;
using Prsi.Core.Cards;

public class GameplayState : IGameState
{
    readonly MonoBehaviour _runner;
    readonly GameStateMachine _fsm;
    
    private CardManager cardManager;
    private float aiThinkTime = 2.0f; // Think time for AI
    
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
        
        // Find CardManager
        cardManager = Object.FindFirstObjectByType<CardManager>();
        if (cardManager == null)
        {
            Debug.LogError("[GameplayState] CardManager not found!");
            return;
        }
        
        // Subscribe to events
        cardManager.OnPlayerWon += OnPlayerWon;
        GameSession.I.ActivePlayerChanged += OnActivePlayerChanged;
        
        // Check if active player is AI; if so, start AI turn
        CheckActivePlayer();
    }
    
    public void Exit()
    {
        // Unsubscribe from events
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
        // No tick logic needed in this state
    }
    
    // Checks active player and starts AI if it is an AI player
    void CheckActivePlayer()
    {
        var activePlayer = GameSession.I.ActiveIndex >= 0 && GameSession.I.ActiveIndex < GameSession.I.Players.Count
            ? GameSession.I.Players[GameSession.I.ActiveIndex]
            : null;

        // Ace effect: an Ace is on top. Anyone without their own Ace to defend automatically
        // "stands" (skipped, does NOT draw). Anyone with an Ace plays normally (may counter).
        if (activePlayer != null && GameSession.I.Rules != null
            && GameSession.I.Rules.AcePending && !HasAce(activePlayer))
        {
            GameSession.I.Rules.AcePending = false;
            _runner.StartCoroutine(SkipTurn(activePlayer));
            return;
        }

        if (activePlayer != null && !activePlayer.IsHuman && activePlayer.CanPlay)
        {
            _runner.StartCoroutine(ProcessAITurn(activePlayer));
        }
        else if (activePlayer != null && activePlayer.IsHuman)
        {
            // Human player plays via UI — nothing else to do
            // Turn stops here and waits for UI interaction
        }
        else if (activePlayer == null || !activePlayer.CanPlay)
        {
            Debug.LogWarning($"[GameplayState] Active player cannot play or does not exist: activePlayer={activePlayer?.Name ?? "null"}, CanPlay={activePlayer?.CanPlay}");
            // Continue to next player
            GameSession.I.ActivateNextPlayer();
            // NON-RECURSIVE — do not call CheckActivePlayer() here
        }
    }
    
    // Does the player have an Ace in hand (to defend against a pending Ace)?
    bool HasAce(Player player)
    {
        foreach (Card c in player.hand)
            if (c.rank == Rank.Ace) return true;
        return false;
    }

    // Ace effect: skipped player briefly "stands" (UI message), then turn passes on.
    IEnumerator SkipTurn(Player player)
    {
        GameLog.Record("STAND", player.Name);
        PlayerEffectDisplay.Instance?.ShowSkip(player);
        yield return new WaitForSeconds(1.2f);
        PlayerEffectDisplay.Instance?.Hide(player);
        GameSession.I.ActivateNextPlayer();
    }

    // AI turn processing
    IEnumerator ProcessAITurn(Player aiPlayer)
    {
        // Wait for AI to think
        yield return new WaitForSeconds(aiThinkTime);
        
        // Get top card from discard pile
        Card topCard = GetTopDiscardCard();
        if (topCard == null)
        {
            Debug.LogError("[GameplayState] Cannot get the top card from the discard pile!");
            yield break;
        }
        
        // Find valid card in AI hand
        Card playableCard = FindPlayableCard(aiPlayer, topCard);
        
        if (playableCard != null)
        {
            // AI can play a card
            cardManager.PlayCard(aiPlayer, playableCard);
        }
        else
        {
            // AI has no valid card — must draw
            cardManager.DrawCardForPlayer(aiPlayer); // Use synchronous method for AI
            
            // In Prší: after drawing, turn ends automatically even if the drawn card is playable
            yield return new WaitForSeconds(0.5f);
            
            GameSession.I.ActivateNextPlayer();
            // NON-RECURSIVE — OnActivePlayerChanged will call CheckActivePlayer()
        }
    }
    
    // Finds a playable card in the player's hand
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
    
    // Gets top card from discard pile
    Card GetTopDiscardCard()
    {
        if (cardManager?.discard?.cards != null && cardManager.discard.cards.Count > 0)
        {
            return cardManager.discard.cards[cardManager.discard.cards.Count - 1];
        }
        return null;
    }
    
    // Called when the active player changes
    void OnActivePlayerChanged(Player activePlayer, int activeIndex)
    {
        // Check the new active player
        CheckActivePlayer();
    }
    
    // Called when a player wins the round → round evaluation (RoundEndState in Phase C).
    void OnPlayerWon(Player winner)
    {
        _fsm.Go<RoundEndState>();
    }
    
    // Public method for card validation (called from UI)
    public bool CanPlayCard(Card cardToPlay)
    {
        Card topCard = GetTopDiscardCard();
        return IsCardPlayable(cardToPlay, topCard);
    }
}
