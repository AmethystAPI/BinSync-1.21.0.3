using Godot;
using Networking;

public partial class Trinket : Item {
	[Export] public Area2D EquipArea;

	private bool _animatingToPlayer = false;
	private Player _playerAnimatingTo;

	public override void _Ready() {
		base._Ready();
	}

	public override void _Process(double delta) {
		if (_animatingToPlayer) {
			Position = Position.Lerp(Vector2.Up * 60f, (float)delta * 0.3f);
		}

		if (!_equipped) return;

		float time = Time.GetTicksMsec() / 1000f;

		float index = _equippingPlayer.EquippedTrinkets.IndexOf(this);
		float offset = Mathf.Pi * 2f * index / _equippingPlayer.EquippedTrinkets.Count;

		Position = new Vector2(12f * Mathf.Cos(time * 0.5f + offset), 12f * Mathf.Sin(time * 0.5f + offset));
	}

	public override void _Input(InputEvent @event) {
		if (@event.IsActionPressed("equip")) {
			if (_equipped) return;

			foreach (Node2D body in EquipArea.GetOverlappingBodies()) {
				if (!(body is Player)) continue;

				Player player = (Player)body;

				if (!player.NetworkPoint.IsOwner) continue;

				_animatingToPlayer = false;

				player.Equip(this);

				break;
			}
		}
	}

	public virtual float ModifySpeed(float speed) {
		return speed;
	}

	public virtual void ModifyProjectile(Weapon weapon, Projectile projectile) {

	}

	public void AnimateToPlayer(Player player) {
		Position = Vector2.Up * 100f;

		_animatingToPlayer = true;
		_playerAnimatingTo = player;
	}

	public override void EquipToPlayer(Player player) {
		base.EquipToPlayer(player);

		if (!NetworkManager.IsOwner(player)) return;

		TrinketRealm.LeaveTinketRealm();
	}
}
