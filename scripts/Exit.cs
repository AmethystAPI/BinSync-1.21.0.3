using Godot;
using Riptide;

public partial class Exit : Node2D, Damageable, Networking.NetworkNode
{
	[Export] public Vector2 Direction;

	private Networking.RpcMap _rpcMap = new Networking.RpcMap();
	public Networking.RpcMap RpcMap => _rpcMap;

	private bool _locked = true;
	private bool _destroyed = false;

	public override void _Ready()
	{
		_rpcMap.Register(nameof(DamageRpc), DamageRpc);

		GetParent().GetParent<Room>().Completed += OnCompleted;
	}

	public bool CanDamage(Projectile projectile)
	{
		if (_locked) return false;

		if (_destroyed) return false;

		if (!(projectile.Source is Player)) return false;

		return true;
	}

	public void Damage(Projectile projectile)
	{
		if (!Game.IsOwner(projectile)) return;

		Game.BounceRpcToClients(this, nameof(DamageRpc), MessageSendMode.Reliable, message => { });
	}

	public void DamageRpc(Message message)
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
