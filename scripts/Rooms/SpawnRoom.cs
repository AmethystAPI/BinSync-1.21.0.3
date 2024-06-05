public partial class SpawnRoom : Room {
	public override void _Ready() {
		base._Ready();

		Activate();
	}

	internal override void SpawnComponents() {
	}

	internal override void SetNextRoom(Room nextRoom) {
		nextRoom.Activate();
	}
}
