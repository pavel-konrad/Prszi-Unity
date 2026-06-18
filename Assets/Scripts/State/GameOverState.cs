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
        var gs = GameSession.I;
        var winner = TournamentRules.OverallWinner(new List<ITournamentPlayer>(gs.Players));

        // Message based on how the game ended for the human player.
        string message;
        if (gs.Human != null && gs.Human.Cash == 0)
            message = "Vypadl jsi!";
        else if (winner != null && winner == (ITournamentPlayer)gs.Human)
            message = "Vyhrál jsi!";
        else
            message = winner != null ? $"Vítěz: {winner.Name}" : "Konec hry";

        if (GameOverModal.Instance != null)
            GameOverModal.Instance.Show(message, ToMenu);
        else
            ToMenu();
    }

    public void Exit() { }
    public void Tick(float dt) { }

    void ToMenu() => _fsm.Go<MenuState>();
}
