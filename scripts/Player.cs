using Godot;
using System;
using System.Collections.Generic;

public partial class Player : CharacterBody2D, Damageable
{
	public static List<Player> Players = new List<Player>();

	[Export] public Weapon EquippedWeapon;

	private int _health = 3;

	public override void _Ready()
	{
		Players.Add(this);
	}

	public override void _PhysicsProcess(double delta)
	{
		if (GetMultiplayerAuthority() != Multiplayer.GetUniqueId()) return;

		Vector2 movement = Vector2.Right * Input.GetAxis("move_left", "move_right") + Vector2.Up * Input.GetAxis("move_down", "move_up");

		Velocity = movement.Normalized() * 100f;

		MoveAndSlide();

		GetParent().GetNode<Camera2D>("Camera").Position = Position;
	}

	public void Damage(Projectile projectile)
	{
		if (GetMultiplayerAuthority() != Multiplayer.GetUniqueId()) return;

		_health--;

		if (_health > 0) return;

		_health = 3;

		GlobalPosition = Vector2.Zero;
	}

	public bool CanDamage(Projectile projectile)
	{
		return !(projectile.Source is Player);
	}

}
