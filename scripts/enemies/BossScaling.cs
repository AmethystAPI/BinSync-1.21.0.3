using Godot;
using Networking;
using System;

public partial class BossScaling : Node2D {
	[Export] public float DifficultyScaling = 1f;

	public override void _Ready() {
		Enemy enemy = GetParent<Enemy>();

		GD.Print(enemy.Health);

		enemy.Health *= Player.Players.Count;

		enemy.Health += Game.Difficulty * DifficultyScaling;

		GD.Print(enemy.Health);
	}
}
