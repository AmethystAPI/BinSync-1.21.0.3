using Godot;
using Networking;
using Riptide;
using System.Collections.Generic;

public partial class Player : CharacterBody2D, Damageable, NetworkPointUser {
	public static List<Player> Players = new List<Player>();
	public static List<Player> AlivePlayers = new List<Player>();
	public static Player LocalPlayer;
	public static PackedScene HatCosmetic;
	public static PackedScene BodyCosmetic;

	[Export] public PackedScene[] StarterWeaponScenes;
	[Export] public Node2D Visuals;
	[Export] public Node2D WeaponHolder;
	[Export] public Node2D TrinketHolder;
	[Export] public Node2D EquipmentHolder;
	[Export] public PackedScene DamageNumber;

	[Export] public PackedScene[] DebugStarterTrinketScenes;

	public float Health = 3f;
	public Vector2 Knockback;
	public List<Trinket> EquippedTrinkets = new List<Trinket>();
	public Dictionary<string, Equipment> EquippedEquipments = new Dictionary<string, Equipment>();

	public NetworkPoint NetworkPoint { get; set; } = new NetworkPoint();

	public AnimationPlayer AnimationPlayer;
	public SquashAndStretch SquashAndStretch;

	private NetworkedVariable<Vector2> _networkedPosition = new NetworkedVariable<Vector2>(Vector2.Zero);
	private NetworkedVariable<Vector2> _networkedVelocity = new NetworkedVariable<Vector2>(Vector2.Zero);
	private NetworkedVariable<Vector2> _networkedFacing = new NetworkedVariable<Vector2>(Vector2.Zero);

	private Weapon _equippedWeapon;

	private StateMachine _stateMachine = new StateMachine("normal");

	public override void _Ready() {
		NetworkPoint.Setup(this);

		NetworkPoint.Register(nameof(_networkedPosition), _networkedPosition);
		NetworkPoint.Register(nameof(_networkedVelocity), _networkedVelocity);
		NetworkPoint.Register(nameof(_networkedFacing), _networkedFacing);
		NetworkPoint.Register(nameof(EquipItemRpc), EquipItemRpc);
		NetworkPoint.Register(nameof(DamageRpc), DamageRpc);
		NetworkPoint.Register(nameof(DieRpc), DieRpc);
		NetworkPoint.Register(nameof(ReviveRpc), ReviveRpc);
		NetworkPoint.Register(nameof(EnterTrinketRealmRpc), EnterTrinketRealmRpc);
		NetworkPoint.Register(nameof(LeaveTrinketRealmRpc), LeaveTrinketRealmRpc);
		NetworkPoint.Register(nameof(EquipCosmeticRpc), EquipCosmeticRpc);
		NetworkPoint.Register(nameof(EquipStarterItemsRpc), EquipStarterItemsRpc);

		Players.Add(this);
		AlivePlayers.Add(this);

		AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		SquashAndStretch = GetNode<SquashAndStretch>("SquashAndStretch");

		_stateMachine.Add(new PlayerNormal("normal", this));
		_stateMachine.Add(new PlayerDash("dash", this));
		_stateMachine.Add(new PlayerAngel("angel", this));
		_stateMachine._Ready();

		if (!NetworkPoint.IsOwner) return;

		LocalPlayer = this;

		GameUI.UpdateHealth(Health);

		if (OS.HasFeature("host")) HatCosmetic = ResourceLoader.Load<PackedScene>("res://scenes/items/equipment/OG_hat.tscn");

		NetworkPoint.BounceRpcToClients(nameof(EquipStarterItemsRpc), message => message.AddString(StarterWeaponScenes[new RandomNumberGenerator().RandiRange(0, StarterWeaponScenes.Length - 1)].ResourcePath));
		NetworkPoint.BounceRpcToClients(nameof(EquipCosmeticRpc), message => message.AddString(HatCosmetic.ResourcePath));
		NetworkPoint.BounceRpcToClients(nameof(EquipCosmeticRpc), message => message.AddString(BodyCosmetic.ResourcePath));
	}

