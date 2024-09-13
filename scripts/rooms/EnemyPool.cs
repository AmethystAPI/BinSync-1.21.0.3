using Godot;

public partial class EnemyPool : Resource {
    [Export] public PackedScene[] EnemyScenes;
    [Export] public float[] Points;
}