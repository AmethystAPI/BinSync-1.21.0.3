using Godot;

public partial class Exit : Node2D, Damageable
{
	[Export] public Vector2 Direction;

	private bool _locked = true;

	public override void _Ready()
	{
		GetParent().GetParent<Room>().Completed += OnCompleted;
	}

	public bool CanDamage(Projectile projectile)
	{
		if (_locked) return false;

		return projectile.Source is Player;
	}

	public void Damage(Projectile projectile)
	{
		WorldGenerator.PlaceNextRoom(GlobalPosition, Direction);

		QueueFree();
	}

	private void OnCompleted()
	{
		_locked = false;
	}
}