	public override void _Process(double delta) {
		_stateMachine._Process(delta);

		Knockback = Knockback.Lerp(Vector2.Zero, (float)delta * 12f);

		_networkedPosition.Sync();
		_networkedVelocity.Sync();
		_networkedFacing.Sync();

		if (NetworkPoint.IsOwner) {
			if (_stateMachine.CurrentState != "Hurt") Interactables.ActivateClosest(this);

			_networkedPosition.Value = GlobalPosition;
			_networkedVelocity.Value = Velocity;
			_networkedFacing.Value = GetGlobalMousePosition() - GlobalPosition;
		} else {
			GlobalPosition = GlobalPosition.Lerp(_networkedPosition.Value, (float)delta * 20.0f);
			Velocity = _networkedVelocity.Value;

			if (GetParent() == TrinketRealm.Me) GlobalPosition = LocalPlayer.GlobalPosition + Vector2.Up * 1000f;
		}

		Visuals.Scale = _networkedFacing.Value.X >= 0 ? Vector2.One : new Vector2(-1, 1);
	}

	public override void _PhysicsProcess(double delta) {
		_stateMachine._PhysicsProcess(delta);
	}

	public override void _Input(InputEvent @event) {
		_stateMachine._Input(@event);
	}

	public void Heal(float health) {
		if (!NetworkPoint.IsOwner) return;

		Health += health;

		if (Health > 3) Health = 3;

		GameUI.UpdateHealth(Health);

		DamageNumber damageNumber = DamageNumber.Instantiate<DamageNumber>();
		damageNumber.Damage = health;
		damageNumber.Color = new Color(105f / 255f, 255f / 255f, 92f / 255f);

		GetParent().AddChild(damageNumber);

		damageNumber.GlobalPosition = GlobalPosition + Vector2.Up * 8f;
	}

	public bool CanDamage(Projectile projectile) {
		if (projectile.Source is Player) return false;

		if (Health <= 0) return false;

		if (Knockback.Length() >= 1f) return false;

		if (_stateMachine.CurrentState == "dash") return false;

		// TODO: DEBUG
		// return false;

		return true;
	}

	public void Damage(Projectile projectile) {
		if (!NetworkPoint.IsOwner) return;

		float damage = projectile.GetDamage();
		Health -= damage;

		GameUI.UpdateHealth(Health);

		if (Health <= 0) Die();

		NetworkPoint.BounceRpcToClients(nameof(DamageRpc), message => {
			Vector2 knockback = projectile.GlobalTransform.BasisXform(Vector2.Right) * 200f * projectile.Knockback;

			message.AddFloat(knockback.X);
			message.AddFloat(knockback.Y);
			message.AddFloat(damage);
		});
	}

	public void Die() {
		Health = 0;

		GameUI.UpdateHealth(Health);

		AlivePlayers.Remove(this);

		NetworkPoint.BounceRpcToClients(nameof(DieRpc));

		_stateMachine.GoToState("angel");

		_equippedWeapon.CancelShoot();
	}

	public void Revive() {
		NetworkPoint.BounceRpcToClients(nameof(ReviveRpc));
	}

	public void Equip(Item item) {
		NetworkPoint.BounceRpcToClients(nameof(EquipItemRpc), message => {
			message.AddString(item.GetPath());
		});
	}

	private void EquipStarterItemsRpc(Message message) {
		string path = message.GetString();

		PackedScene weaponScene = ResourceLoader.Load<PackedScene>(path);
		Weapon weapon = NetworkManager.SpawnNetworkSafe<Weapon>(weaponScene, "Weapon");

		AddChild(weapon);

		if (NetworkPoint.IsOwner) Equip(weapon);

		foreach (PackedScene scene in DebugStarterTrinketScenes) {
			Trinket trinket = NetworkManager.SpawnNetworkSafe<Trinket>(scene, "Trinket");

			AddChild(trinket);

			if (NetworkPoint.IsOwner) Equip(trinket);
		}
	}

