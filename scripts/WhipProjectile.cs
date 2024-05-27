using Godot;
using System;

public partial class WhipProjectile : Projectile {

	[Export] public float DamageIncrease = 8f;

	private float _damage;

	public override void _Ready() {
		base._Ready();

		_damage = Damage;
	}

	public override void _Process(double delta) {
		base._Process(delta);

		_damage += DamageIncrease * (float)delta;
	}

	public override float GetDamage() {
		return _damage;
	}
}
