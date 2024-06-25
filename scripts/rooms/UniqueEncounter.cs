using Godot;

public partial class UniqueEncounter : Resource {
    public class State {
        public UniqueEncounter Source;
        public int Placed = 0;
        public int RoomsTillPlace = 0;
    }

    [Export] public RoomPlacer RoomPlacer;
    [Export] public int Limit = 1;
    [Export] public Vector2I Interval = new Vector2I(3, 6);
    [Export] public int Priority = 1;

    public State GetState(RandomNumberGenerator random) {
        return new State {
            Source = this,
            Placed = 0,
            RoomsTillPlace = random.RandiRange(Interval.X, Interval.Y)
        };
    }
}