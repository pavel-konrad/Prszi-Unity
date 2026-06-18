using System.Collections;
using UnityEngine;

public class DealingState : IGameState
{
    readonly MonoBehaviour _runner;
    readonly GameStateMachine _fsm;
    public System.Action OnDone;
    
    public DealingState(MonoBehaviour runner)
    { 
        _runner = runner;
        _fsm = runner.GetComponent<GameStateMachine>();
    }

    public void Enter(){ _runner.StartCoroutine(DealRoutine()); }
    public void Exit(){}
    public void Tick(float dt){}

    IEnumerator DealRoutine(){
        // Clear active player during dealing
        GameSession.I.SetActiveIndex(-1);
        
        // Start deal animations for all hand components
        StartDealingAnimations();
        
        // Wait for animations to start
        yield return new WaitForSeconds(0.5f);

        // Deal cards
        var cardManager = Object.FindFirstObjectByType<CardManager>();
        
        if (cardManager != null)
        {
            cardManager.DealCardsToPlayers();
            
            // Update UI for dealing cards
            PlayerHand playerHand = Object.FindFirstObjectByType<PlayerHand>();
            if (playerHand != null)
            {
                playerHand.UpdateHand(GameSession.I.Human);
            }
        }
        else
        {
            Debug.LogError("[DealingState] CardManager not found! Check whether it is in the scene.");
        }

        // Raise events for UI
        GameSession.I.NotifySessionChanged();

        // Human goes first
        GameSession.I.SetActiveIndex(0);

        // Straight into gameplay (no betting — tournament economy settles at round end)
        _fsm.Go<GameplayState>();
    }
    
    // Start dealing animations for all hand components
    void StartDealingAnimations()
    {
        // Find PlayerHand component
        PlayerHand playerHand = Object.FindFirstObjectByType<PlayerHand>();
        if (playerHand != null)
        {
            playerHand.StartDealingAnimation();
        }
        
        // Find all AIHand components
        AIHand[] aiHands = Object.FindObjectsOfType<AIHand>();
        foreach (var aiHand in aiHands)
        {
            aiHand.StartDealingAnimation();
        }
        
        // Sound plays via Animation Events in individual hand animations
        // AudioEvents.TriggerCardDealt() is called from AnimationEventReceiver
    }
}
