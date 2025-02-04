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

    private HashSet<RoomLayout.Connection> _connections = new HashSet<RoomLayout.Connection>();
    private List<Vector2> _bounds = new List<Vector2>();
    private List<Vector2> _spawnLocations = new List<Vector2>();
    private List<Vector2> _edgeLocations = new List<Vector2>();
    private List<int> _edgeDistances = new List<int>();
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

        roomLayout.SpawnLocations = CalculateSpawnLocations(roomLayout).ToArray();
        _spawnLocations = new List<Vector2>(roomLayout.SpawnLocations);

        (List<Vector2> edgeLocations, List<int> edgeDistances) = CalculateSpawnEdgeField(roomLayout);

        roomLayout.EdgeFieldPosition = edgeLocations.ToArray();
        roomLayout.EdgeFieldDistance = edgeDistances.ToArray();

        _edgeLocations = edgeLocations;
        _edgeDistances = edgeDistances;

        if (ResourceLoader.Exists(GetRoomLayoutPath()))
            File.Delete(ProjectSettings.GlobalizePath(GetRoomLayoutPath()));

        ResourceSaver.Save(roomLayout, GetRoomLayoutPath());
        roomLayout.TakeOverPath(GetRoomLayoutPath());

        UpdateGizmo();
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

        _connections = new HashSet<RoomLayout.Connection>();
        _bounds = new List<Vector2>();
        _spawnLocations = new List<Vector2>();
        _edgeLocations = new List<Vector2>();
        _edgeLocations = new List<Vector2>();
        _mode = Mode.Bounds;
        _direction = Vector2.Right;

        _gui.GetNode<Label>("ModeLabel").Text = "Mode: Bounds";

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
        _roomLayoutGizmo.SpawnLocations = _spawnLocations;
        _roomLayoutGizmo.EdgeLocations = _edgeLocations;
        _roomLayoutGizmo.EdgeDistances = _edgeDistances;

        _roomLayoutGizmo.QueueRedraw();
    }

    private string GetRoomLayoutPath() {
        string relativePath = _selectedPath.Substring("res://content/rooms/".Length);
        string fileName = Path.GetFileName(relativePath);
        string relativeFolders = relativePath.Substring(0, relativePath.Length - fileName.Length);
        string saveRelativePath = relativeFolders + "room_layout." + Path.GetFileNameWithoutExtension(relativePath) + ".tres";

        return "res://generated/rooms/" + saveRelativePath;
    }

    private bool LocationIsValidSpawn(Vector2 location, RoomLayout roomLayout) {
        if (location.X < roomLayout.TopLeftBound.X) return false;
        if (location.Y < roomLayout.TopLeftBound.Y) return false;
        if (location.X >= roomLayout.BottomRightBound.X) return false;
        if (location.Y >= roomLayout.BottomRightBound.Y) return false;

        if (roomLayout.Walls.Contains(location)) return false;

        List<Vector2> offsets = new List<Vector2>() {
            Vector2.Up + Vector2.Left, Vector2.Up, Vector2.Up + Vector2.Right,
            Vector2.Left, Vector2.Right,
            Vector2.Down + Vector2.Left, Vector2.Down, Vector2.Down + Vector2.Right
        };

        foreach (Vector2 offset in offsets) {
            if (roomLayout.Walls.Contains(location + offset)) return false;
        }

        return true;
    }

    private List<Vector2> CalculateSpawnLocations(RoomLayout roomLayout) {
        List<Vector2> spawnLocations = new List<Vector2>();

        List<Vector2> locationsToCheck = new List<Vector2>();

        RoomLayout.Connection[] connections = roomLayout.GetConnections();

        foreach (RoomLayout.Connection connection in connections) {
            if (connection.Direction == Vector2.Right) {
                locationsToCheck.Add(connection.Location + Vector2.Left);
                spawnLocations.Add(connection.Location + Vector2.Left);
            } else if (connection.Direction == Vector2.Left) {
                locationsToCheck.Add(connection.Location);
                spawnLocations.Add(connection.Location);
            } else if (connection.Direction == Vector2.Up) {
                locationsToCheck.Add(connection.Location);
                spawnLocations.Add(connection.Location);
            } else if (connection.Direction == Vector2.Down) {
                locationsToCheck.Add(connection.Location + Vector2.Up);
                spawnLocations.Add(connection.Location + Vector2.Up);
            }
        }

        List<Vector2> offsets = new List<Vector2>() { Vector2.Left, Vector2.Right, Vector2.Up, Vector2.Down };

        while (locationsToCheck.Count > 0) {
            Vector2 location = locationsToCheck[0];
            locationsToCheck.RemoveAt(0);


            foreach (Vector2 offset in offsets) {
                if (!LocationIsValidSpawn(location + offset, roomLayout)) continue;

                if (spawnLocations.Contains(location + offset)) continue;

                locationsToCheck.Add(location + offset);
                spawnLocations.Add(location + offset);
            }
        }

        spawnLocations = spawnLocations.Where(spawnLocation => {
            if (spawnLocation.X == roomLayout.TopLeftBound.X) return false;
            if (spawnLocation.Y == roomLayout.TopLeftBound.Y) return false;

            if (spawnLocation.X == roomLayout.BottomRightBound.X - 1) return false;
            if (spawnLocation.Y == roomLayout.BottomRightBound.Y - 1) return false;

            return true;
        }).ToList();

        GD.Print("Calculated " + spawnLocations.Count + " spawn locations");

        return spawnLocations;
    }

    private bool LocationIsEmpty(Vector2 location, RoomLayout roomLayout) {
        if (location.X < roomLayout.TopLeftBound.X) return false;
        if (location.Y < roomLayout.TopLeftBound.Y) return false;
        if (location.X >= roomLayout.BottomRightBound.X) return false;
        if (location.Y >= roomLayout.BottomRightBound.Y) return false;

        if (roomLayout.Walls.Contains(location)) return false;

        return true;
    }

    private bool LocationTouchingWalls(Vector2 location, RoomLayout roomLayout) {
        if (location.X < roomLayout.TopLeftBound.X) return true;
        if (location.Y < roomLayout.TopLeftBound.Y) return true;
        if (location.X >= roomLayout.BottomRightBound.X) return true;
        if (location.Y >= roomLayout.BottomRightBound.Y) return true;

        List<Vector2> offsets = new List<Vector2>() {
            Vector2.Up + Vector2.Left, Vector2.Up, Vector2.Up + Vector2.Right,
            Vector2.Left, Vector2.Right,
            Vector2.Down + Vector2.Left, Vector2.Down, Vector2.Down + Vector2.Right
        };

        foreach (Vector2 offset in offsets) {
            if (roomLayout.Walls.Contains(location + offset)) return true;
        }

        return false;
    }

    private bool LocationIsValidEdge(Vector2 location, RoomLayout roomLayout) {
        if (location.X < roomLayout.TopLeftBound.X) return true;
        if (location.Y < roomLayout.TopLeftBound.Y) return true;
        if (location.X >= roomLayout.BottomRightBound.X) return true;
        if (location.Y >= roomLayout.BottomRightBound.Y) return true;

        if (roomLayout.Walls.Contains(location)) return true;

        return false;
    }

    private (List<Vector2>, List<int>) CalculateSpawnEdgeField(RoomLayout roomLayout) {
        List<Vector2> inRoomLocations = new List<Vector2>();
        List<Vector2> inRoomLocationsToCheck = new List<Vector2>();

        RoomLayout.Connection[] connections = roomLayout.GetConnections();

        foreach (RoomLayout.Connection connection in connections) {
            if (connection.Direction == Vector2.Right) {
                inRoomLocationsToCheck.Add(connection.Location + Vector2.Left);
                inRoomLocations.Add(connection.Location + Vector2.Left);
            } else if (connection.Direction == Vector2.Left) {
                inRoomLocationsToCheck.Add(connection.Location);
                inRoomLocations.Add(connection.Location);
            } else if (connection.Direction == Vector2.Up) {
                inRoomLocationsToCheck.Add(connection.Location);
                inRoomLocations.Add(connection.Location);
            } else if (connection.Direction == Vector2.Down) {
                inRoomLocationsToCheck.Add(connection.Location + Vector2.Up);
                inRoomLocations.Add(connection.Location + Vector2.Up);
            }
        }

        List<Vector2> offsets = new List<Vector2>() { Vector2.Left, Vector2.Right, Vector2.Up, Vector2.Down };

        while (inRoomLocationsToCheck.Count > 0) {
            Vector2 location = inRoomLocationsToCheck[0];
            inRoomLocationsToCheck.RemoveAt(0);

            foreach (Vector2 offset in offsets) {
                if (!LocationIsEmpty(location + offset, roomLayout)) continue;

                if (inRoomLocations.Contains(location + offset)) continue;

                inRoomLocationsToCheck.Add(location + offset);
                inRoomLocations.Add(location + offset);
            }
        }

        List<Vector2> floodQueue = inRoomLocations.Where(location => LocationTouchingWalls(location, roomLayout)).ToList();
        List<int> floodQueueDistances = new List<int>();

        foreach(Vector2 location in floodQueue) { 
            floodQueueDistances.Add(0);
        }

        List<Vector2> edgeLocations = new List<Vector2>();
        List<int> edgeDistances = new List<int>();

        while(floodQueue.Count > 0) {
            Vector2 location = floodQueue[0];
            floodQueue.RemoveAt(0);
            int distance = floodQueueDistances[0];
            floodQueueDistances.RemoveAt(0);

            int neighborDistance = distance + 1;

            if(distance >= 20) continue;

            foreach (Vector2 offset in offsets) {
                if (!LocationIsValidEdge(location + offset, roomLayout)) continue;

                int index = edgeLocations.IndexOf(location + offset);

                if (index != -1) {
                    edgeDistances[index] = Math.Min(edgeDistances[index], neighborDistance);

                    continue;
                }

                edgeLocations.Add(location + offset);
                edgeDistances.Add(neighborDistance);
                
                floodQueue.Add(location + offset);
                floodQueueDistances.Add(neighborDistance);
            }
        }

        GD.Print("Found " + edgeLocations.Count + " edges!");

        return (edgeLocations, edgeDistances);
    }
}
#endif
