using Godot;
using Riptide;
using System;
using System.Collections.Generic;

public partial class Player : CharacterBody2D, Damageable, Networking.NetworkNode
{
	public static List<Player> Players = new List<Player>();

	[Export] public PackedScene DefaultWeaponScene;

	private Networking.RpcMap _rpcMap = new Networking.RpcMap();
	public Networking.RpcMap RpcMap => _rpcMap;

	private Networking.SyncedVariable<Vector2> _syncedPosition = new Networking.SyncedVariable<Vector2>(nameof(_syncedPosition), Vector2.Zero, Networking.Authority.Client, false, 50);
	private Networking.SyncedVariable<Vector2> _syncedVelocity = new Networking.SyncedVariable<Vector2>(nameof(_syncedVelocity), Vector2.Zero, Networking.Authority.Client);

	private int _health = 3;
	private bool _dashing = false;
	private Vector2 _dashDirection = Vector2.Right;
	private float _dashTimer;
	private Weapon _equippedWeapon;
	private List<Trinket> _equippedTrinkets = new List<Trinket>();

	public override void _Ready()
	{
		_rpcMap.Register(_syncedPosition, this);
		_rpcMap.Register(_syncedVelocity, this);
		_rpcMap.Register(nameof(EquipWeaponRpc), EquipWeaponRpc);

		Players.Add(this);

		Weapon weapon = DefaultWeaponScene.Instantiate<Weapon>();

		AddChild(weapon);

		if (!Game.IsOwner(this)) return;

		Equip(weapon);
	}

	public override void _Process(double delta)
	{
		_syncedPosition.Sync();
		_syncedVelocity.Sync();

		if (Game.IsOwner(this))
		{
			_syncedPosition.Value = GlobalPosition;
			_syncedVelocity.Value = Velocity;
		}
		else
		{
			GlobalPosition = GlobalPosition.Lerp(_syncedPosition.Value, (float)delta * 20.0f);
			Velocity = _syncedVelocity.Value;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!Game.IsOwner(this)) return;

		if (!_dashing)
		{
			Vector2 movement = Vector2.Right * Input.GetAxis("move_left", "move_right") + Vector2.Up * Input.GetAxis("move_down", "move_up");

			float modifiedSpeed = 100f;
			foreach (Trinket trinket in _equippedTrinkets)
			{
				modifiedSpeed = trinket.ModifySpeed(modifiedSpeed);
			}

			Velocity = movement.Normalized() * modifiedSpeed;
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

	public void Equip(Item item)
	{
		Game.SendRpcToOtherClients(this, nameof(EquipWeaponRpc), MessageSendMode.Reliable, message =>
		{
			message.AddString(item.GetPath());
		});
	}

	private void EquipWeaponRpc(Message message)
	{
		string itemPath = message.GetString();

		Item item = GetNode<Item>(itemPath);

		if (item is Weapon)
		{
			if (_equippedWeapon != null) _equippedWeapon.QueueFree();

			item.GetParent().RemoveChild(item);
			GetNode("WeaponHolder").AddChild(item);
			item.SetMultiplayerAuthority(GetMultiplayerAuthority());

			_equippedWeapon = (Weapon)item;

		}

		if (item is Trinket)
		{
			item.GetParent().RemoveChild(item);
			GetNode("Trinkets").AddChild(item);
			item.SetMultiplayerAuthority(GetMultiplayerAuthority());

			_equippedTrinkets.Add((Trinket)item);
		}

		item.Equip();
	}
}
