using Godot;
using Riptide;

public partial class SpawnRoom : Room {
	public override void _Ready() {
		base._Ready();
	}
	protected override void EndRpc(Message message) { }
}
