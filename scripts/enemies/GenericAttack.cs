using Godot;
using System;

public partial class GenericAttack : NodeState {
	[Export] public string ReturnState = "Idle";

	private Enemy _enemy;
	private AnimationPlayer _animationPlayer;

	public override void _Ready() {
		_enemy = GetParent().GetParent<Enemy>();
		_animationPlayer = GetParent().GetParent().GetNode<AnimationPlayer>("AnimationPlayer");
	}

	public override void Enter() {
		_animationPlayer.Play("attack");
	}

	public virtual void Attack() {

	}

	private void End() {
		GoToState(ReturnState);
	}
}
