using UnityEngine;

public class GameRootBinder : MonoBehaviour
{
    [Header("UI panels")]
    public PlayerUI human;        // PlayerBar (yours)
    public PlayerUI enemy1;       // EnemyBar 1
    public PlayerUI enemy2;       // EnemyBar 2
    public PlayerUI enemy3;       // EnemyBar 3

    void Start()
    {
        var gs = GameSession.I;
        if (gs == null) { Debug.LogError("[GameRootBinder] GameSession is missing in the scene."); return; }

        if (human  && gs.Players.Count > 0) human.Bind(gs.Players[0]);
        if (enemy1 && gs.Players.Count > 1) enemy1.Bind(gs.Players[1]);
        if (enemy2 && gs.Players.Count > 2) enemy2.Bind(gs.Players[2]);
        if (enemy3 && gs.Players.Count > 3) enemy3.Bind(gs.Players[3]);

        // force UI to initial refresh
        foreach (var p in gs.Players) p.NotifyChanged();

        // Who is on turn at the start
        gs.SetActiveIndex(0);
    }
}
