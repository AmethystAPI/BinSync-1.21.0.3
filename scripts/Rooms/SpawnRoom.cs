using Godot;
using Riptide;

public partial class SpawnRoom : Room {
	public override void PlaceEntrance(Vector2 direction) { }

	protected override void EndRpc(Message message) { }

	public override void Place() {
		base.Place();

		CallDeferred(nameof(ImmediateEnd));
	}

	private void ImmediateEnd() {
		float originalDifficulty = Game.Difficulty;

		Complete();

		Game.Difficulty = originalDifficulty;
	}
}
