using Godot;
using Networking;
using Riptide;
using System.Collections.Generic;

public partial class Player : CharacterBody2D, Damageable, NetworkPointUser
{
	public static List<Player> Players = new List<Player>();
	public static List<Player> AlivePlayers = new List<Player>();

	[Export] public PackedScene DefaultWeaponScene;

	public List<Trinket> EquippedTrinkets = new List<Trinket>();
	public float Health = 3f;
	public NetworkPoint NetworkPoint { get; set; } = new NetworkPoint();

	private NetworkedVariable<Vector2> _networkedPosition = new NetworkedVariable<Vector2>(Vector2.Zero);
	private NetworkedVariable<Vector2> _networkedVelocity = new NetworkedVariable<Vector2>(Vector2.Zero);

	private bool _dashing = false;
	private Vector2 _dashDirection = Vector2.Right;
	private float _dashTimer;
	private Weapon _equippedWeapon;
	private AnimationPlayer _animationPlayer;
	private float _angelAngle;
	private float _angelSwapTimer;
	private int _angelTurn = 1;
	private RandomNumberGenerator _randomNumberGenerator = new RandomNumberGenerator();
	private Area2D _ressurectArea;

	public override void _Ready()
	{
		NetworkPoint.Setup(this);

		NetworkPoint.Register(nameof(_networkedPosition), _networkedPosition);
		NetworkPoint.Register(nameof(_networkedVelocity), _networkedVelocity);
		NetworkPoint.Register(nameof(EquipWeaponRpc), EquipWeaponRpc);
		NetworkPoint.Register(nameof(UpdateHealthRpc), UpdateHealthRpc);
		NetworkPoint.Register(nameof(ReviveRpc), ReviveRpc);

		Players.Add(this);
		AlivePlayers.Add(this);

		_animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		_ressurectArea = GetNode<Area2D>("RessurectArea");

		// Weapon weapon = DefaultWeaponScene.Instantiate<Weapon>();
		// Game.NameSpawnedNetworkNode("Weapon", weapon);

		// AddChild(weapon);

		if (!NetworkPoint.IsOwner) return;

		GameUI.UpdateHealth(Health);

		// Equip(weapon);
	}

	public override void _Process(double delta)
	{
		if (Health > 0)
		{
			_animationPlayer.Play("idle");
		}
		else
		{
			_animationPlayer.Play("dead");
		}

		_networkedPosition.Sync();
		_networkedVelocity.Sync();

		if (NetworkPoint.IsOwner)
		{
			_networkedPosition.Value = GlobalPosition;
			_networkedVelocity.Value = Velocity;
		}
		else
		{
			GlobalPosition = GlobalPosition.Lerp(_networkedPosition.Value, (float)delta * 20.0f);
			Velocity = _networkedVelocity.Value;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!NetworkPoint.IsOwner) return;

		if (Health <= 0)
		{
			_angelSwapTimer -= (float)delta;

			if (_angelSwapTimer <= 0)
			{
				_angelSwapTimer = _randomNumberGenerator.RandfRange(0.8f, 1.2f);

				if (_randomNumberGenerator.RandfRange(0f, 1f) < 0.5f) _angelTurn = -_angelTurn;
			}

			_angelAngle += Mathf.Pi * (float)delta * _angelTurn;

			Velocity = Vector2.Right.Rotated(_angelAngle) * 50f;
		}
		else if (!_dashing)
		{
			// Vector2 movement = Vector2.Right * Input.GetAxis("move_left", "move_right") + Vector2.Up * Input.GetAxis("move_down", "move_up");

			// float modifiedSpeed = 100f;
			// foreach (Trinket trinket in EquippedTrinkets)
			// {
			// 	modifiedSpeed = trinket.ModifySpeed(modifiedSpeed);
			// }

			// Velocity = movement.Normalized() * modifiedSpeed;
		}
		else
		{
			foreach (Node2D body in _ressurectArea.GetOverlappingBodies())
			{
				if (!(body is Player)) continue;

				if (body == this) continue;

				Player player = (Player)body;

				if (player.Health > 0) continue;

				// Game.BounceRpcToClients(body, nameof(ReviveRpc), MessageSendMode.Reliable, message => { });
			}
		}

		GetParent().GetNode<Camera2D>("Camera").Position = Position;
	}

	public override void _Input(InputEvent @event)
	{
		if (!@event.IsActionPressed("dash")) return;

		// _dashing = true;

		// _dashTimer = 0.06f;

		// _dashDirection = (GetGlobalMousePosition() - GlobalPosition).Normalized();
	}

	public void ModifyHealth(float change)
	{
		Health += change;

		if (Health > 3) Health = 3;

		if (Health <= 0) Die();

		if (!NetworkPoint.IsOwner) return;

		// Game.BounceRpcToClients(this, nameof(UpdateHealthRpc), MessageSendMode.Reliable, message =>
		// {
		// 	message.AddFloat(Health);
		// });

		GameUI.UpdateHealth(Health);
	}

	public void SetHealth(float health)
	{
		Health = health;

		if (Health > 3) Health = 3;

		if (Health <= 0) Die();

		if (!NetworkPoint.IsOwner) return;

		// Game.BounceRpcToClients(this, nameof(UpdateHealthRpc), MessageSendMode.Reliable, message =>
		// {
		// 	message.AddFloat(Health);
		// });

		GameUI.UpdateHealth(Health);
	}

	public bool CanDamage(Projectile projectile)
	{
		if (projectile.Source is Player) return false;

		if (Health <= 0) return false;

		return true;
	}

	public void Damage(Projectile projectile)
	{
		if (!NetworkPoint.IsOwner) return;

		ModifyHealth(-projectile.Damage);
	}

	public void Equip(Item item)
	{
		// Game.BounceRpcToClients(this, nameof(EquipWeaponRpc), MessageSendMode.Reliable, message =>
		// {
		// 	message.AddString(item.GetPath());
		// });
	}

	public void Cleanup()
	{
		Players.Remove(this);

		QueueFree();
	}

	private void Die()
	{
		AlivePlayers.Remove(this);

		if (AlivePlayers.Count != 0) return;

		if (!NetworkManager.IsHost) return;

		Game.Restart();
	}

	private void UpdateHealthRpc(Message message)
	{
		if (NetworkPoint.IsOwner) return;

		float newHealth = message.GetFloat();

		SetHealth(newHealth);
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

			EquippedTrinkets.Add((Trinket)item);
		}

		item.Equip(this);
	}

	private void ReviveRpc(Message message)
	{
		if (Health > 0) return;

		AlivePlayers.Add(this);

		SetHealth(1f);
	}
}
