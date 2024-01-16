using System;
using Godot;

public partial class AnimatedParticle : GpuParticles2D {
	[Export] public Texture2D[] Textures = new Texture2D[] { };
	[Export] public float FPS = 8;

	private float _timer;

	public override void _Process(double delta) {
		_timer += (float)delta;

		Texture = Textures[Math.Min(Mathf.FloorToInt(_timer * FPS), Textures.Length - 1)];

		if (_timer > Textures.Length * (1f / FPS)) QueueFree();
	}
}
