using Godot;
using System;
using System.Collections.Generic;

public partial class Player : CharacterBody2D, Damageable
{
	public static List<Player> Players = new List<Player>();

	[Export] public PackedScene DefaultWeaponScene;

	private int _health = 3;
	private bool _dashing = false;
	private Vector2 _dashDirection = Vector2.Right;
	private float _dashTimer;
	private Weapon _equippedWeapon;

	public override void _Ready()
	{
		Players.Add(this);

		if (GetMultiplayerAuthority() != Multiplayer.GetUniqueId()) return;

		EquipWeapon(DefaultWeaponScene);
	}

	public override void _PhysicsProcess(double delta)
	{
		if (GetMultiplayerAuthority() != Multiplayer.GetUniqueId()) return;

		if (!_dashing)
		{
			Vector2 movement = Vector2.Right * Input.GetAxis("move_left", "move_right") + Vector2.Up * Input.GetAxis("move_down", "move_up");

			Velocity = movement.Normalized() * 100f;
		}
		else
		{
			Velocity = _dashDirection * 700f;

			_dashTimer -= (float)delta;

			if (_dashTimer <= 0) _dashing = false;
		}

		MoveAndSlide();

		GetParent().GetNode<Camera2D>("Camera").Position = Position;
	}

	public override void _Input(InputEvent @event)
	{
		if (!@event.IsActionPressed("dash")) return;

		_dashing = true;

		_dashTimer = 0.06f;

		_dashDirection = (GetGlobalMousePosition() - GlobalPosition).Normalized();
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

	public void EquipWeapon(PackedScene weaponScene)
	{
		Rpc(nameof(EquipWeaponRpc), weaponScene.ResourcePath);
	}

	[Rpc(CallLocal = true)]
	private void EquipWeaponRpc(string weaponScenePath)
	{
		PackedScene weaponScene = ResourceLoader.Load<PackedScene>(weaponScenePath);

		if (_equippedWeapon != null) _equippedWeapon.QueueFree();

		Weapon weapon = weaponScene.Instantiate<Weapon>();
		GetNode("WeaponHolder").AddChild(weapon);
		weapon.SetMultiplayerAuthority(GetMultiplayerAuthority());

		_equippedWeapon = weapon;
	}
}