	public void Cleanup() {
		Players.Remove(this);

		QueueFree();
	}

	public void EnterTrinketRealm() {
		NetworkPoint.BounceRpcToClients(nameof(EnterTrinketRealmRpc));
	}

	private void EnterTrinketRealmRpc(Message message) {
		if (NetworkPoint.IsOwner) return;
	}

	public void LeaveTrinketRealm() {
		NetworkPoint.BounceRpcToClients(nameof(LeaveTrinketRealmRpc));
	}

	private void LeaveTrinketRealmRpc(Message message) {
		if (NetworkPoint.IsOwner) return;
	}

	private void DamageRpc(Message message) {
		Knockback = new Vector2(message.GetFloat(), message.GetFloat());

		AnimationPlayer.Play("hurt");

		SquashAndStretch.Trigger(new Vector2(1.4f, 0.6f), 10f);

		Camera.Shake(2f);

		DamageNumber damageNumber = DamageNumber.Instantiate<DamageNumber>();
		damageNumber.Damage = message.GetFloat();

		// if (Health <= 0f) damageNumber.Color = new Color(0f, 0f, 0f);

		GetParent().AddChild(damageNumber);

		damageNumber.GlobalPosition = GlobalPosition + Vector2.Up * 8f;

		foreach (Equipment equipment in EquippedEquipments.Values) {
			equipment.AnimationPlayer.Play("hurt");
		}

		Audio.Play("player_damage");
	}

	private void DieRpc(Message message) {
		WeaponHolder.Visible = false;
		TrinketHolder.Visible = false;

		if (!NetworkPoint.IsOwner) {
			Health = 0;

			AlivePlayers.Remove(this);

			_stateMachine.GoToState("angel");
		}

		if (AlivePlayers.Count != 0) return;

		if (!NetworkManager.IsHost) return;

		Game.Restart();
	}

	private void EquipItemRpc(Message message) {
		string itemPath = message.GetString();

		Item item = GetNode<Item>(itemPath);

		if (item is Weapon weapon) {
			if (_equippedWeapon != null) _equippedWeapon.QueueFree();

			item.GetParent().RemoveChild(item);
			WeaponHolder.AddChild(item);
			item.SetMultiplayerAuthority(GetMultiplayerAuthority());

			_equippedWeapon = weapon;
		}

		if (item is Trinket trinket) {
			item.GetParent().RemoveChild(item);
			TrinketHolder.AddChild(item);
			item.SetMultiplayerAuthority(GetMultiplayerAuthority());

			EquippedTrinkets.Add(trinket);
		}

		if (item is Equipment equipment) {
			item.GetParent().RemoveChild(item);
			EquipmentHolder.AddChild(item);
			item.SetMultiplayerAuthority(GetMultiplayerAuthority());

			if (EquippedEquipments.ContainsKey(equipment.Slot)) EquippedEquipments[equipment.Slot].QueueFree();

			EquippedEquipments[equipment.Slot] = equipment;
		}

		item.OnEquipToPlayer(this);
	}

	private void ReviveRpc(Message message) {
		WeaponHolder.Visible = true;
		TrinketHolder.Visible = true;


		_stateMachine.GoToState("normal");

		AlivePlayers.Add(this);

		Health = 1;

		if (!NetworkPoint.IsOwner) return;

		GameUI.UpdateHealth(Health);
	}

	private void EquipCosmeticRpc(Message message) {
		string path = message.GetString();

		PackedScene scene = ResourceLoader.Load<PackedScene>(path);

		Equipment equipment = NetworkManager.SpawnNetworkSafe<Equipment>(scene, "Equipment");

		AddChild(equipment);

		if (NetworkPoint.IsOwner) Equip(equipment);
	}
}
