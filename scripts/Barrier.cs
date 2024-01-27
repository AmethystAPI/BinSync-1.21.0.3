using Godot;

public partial class Barrier : StaticBody2D {
	[Export] public TileMap TileMap;
	[Export] public Vector2 SlideDirection;
	[Export] public TileMap TileMapFade;

	private float _timer;
	private bool _activated = true;

	public override void _Ready() {
		Visible = true;
	}

	public override void _Process(double delta) {
		if (_activated) return;

		_timer += (float)delta;

		TileMap.Position += SlideDirection * (float)delta * 500f;
		TileMap.SetLayerYSortOrigin(0, Mathf.FloorToInt(TileMap.Position.Y) % 16);
		TileMapFade.Modulate = new Color(1, 1, 1, Mathf.Max(1f - _timer, 0));
	}

	public void Deactivate() {
		_activated = false;

		CollisionLayer = 0;
	}
}
