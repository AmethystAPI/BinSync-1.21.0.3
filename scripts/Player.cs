using Godot;
using System;
using System.Collections.Generic;

public partial class Player : CharacterBody2D, Damageable, Networking.NetworkNode
{
	public static List<Player> Players = new List<Player>();

	[Export] public PackedScene DefaultWeaponScene;
	[Export] public bool IsLocal;

	private Networking.RpcMap _rpcMap = new Networking.RpcMap();
	public Networking.RpcMap RpcMap => _rpcMap;

	private Networking.SyncedVariable<Vector2> _syncedPosition = new Networking.SyncedVariable<Vector2>(nameof(_syncedPosition), Vector2.Zero, Networking.Authority.Client, false, 50);

	private int _health = 3;
	private bool _dashing = false;
	private Vector2 _dashDirection = Vector2.Right;
	private float _dashTimer;
	private Weapon _equippedWeapon;

	public override void _Ready()
	{
		_rpcMap.Register(_syncedPosition, this);

		Players.Add(this);

		// Weapon weapon = DefaultWeaponScene.Instantiate<Weapon>();

		// AddChild(weapon);

		// IsLocal = GetMultiplayerAuthority() == Multiplayer.GetUniqueId();

		// if (GetMultiplayerAuthority() != Multiplayer.GetUniqueId()) return;

		// EquipWeapon(weapon);
	}

	public override void _Process(double delta)
	{
		if (Game.IsOwner(this))
		{
			_syncedPosition.Value = GlobalPosition;
		}
		else
		{
			GlobalPosition = _syncedPosition.Value;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		_syncedPosition.Sync();

		if (!Game.IsOwner(this)) return;

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

		// Rpc(nameof(SetVelocityRpc), Velocity);

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

	public void EquipWeapon(Weapon weapon)
	{
		Rpc(nameof(EquipWeaponRpc), weapon.GetPath());
	}

	[Rpc(CallLocal = true)]
	private void EquipWeaponRpc(string weaponPath)
	{
		Weapon weapon = GetNode<Weapon>(weaponPath);

		GD.Print("Equipping " + weaponPath + " " + weapon.GetMultiplayerAuthority() + " " + Multiplayer.GetUniqueId());

		if (_equippedWeapon != null) _equippedWeapon.QueueFree();

		weapon.GetParent().RemoveChild(weapon);

		AddChild(weapon);

		weapon.GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer");

		// GetNode("WeaponHolder").AddChild(weapon);
		weapon.SetMultiplayerAuthority(GetMultiplayerAuthority());

		_equippedWeapon = weapon;

		weapon.Equip();
	}

	[Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
	private void SetVelocityRpc(Vector2 velocity)
	{
		Velocity = velocity;
	}
}
