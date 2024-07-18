#if TOOLS
using Godot;
using System;

[Tool]
public partial class SmartTileMapEditor : EditorPlugin {
	Control dockedScene = null;

	public override void _EnterTree() {
		dockedScene = ResourceLoader.Load<PackedScene>("res://addons/smart_tilemap/TileMapToolEditor.tscn").Instantiate<Control>();

		AddControlToBottomPanel(dockedScene, "Tile Map Tool Editor");
	}

	public override void _ExitTree() {
		RemoveControlFromBottomPanel(dockedScene);

		dockedScene.QueueFree();
	}
}
#endif
