using UnityEngine;

public class GameDirector : MonoBehaviour
{
    [Header("Panels")]
    public GameObject introPanel;
    public GameObject gamePanel;

    [Header("Controllers")]
    public MainMenu mainMenu;

    private GameStateMachine fsm;

    void Awake()
    {
        fsm = gameObject.AddComponent<GameStateMachine>();

        var menu = new MenuState(introPanel, gamePanel, mainMenu, fsm);
        var dealing = new DealingState(this);
        var gameplay = new GameplayState(this);
        var roundEnd = new RoundEndState(this);
        var gameOver = new GameOverState(this);

        fsm.Register<MenuState>(menu);
        fsm.Register<DealingState>(dealing);
        fsm.Register<GameplayState>(gameplay);
        fsm.Register<RoundEndState>(roundEnd);
        fsm.Register<GameOverState>(gameOver);
    }

    void Start()
    {
        fsm.Go<MenuState>();
    }
}
