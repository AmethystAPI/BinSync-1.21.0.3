using Godot;

public partial class Exit : Node2D, Damageable
{
	[Export] public Vector2 Direction;

	private bool _locked = true;
	private bool _destroyed = false;

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
		if (_destroyed) return;

		if (projectile.GetMultiplayerAuthority() != Multiplayer.GetUniqueId()) return;

		Rpc(nameof(DamageRpc));
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void DamageRpc()
	{
		_destroyed = true;

		WorldGenerator.PlaceNextRoom(GlobalPosition, Direction);

		QueueFree();
	}

	private void OnCompleted()
	{
		_locked = false;
	}
}
