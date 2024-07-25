using Godot;

[Tool]
public partial class DecorationPool : Resource {
    [Export] public PackedScene[] Scenes;
    [Export] public float Spacing = 100;
    [Export] public float MinimumSpacing = 0;
    [Export] public bool Rotate = false;
}