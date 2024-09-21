#if TOOLS
using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

[Tool]
public partial class RoomTool : EditorPlugin {
	private enum Mode {
		Connections,
		Bounds
	}

	private HashSet<Vector2> _connections = new HashSet<Vector2>();
	private List<Vector2> _bounds = new List<Vector2>();
	private Mode _mode = Mode.Connections;
	private string _selectedPath;

	private RoomLayoutGizmo _roomLayoutGizmo;
	private Control _gui;


	public override void _SaveExternalData() {
		if (_roomLayoutGizmo == null) return;
		if (_bounds.Count < 2) return;

		RoomLayout roomLayout = new RoomLayout();
		roomLayout.Connections = _connections.ToArray();
		roomLayout.TopLeftBound = new Vector2(Mathf.Min(_bounds[0].X, _bounds[1].X), Mathf.Min(_bounds[0].Y, _bounds[1].Y));
		roomLayout.BottomRightBound = new Vector2(Mathf.Max(_bounds[0].X, _bounds[1].X), Mathf.Max(_bounds[0].Y, _bounds[1].Y));

		if (ResourceLoader.Exists(GetRoomLayoutPath()))
			File.Delete(ProjectSettings.GlobalizePath(GetRoomLayoutPath()));

		ResourceSaver.Save(roomLayout, GetRoomLayoutPath());
		roomLayout.TakeOverPath(GetRoomLayoutPath());
	}

	public override void _EnterTree() {
		EditorInterface.Singleton.GetSelection().SelectionChanged += SelectionChanged;
	}

	public override void _ExitTree() {
		EditorInterface.Singleton.GetSelection().SelectionChanged -= SelectionChanged;
	}

	private void SelectionChanged() {
		Array<Node> nodes = EditorInterface.Singleton.GetSelection().GetSelectedNodes();

		if (_roomLayoutGizmo != null && IsInstanceValid(_roomLayoutGizmo)) {
			_roomLayoutGizmo.Free();
			_roomLayoutGizmo = null;
		}

		if (_gui != null && IsInstanceValid(_gui)) {
			_gui.Free();
			_gui = null;
		}

		if (nodes.Count != 1) return;

		Node node = nodes[0];

		if (!node.SceneFilePath.StartsWith("res://content/rooms/")) return;

		_selectedPath = node.SceneFilePath;

		_roomLayoutGizmo = ResourceLoader.Load<PackedScene>("res://addons/room_tool/room_layout_gizmo.tscn").Instantiate<RoomLayoutGizmo>();
		node.AddChild(_roomLayoutGizmo);

		_gui = ResourceLoader.Load<PackedScene>("res://addons/room_tool/gui.tscn").Instantiate<Control>();
		node.AddChild(_gui);

		if (ResourceLoader.Exists(GetRoomLayoutPath())) {
			RoomLayout roomLayout = ResourceLoader.Load<RoomLayout>(GetRoomLayoutPath());

			_connections = new HashSet<Vector2>(roomLayout.Connections);
			_bounds = new List<Vector2>() { roomLayout.TopLeftBound, roomLayout.BottomRightBound };
		}

		UpdateGizmo();
	}

	public override bool _Handles(GodotObject @object) {
		if (!(@object is Node node)) return false;

		if (!node.SceneFilePath.StartsWith("res://content/rooms/")) return false;

		return true;

	}

	public override bool _ForwardCanvasGuiInput(InputEvent @event) {
		if (_roomLayoutGizmo == null || !IsInstanceValid(_roomLayoutGizmo)) return false;

		if (@event is InputEventMouseMotion mouseMotionEvent) {
			Vector2 position = _gui.GetViewportTransform().AffineInverse() * mouseMotionEvent.Position;
			_gui.GlobalPosition = position;

			return true;
		}

		if (@event is InputEventMouseButton mouseEvent) {
			if (!mouseEvent.Pressed) return true;

			if (mouseEvent.ButtonIndex == MouseButton.Left) {

				Vector2 position = _roomLayoutGizmo.GetViewportTransform().AffineInverse() * mouseEvent.Position;

				if (_mode == Mode.Connections) {
					_connections.Add((position / 16f).Round() * 16f);
				}

				if (_mode == Mode.Bounds) {
					_bounds.Add((position / 16f).Round() * 16f);

					if (_bounds.Count > 2) _bounds.RemoveAt(0);
				}

				EditorInterface.Singleton.MarkSceneAsUnsaved();

				UpdateGizmo();
			}

			if (mouseEvent.ButtonIndex == MouseButton.Right) {

				Vector2 position = _roomLayoutGizmo.GetViewportTransform().AffineInverse() * mouseEvent.Position;

				if (_mode == Mode.Connections) {
					if (_connections.Contains((position / 16f).Round() * 16f))
						_connections.Remove((position / 16f).Round() * 16f);
				}

				if (_mode == Mode.Bounds) {
					if (_bounds.Contains((position / 16f).Round() * 16f))
						_bounds.Remove((position / 16f).Round() * 16f);
				}

				EditorInterface.Singleton.MarkSceneAsUnsaved();

				UpdateGizmo();
			}

			return true;
		}

		if (@event is InputEventKey keyEvent) {
			if (keyEvent.Keycode != Key.F) return false;
			if (!keyEvent.Pressed) return true;

			if (_mode == Mode.Connections) {
				_mode = Mode.Bounds;
				_gui.GetNode<Label>("ModeLabel").Text = "Mode: Bounds";
			} else if (_mode == Mode.Bounds) {
				_mode = Mode.Connections;
				_gui.GetNode<Label>("ModeLabel").Text = "Mode: Connections";
			}

			return true;
		}

		return false;
	}

	private void UpdateGizmo() {
		_roomLayoutGizmo.Connections = _connections;
		_roomLayoutGizmo.Bounds = _bounds;

		_roomLayoutGizmo.QueueRedraw();
	}

	private string GetRoomLayoutPath() {
		string relativePath = _selectedPath.Substring("res://content/rooms/".Length);
		string fileName = Path.GetFileName(relativePath);
		string relativeFolders = relativePath.Substring(0, relativePath.Length - fileName.Length);
		string saveRelativePath = relativeFolders + "room_layout_" + Path.GetFileNameWithoutExtension(relativePath) + ".tres";

		return "res://generated/rooms/" + saveRelativePath;
	}
}
#endif
