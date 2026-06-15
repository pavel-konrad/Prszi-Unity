using System;
using System.Collections.Generic;
using UnityEngine;

public class GameStateMachine : MonoBehaviour
{
    readonly Dictionary<Type, IGameState> _states = new();
    IGameState _current;

    public void Register<T>(IGameState state) where T:IGameState { _states[typeof(T)] = state; }
    public void Go<T>() where T:IGameState {
        _current?.Exit();
        _current = _states[typeof(T)];
        _current.Enter();
    }
    void Update(){ _current?.Tick(Time.deltaTime); }
}
