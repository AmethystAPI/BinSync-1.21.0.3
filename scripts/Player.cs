using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[Export] public Weapon EquippedWeapon;

	public override void _PhysicsProcess(double delta)
	{
		if (GetMultiplayerAuthority() != Multiplayer.GetUniqueId()) return;

		Vector2 movement = Vector2.Right * Input.GetAxis("move_left", "move_right") + Vector2.Up * Input.GetAxis("move_down", "move_up");

		Velocity = movement * 100f;

		MoveAndSlide();

		GetParent().GetNode<Camera2D>("Camera").Position = Position;
	}
}
