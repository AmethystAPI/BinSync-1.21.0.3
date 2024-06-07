using Godot;

public partial class BossScaling : Node2D {
	[Export] public float DifficultyScaling = 1f;

	public override void _Ready() {
		Enemy enemy = GetParent<Enemy>();

		enemy.Health *= Player.Players.Count;

		enemy.Health += Game.Difficulty * DifficultyScaling;
	}
}
