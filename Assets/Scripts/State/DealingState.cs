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
        // Vymazat aktivního hráče během rozdávání
        GameSession.I.SetActiveIndex(-1);
        
        // Spustit animace rozdání karet pro všechny hand komponenty
        StartDealingAnimations();
        
        // Počkat na začátek animací
        yield return new WaitForSeconds(0.5f);

        // Rozdat karty
        var cardManager = Object.FindFirstObjectByType<CardManager>();
        
        if (cardManager != null)
        {
            cardManager.DealCardsToPlayers();
            
            // Aktualizovat UI pro rozdání karet
            PlayerHand playerHand = Object.FindFirstObjectByType<PlayerHand>();
            if (playerHand != null)
            {
                playerHand.UpdateHand(GameSession.I.Human);
            }
        }
        else
        {
            Debug.LogError("[DealingState] CardManager nebyl nalezen! Zkontrolujte, zda je v scéně.");
        }

        // Vyvolat události pro UI
        GameSession.I.NotifySessionChanged();

        // Human začíná
        GameSession.I.SetActiveIndex(0);

        // Rovnou do hry (žádné sázení — turnaj řeší ekonomiku až na konci kola)
        _fsm.Go<GameplayState>();
    }
    
    // Spustit animace rozdávání pro všechny hand komponenty
    void StartDealingAnimations()
    {
        // Najít PlayerHand komponentu
        PlayerHand playerHand = Object.FindFirstObjectByType<PlayerHand>();
        if (playerHand != null)
        {
            playerHand.StartDealingAnimation();
        }
        
        // Najít všechny AIHand komponenty
        AIHand[] aiHands = Object.FindObjectsOfType<AIHand>();
        foreach (var aiHand in aiHands)
        {
            aiHand.StartDealingAnimation();
        }
        
        // Zvuk se spustí přes Animation Events v jednotlivých hand animacích
        // AudioEvents.TriggerCardDealt() se volá z AnimationEventReceiver
    }
}
