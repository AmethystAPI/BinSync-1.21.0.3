using Godot;

public partial class DestructionLeaf : CharacterBody2D {
	[Export] public float Resistance;
	[Export] public Vector2 SpeedRange;

	private Node2D _floatOrigin;
	private Node2D _swingOrigin;
	private float _heightTimer = 0f;
	private float _swingTimer = 0f;
	private float _timerFactor = 3f;

	private float _life = 40f;

	public override void _Ready() {
		RandomNumberGenerator random = new RandomNumberGenerator();

		Velocity = Vector2.Up.Rotated(random.RandfRange(-Mathf.Pi, Mathf.Pi)) * random.RandfRange(SpeedRange.X, SpeedRange.Y);

		_floatOrigin = GetNode<Node2D>("FloatOrigin");
		_swingOrigin = _floatOrigin.GetNode<Node2D>("SwingOrigin");

		Node2D sprite = _swingOrigin.GetNode<Node2D>("Sprite");
		sprite.Rotation = random.RandfRange(0f, 2f * Mathf.Pi);

		GetNode<Node2D>("Shadow").Rotation = sprite.Rotation;

		_timerFactor += random.RandfRange(-0.2f, 0.2f);
		_swingTimer += random.RandfRange(-1f, 1f);
	}

	public override void _Process(double delta) {
		_heightTimer += _timerFactor * (float)delta;
		_swingTimer += (float)delta;

		_heightTimer = Mathf.Min(_heightTimer, 1f);

		_timerFactor = Mathf.Lerp(_timerFactor, 0.2f, (float)delta * 6f);

		Velocity = Velocity.Lerp(Vector2.Zero, Resistance * (float)delta);

		_floatOrigin.Position = Vector2.Up * Mathf.Sin(_heightTimer * Mathf.Pi) * 16f;

		float sinValue = Mathf.Sin(_swingTimer * 2f);
		_swingOrigin.Rotation = Mathf.Pow(Mathf.Abs(sinValue), 0.9f) * Mathf.Sign(sinValue) * Mathf.Pi / 4f * Mathf.Pow(Mathf.Max(-_floatOrigin.Position.Y, 0f) / 16f, 0.8f);

		MoveAndSlide();

		_life -= (float)delta;

		if (_life <= 0) QueueFree();
	}
}
