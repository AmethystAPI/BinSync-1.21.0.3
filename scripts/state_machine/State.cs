using Godot;

public class State {
  public string Name;

  private StateMachine _stateMachine;

  public State(string name) {
    Name = name;
  }

  public void GoToState(string name) {
    _stateMachine.GoToState(name);
  }

  public StateType GetState<StateType>(string name) where StateType : State {
    return _stateMachine.GetState<StateType>(name);
  }

  public void InitializeState(StateMachine stateMachine) {
    _stateMachine = stateMachine;

    Initialize();
  }

  public virtual void Initialize() {

  }

  public virtual void Enter() {

  }

  public virtual void UpdateBackground(float delta) {

  }

  public virtual void Update(float delta) {

  }

  public virtual void PhsysicsUpdate(float delta) {

  }

  public virtual void Exit() {

  }

  public virtual void OnInput(InputEvent inputEvent) {

  }
}