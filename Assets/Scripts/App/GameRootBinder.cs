using UnityEngine;

public class GameRootBinder : MonoBehaviour
{
    [Header("UI panely")]
    public PlayerUI human;        // PlayerBar (tvůj)
    public PlayerUI enemy1;       // EnemyBar 1
    public PlayerUI enemy2;       // EnemyBar 2
    public PlayerUI enemy3;       // EnemyBar 3

    void Start()
    {
        var gs = GameSession.I;
        if (gs == null) { Debug.LogError("[GameRootBinder] Chybí GameSession ve scéně."); return; }

        if (human  && gs.Players.Count > 0) human.Bind(gs.Players[0]);
        if (enemy1 && gs.Players.Count > 1) enemy1.Bind(gs.Players[1]);
        if (enemy2 && gs.Players.Count > 2) enemy2.Bind(gs.Players[2]);
        if (enemy3 && gs.Players.Count > 3) enemy3.Bind(gs.Players[3]);

        // donutit UI k počátečnímu refreshi
        foreach (var p in gs.Players) p.NotifyChanged();

        // Kdo je na tahu na startu
        gs.SetActiveIndex(0);
    }
}
