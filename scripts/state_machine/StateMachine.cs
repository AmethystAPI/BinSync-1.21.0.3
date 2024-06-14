using Godot;

public partial class StateMachine : Node {
  [Export] public string DefaultState = "Idle";

  public string CurrentState => _currentState.Name;

  private State _currentState;

  public override void _Ready() {
    _currentState = GetNode<State>(DefaultState);
    _currentState.Enter();
  }

  public override void _Process(double delta) {
    _currentState.Update((float)delta);
  }

  public override void _PhysicsProcess(double delta) {
    _currentState.PhsysicsUpdate((float)delta);
  }

  public override void _Input(InputEvent inputEvent) {
    _currentState.OnInput(inputEvent);
  }

  public void GoToState(string name) {
    _currentState.Exit();

    _currentState = GetNode<State>(name);

    _currentState.Enter();
  }

  public StateType GetState<StateType>(string name) where StateType : State {
    return GetNode<StateType>(name);
  }
}
