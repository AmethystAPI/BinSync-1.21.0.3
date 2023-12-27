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

		return projectile.Source is Player;
	}

	public void Damage(Projectile projectile)
	{
		if (_destroyed) return;

		if (!Game.IsOwner(projectile)) return;

		Game.SendRpcToAllClients(this, nameof(DamageRpc), MessageSendMode.Reliable, message => { });
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
