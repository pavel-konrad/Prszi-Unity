using UnityEngine;

public class GameRootBinder : MonoBehaviour
{
    [Header("UI panely")]
    public PlayerUI human;        // PlayerBar (tvůj)
    public PlayerUI enemy1;       // EnemyBar 1
    public PlayerUI enemy2;       // EnemyBar 2
    public PlayerUI enemy3;       // EnemyBar 3

    [Header("Start volby (volitelné)")]
    public bool randomizeAiBets = true;

    void Start()
    {
        var gs = GameSession.I;
        if (gs == null) { Debug.LogError("[GameRootBinder] Chybí GameSession ve scéně."); return; }
        
        
        // GameRootBinder.Start()
        if (human  && gs.Players.Count > 0) human.Bind(gs.Players[0]);
        if (enemy1 && gs.Players.Count > 1) enemy1.Bind(gs.Players[1]);
        if (enemy2 && gs.Players.Count > 2) enemy2.Bind(gs.Players[2]);
        if (enemy3 && gs.Players.Count > 3) enemy3.Bind(gs.Players[3]);

        // 🔑 donutit UI k počátečnímu refreshi
        foreach (var p in gs.Players) p.NotifyChanged();


        // Volitelně: náhodné sázky AI ze sady {25..1000}
        if (randomizeAiBets)
        {
            for (int i = 1; i < gs.Players.Count; i++)
                gs.Players[i].SetBet(BetRules.RandomAffordable(gs.Players[i].Cash));
        }

        // Kdo je na tahu na startu (rozsvítí ActiveLoop)
        gs.SetActiveIndex(0);
        GameSession.I.StartNewRound();

    }
}
