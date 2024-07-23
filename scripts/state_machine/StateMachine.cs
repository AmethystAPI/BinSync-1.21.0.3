using System.Collections.Generic;
using Godot;

public class StateMachine {
  public string CurrentState => _currentState.Name;

  private Dictionary<string, State> _states = new Dictionary<string, State>();
  private State _currentState;

  public StateMachine(string defaultState, State[] states) {
    foreach (State state in states) {
      _states.Add(state.Name, state);
    }

    _currentState = GetState<State>(defaultState);
  }

  public void _Ready() {
    _currentState.Enter();
  }

  public void _Process(double delta) {
    _currentState.Update((float)delta);
  }

  public void _PhysicsProcess(double delta) {
    _currentState.PhsysicsUpdate((float)delta);
  }

  public void _Input(InputEvent inputEvent) {
    _currentState.OnInput(inputEvent);
  }

  public void GoToState(string name) {
    _currentState.Exit();

    _currentState = GetState<State>(name);

    _currentState.Enter();
  }

  public StateType GetState<StateType>(string name) where StateType : State {
    return _states[name] as StateType;
  }
}
