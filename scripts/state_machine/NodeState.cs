using Godot;

public partial class NodeState : Node {
  public void GoToState(string name) {
    GetParent<NodeStateMachine>().GoToState(name);
  }

  public StateType GetState<StateType>(string name) where StateType : NodeState {
    return GetParent<NodeStateMachine>().GetState<StateType>(name);
  }

  public virtual void Enter() {

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