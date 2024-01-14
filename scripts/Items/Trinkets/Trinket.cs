using Godot;

public partial class Trinket : Item {
	public override void _Process(double delta) {
		if (!_equipped) return;

		float time = Time.GetTicksMsec() / 1000f;

		float index = _equippingPlayer.EquippedTrinkets.IndexOf(this);
		float offset = Mathf.Pi * 2f * index / _equippingPlayer.EquippedTrinkets.Count;

		Position = new Vector2(12f * Mathf.Cos(time * 0.5f + offset), 12f * Mathf.Sin(time * 0.5f + offset));
	}

	public virtual float ModifySpeed(float speed) {
		return speed;
	}

	public virtual void ModifyProjectile(Weapon weapon, Projectile projectile) {

	}
}
