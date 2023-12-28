using Godot;

public partial class State : Node
{
  public void GoToState(string name)
  {
    GetParent<StateMachine>().GoToState(name);
  }

  public virtual void Enter()
  {

  }

  public virtual void Update(float delta)
  {

  }

  public virtual void PhsysicsUpdate(float delta)
  {

  }

  public virtual void Exit()
  {

  }

  public virtual void OnInput(InputEvent inputEvent)
  {

  }
}