using UnityEngine;
using UnityEngine.UI;

public class GameDirector : MonoBehaviour
{
    [Header("Panels")]
    public GameObject introPanel;
    public GameObject gamePanel;

    [Header("Controllers")]
    public MainMenu mainMenu;

    private GameStateMachine fsm;
    private BetSelectionState betSelectionState;

    void Awake()
    {
        fsm = gameObject.AddComponent<GameStateMachine>();

        var menu = new MenuState(introPanel, gamePanel, mainMenu, fsm);
        var dealing = new DealingState(this);
        betSelectionState = new BetSelectionState(fsm, OnBetSelected);
        var gameplay = new GameplayState(this);

        fsm.Register<MenuState>(menu);
        fsm.Register<DealingState>(dealing);
        fsm.Register<BetSelectionState>(betSelectionState);
        fsm.Register<GameplayState>(gameplay);
    }

    void Start()
    {
        fsm.Go<MenuState>();
    }

    void OnBetSelected()
    {
        // Po výběru sázky pokračovat v hře
        // TODO: přejít na další stav (např. PlayerTurnState)
        // Pro teď jen vyvolat OnDone callback z DealingState
        // dealing.OnDone?.Invoke();
    }

    // Veřejná metoda pro UI tlačítka
    public void OnBetButtonClicked(int betAmount)
    {
        if (betSelectionState != null)
        {
            betSelectionState.OnBetSelected(betAmount);
        }
    }
}
