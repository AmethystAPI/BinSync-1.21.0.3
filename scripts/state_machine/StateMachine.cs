using System.Collections.Generic;
using Godot;

public class StateMachine {
  public string CurrentState => _currentState.Name;

  private Dictionary<string, State> _states = new Dictionary<string, State>();
  private State _currentState;
  private string _defaultState;

  public StateMachine(string defaultState) {
    _defaultState = defaultState;
  }

  public void Add(State state) {
    _states.Add(state.Name, state);

    state.InitializeState(this);
  }

  public void _Ready() {
    _currentState = GetState<State>(_defaultState);
    _currentState.Enter();
  }

  public void _Process(double delta) {
    foreach (State state in _states.Values) {
      state.UpdateBackground((float)delta);
    }

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
