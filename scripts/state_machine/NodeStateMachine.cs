using Godot;

public partial class NodeStateMachine : Node {
  [Export] public string DefaultState = "Idle";

  public string CurrentState => _currentState.Name;

  private NodeState _currentState;

  public override void _Ready() {
    _currentState = GetNode<NodeState>(DefaultState);
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

    _currentState = GetNode<NodeState>(name);

    _currentState.Enter();
  }

  public StateType GetState<StateType>(string name) where StateType : NodeState {
    return GetNode<StateType>(name);
  }
}
