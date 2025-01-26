using Godot;

public partial class Barrier : StaticBody2D {
	[Export] public Destructable[] Destructables = new Destructable[0];

	public void Deactivate() {
		CollisionLayer = 0;

		foreach (Node node in GetChildren()) {
			if (!(node is Destructable)) continue;

			((Destructable)node).Invincible = false;
			((Destructable)node).SoundEffect = null;
		}

		foreach (Destructable destructable in Destructables) {
			destructable.Damage(null);
		}
	}
}
