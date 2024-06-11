public partial class SpawnRoom : Room {
	public override void _Ready() {
		base._Ready();

		Activate();
	}

	protected override void SpawnComponents() {
	}

	public override void SetNextRoom(Room nextRoom) {
		nextRoom.Activate();
	}
}
