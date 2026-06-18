using UnityEngine;
using System.Threading.Tasks;

public class MenuState : IGameState
{
    readonly GameObject _intro, _game;
    readonly MainMenu _menu;
    readonly GameStateMachine _fsm;

    public MenuState(GameObject introPanel, GameObject gamePanel, MainMenu menu, GameStateMachine fsm){
        _intro=introPanel; _game=gamePanel; _menu=menu; _fsm=fsm;
    }

    public void Enter(){ _intro.SetActive(true); _game.SetActive(false); _menu.OnStart = OnStart; }
    public void Exit(){ _menu.OnStart = null; }
    public void Tick(float dt){}

    async void OnStart(){
        // Sound plays via Animation Event v MainMenu animaci
        // AudioEvents.TriggerGameStart() is called from AnimationEventReceiver
        await Task.Delay(2000);
        _intro.SetActive(false); 
        _game.SetActive(true); 
        
        
        _fsm.Go<DealingState>(); 
    }
}
