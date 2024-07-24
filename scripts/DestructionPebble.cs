using Godot;

public partial class DestructionPebble : CharacterBody2D {
	[Export] public float AirResistance = 3;
	[Export] public float GroundResistance = 5;
	[Export] public Vector2 SpeedRange;
	[Export] public float TimerFactor = 6f;
	[Export] public Sprite2D Sprite;
	[Export] public float Lifetime = 40f;
	[Export] public float DecayFactor = 0.1f;

	private Node2D _arcOrigin;
	private float _heightTimer = 0f;

	private float _life = 40f;

	public override void _Ready() {
		_life = Lifetime;

		RandomNumberGenerator random = new RandomNumberGenerator();

		Velocity = Vector2.Up.Rotated(random.RandfRange(-Mathf.Pi, Mathf.Pi)) * random.RandfRange(SpeedRange.X, SpeedRange.Y);

		_arcOrigin = GetNode<Node2D>("ArcOrigin");

		Node2D sprite = _arcOrigin.GetNode<Node2D>("Sprite");
		sprite.Rotation = random.RandfRange(0f, 2f * Mathf.Pi);

		GetNode<Node2D>("Shadow").Rotation = sprite.Rotation;

		TimerFactor += random.RandfRange(-0.2f, 0.2f);
	}

	public override void _Process(double delta) {
		if (_life <= Lifetime * DecayFactor) {
			Sprite.Modulate = new Color(Sprite.Modulate.R, Sprite.Modulate.G, Sprite.Modulate.B, _life / (Lifetime * DecayFactor));
		}

		_heightTimer += TimerFactor * (float)delta;

		_heightTimer = Mathf.Min(_heightTimer, 1f);

		TimerFactor = Mathf.Lerp(TimerFactor, 0.2f, (float)delta * 6f);

		if (_heightTimer < 1f) {
			Velocity = Velocity.Lerp(Vector2.Zero, AirResistance * (float)delta);
		} else {
			Velocity = Velocity.Lerp(Vector2.Zero, GroundResistance * (float)delta);
		}

		_arcOrigin.Position = Vector2.Up * CalculateHeight(_heightTimer) * 8f;

		MoveAndSlide();

		_life -= (float)delta;

		if (_life <= 0) QueueFree();
	}

	private float CalculateHeight(float t) {
		return -Mathf.Pow(Mathf.Abs(2f * t - 1), 2.2f) + 1;
	}
}
