using Godot;

public partial class EnemyPool {
    public record Entry(PackedScene Scene, float Points);

    public Entry[] Entries;
}