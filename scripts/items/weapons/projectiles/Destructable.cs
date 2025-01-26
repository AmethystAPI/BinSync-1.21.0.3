using Godot;

public partial class Destructable : Node2D, Damageable {
	[Export] public PackedScene Particle;
	[Export] public int ParticleCount = 6;
	[Export] public bool Invincible = false;
	[Export] public string SoundEffect = "destructable_hit";

	public bool CanDamage(Projectile projectile) {
		return !Invincible;
	}

	public void Damage(Projectile projectile) {
		for (int i = 0; i < ParticleCount; i++) {
			Node2D node = Particle.Instantiate<Node2D>();
			GetParent().AddChild(node);

			node.GlobalPosition = GlobalPosition;
		}

		if (SoundEffect != null)
			Audio.Play(SoundEffect);

		QueueFree();
	}
}
