using System.Collections.Generic;
using UnityEngine;
using Prsi.Core.Game;

/// <summary>
/// End of a round: settle the card penalties through TournamentRules, clear the
/// active highlight, show the inter-round modal with the winner and scores, then
/// advance — another round while >1 player has money, otherwise the game-over state.
/// </summary>
public class RoundEndState : IGameState
{
    readonly GameStateMachine _fsm;

    public RoundEndState(MonoBehaviour runner)
    {
        _fsm = runner.GetComponent<GameStateMachine>();
    }

    public void Enter()
    {
        var gs = GameSession.I;
        var players = gs.Players;

        // Round winner = the player who emptied their hand.
        Player winner = players.Find(p => p.HasWon);

        GameLog.Record("ROUND_END", winner != null ? winner.Name : "?");
        GameLog.Flush();

        // Pay penalties (losers pay card value × rate; winner collects).
        var tournamentPlayers = new List<ITournamentPlayer>(players);
        TournamentRules.SettleRound(tournamentPlayers, winner, gs.penaltyRate);

        // Clear the active highlight so no bar stays glowing between rounds.
        gs.SetActiveIndex(-1);
        gs.NotifySessionChanged();

        // Build the score list (cash, OUT) and show the modal.
        var rows = new List<RoundEndModal.ScoreRow>();
        foreach (var p in players)
            rows.Add(new RoundEndModal.ScoreRow { Name = p.Name, Cash = p.Cash, Eliminated = p.Cash == 0 });

        if (RoundEndModal.Instance != null)
            RoundEndModal.Instance.Show(winner != null ? winner.Name : "?", rows, Advance);
        else
            Advance(); // no modal in scene → just continue
    }

    public void Exit() { }
    public void Tick(float dt) { }

    void Advance()
    {
        var remaining = TournamentRules.RemainingPlayers(new List<ITournamentPlayer>(GameSession.I.Players));
        if (remaining.Count > 1) _fsm.Go<DealingState>();
        else _fsm.Go<GameOverState>();
    }
}
