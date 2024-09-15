using Godot;
using Networking;

public partial class Trinket : Item {
	[Export] public string Id;
	[Export] public Texture2D Icon;
	[Export] public string Description;

	public override void _Ready() {
		base._Ready();
	}

	public override void _Process(double delta) {
		if (!_equipped) return;

		float time = Time.GetTicksMsec() / 1000f;

		float index = _equippingPlayer.EquippedTrinkets.IndexOf(this);
		float offset = Mathf.Pi * 2f * index / _equippingPlayer.EquippedTrinkets.Count;

		Position = new Vector2(12f * Mathf.Cos(time * 0.5f + offset), 12f * Mathf.Sin(time * 0.5f + offset));
	}

	public static void ModifyProjectile(Weapon weapon, Projectile projectile) {
		for (int iteration = 0; iteration < weapon._equippingPlayer.GetTrinketCount("strength"); iteration++) {
			projectile.Damage *= 2f;

			if (projectile is WhipProjectile whipProjectile) whipProjectile.DamageIncrease *= 1.5f;
		}
	}
}
