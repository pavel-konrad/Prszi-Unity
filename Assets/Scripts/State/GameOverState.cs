using System.Collections.Generic;
using UnityEngine;
using Prsi.Core.Game;

/// <summary>
/// Tournament over: the last player with money is the overall winner. Shows the
/// final modal; "Do menu" returns to the menu.
/// </summary>
public class GameOverState : IGameState
{
    readonly GameStateMachine _fsm;

    public GameOverState(MonoBehaviour runner)
    {
        _fsm = runner.GetComponent<GameStateMachine>();
    }

    public void Enter()
    {
        var winner = TournamentRules.OverallWinner(new List<ITournamentPlayer>(GameSession.I.Players));
        string name = winner != null ? winner.Name : "?";

        if (GameOverModal.Instance != null)
            GameOverModal.Instance.Show(name, ToMenu);
        else
            ToMenu();
    }

    public void Exit() { }
    public void Tick(float dt) { }

    void ToMenu() => _fsm.Go<MenuState>();
}
