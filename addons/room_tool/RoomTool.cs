#if TOOLS
using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

[Tool]
public partial class RoomTool : EditorPlugin {
	private enum Mode {
		Connections,
		Bounds
	}

	private HashSet<RoomLayout.Connection> _connections = new HashSet<RoomLayout.Connection>();
	private List<Vector2> _bounds = new List<Vector2>();
	private Mode _mode = Mode.Connections;
	private Vector2 _direction = Vector2.Right;
	private string _selectedPath;

	private RoomLayoutGizmo _roomLayoutGizmo;
	private Control _gui;


	public override void _SaveExternalData() {
		if (_roomLayoutGizmo == null) return;
		if (_bounds.Count < 2) return;

		RoomLayout roomLayout = new RoomLayout();
		roomLayout.SetConnections(_connections.ToArray());
		roomLayout.TopLeftBound = new Vector2(Mathf.Min(_bounds[0].X, _bounds[1].X), Mathf.Min(_bounds[0].Y, _bounds[1].Y));
		roomLayout.BottomRightBound = new Vector2(Mathf.Max(_bounds[0].X, _bounds[1].X), Mathf.Max(_bounds[0].Y, _bounds[1].Y));

		List<Vector2> walls = new List<Vector2>();

		TileMapLayer wallsLayer = _roomLayoutGizmo.GetParent().GetNode<TileMapLayer>("Walls");
		for (int x = (int)roomLayout.TopLeftBound.X; x < (int)roomLayout.BottomRightBound.X; x++) {
			for (int y = (int)roomLayout.TopLeftBound.Y; y < (int)roomLayout.BottomRightBound.Y; y++) {
				TileData tileData = wallsLayer.GetCellTileData(new Vector2I(x, y));

				if (tileData == null) continue;

				walls.Add(new Vector2(x, y));
			}
		}

		roomLayout.Walls = walls.ToArray();

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

			_connections = new HashSet<RoomLayout.Connection>(roomLayout.GetConnections());
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
					_connections.Add(new RoomLayout.Connection {
						Location = (position / 16f).Round(),
						Direction = _direction
					});
				}

				if (_mode == Mode.Bounds) {
					_bounds.Add((position / 16f).Round());

					if (_bounds.Count > 2) _bounds.RemoveAt(0);
				}

				EditorInterface.Singleton.MarkSceneAsUnsaved();

				UpdateGizmo();
			}

			if (mouseEvent.ButtonIndex == MouseButton.Right) {

				Vector2 position = _roomLayoutGizmo.GetViewportTransform().AffineInverse() * mouseEvent.Position;

				if (_mode == Mode.Connections) {
					RoomLayout.Connection connection = new RoomLayout.Connection {
						Location = (position / 16f).Round(),
						Direction = _direction
					};

					if (_connections.Contains(connection))
						_connections.Remove(connection);
				}

				if (_mode == Mode.Bounds) {
					if (_bounds.Contains((position / 16f).Round()))
						_bounds.Remove((position / 16f).Round());
				}

				EditorInterface.Singleton.MarkSceneAsUnsaved();

				UpdateGizmo();
			}

			return true;
		}

		if (@event is InputEventKey keyEvent) {
			if (keyEvent.Keycode != Key.F && keyEvent.Keycode != Key.E) return false;
			if (!keyEvent.Pressed) return true;

			if (keyEvent.Keycode == Key.F) {
				if (_mode == Mode.Connections) {
					_mode = Mode.Bounds;
					_gui.GetNode<Label>("ModeLabel").Text = "Mode: Bounds";
				} else if (_mode == Mode.Bounds) {
					_mode = Mode.Connections;
					_gui.GetNode<Label>("ModeLabel").Text = "Mode: Connections";
				}
			}

			if (keyEvent.Keycode == Key.E) {
				_direction = new Vector2(_direction.Y, -_direction.X);

				UpdateGizmo();
			}

			return true;
		}

		return false;
	}

	private void UpdateGizmo() {
		_roomLayoutGizmo.Connections = _connections;
		_roomLayoutGizmo.Bounds = _bounds;
		_roomLayoutGizmo.Direction = _direction;

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
