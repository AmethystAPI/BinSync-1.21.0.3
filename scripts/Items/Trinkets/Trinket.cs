using Godot;
using Networking;

public partial class Trinket : Item {
	private Area2D _equipArea;

	public override void _Ready() {
		base._Ready();

		_equipArea = GetNode<Area2D>("EquipArea");
	}

	public override void _Process(double delta) {
		if (!_equipped) return;

		float time = Time.GetTicksMsec() / 1000f;

		float index = _equippingPlayer.EquippedTrinkets.IndexOf(this);
		float offset = Mathf.Pi * 2f * index / _equippingPlayer.EquippedTrinkets.Count;

		Position = new Vector2(12f * Mathf.Cos(time * 0.5f + offset), 12f * Mathf.Sin(time * 0.5f + offset));
	}

	public override void _Input(InputEvent @event) {
		base._Input(@event);

		if (@event.IsActionReleased("equip") && !_equipped) {
			foreach (Node2D body in _equipArea.GetOverlappingBodies()) {
				if (!(body is Player)) continue;

				if (!NetworkManager.IsOwner(body)) continue;

				((Player)body).Equip(this);
			}
		}
	}

	public virtual float ModifySpeed(float speed) {
		return speed;
	}

	public virtual void ModifyProjectile(Weapon weapon, Projectile projectile) {

	}

	public override void EquipToPlayer(Player player) {
		base.EquipToPlayer(player);
	}
}
