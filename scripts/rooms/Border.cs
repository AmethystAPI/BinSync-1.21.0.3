using Godot;

[Tool]
public partial class Border : Resource {
    [Export] public Texture2D[] Textures = new Texture2D[0];
    [Export] public float[] Spacings = new float[0];
    [Export] public Vector2[] Variances = new Vector2[0];
    [Export] public float[] Offsets = new float[0];
    [Export] public int[] Layers = new int[0];
    [Export] public Vector3I Tile;
}