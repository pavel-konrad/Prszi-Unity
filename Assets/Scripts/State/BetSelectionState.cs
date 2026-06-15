using UnityEngine;

public class BetSelectionState : IGameState
{
    readonly GameStateMachine _fsm;
    readonly System.Action _onBetSelected;

    public BetSelectionState(GameStateMachine fsm, System.Action onBetSelected)
    {
        _fsm = fsm;
        _onBetSelected = onBetSelected;
    }

    public void Enter()
    {
        // Vyvolat událost pro otevření modálního okna
        var human = GameSession.I.Human;
        if (human != null)
        {
            UIEvents.TriggerBetModalOpened(human);
        }
    }

    public void Exit()
    {
        // Vyvolat událost pro zavření modálního okna
        UIEvents.TriggerBetModalClosed();
    }

    public void Tick(float dt) { }

    // Volá se z UI když hráč vybere sázku
    public void OnBetSelected(int betAmount)
    {
        var human = GameSession.I.Human;
        if (human != null)
        {
            human.SetBet(betAmount);
            int staked = human.PlaceBet();
            GameSession.I.AddToPot(staked);
            GameSession.I.NotifySessionChanged();
        }

        // Spustit animaci zavření modalu (zvuk se spustí přes Animation Event)
        Exit();
        _onBetSelected?.Invoke();
        
        // Přejít do herního stavu
        _fsm.Go<GameplayState>();
    }
}
