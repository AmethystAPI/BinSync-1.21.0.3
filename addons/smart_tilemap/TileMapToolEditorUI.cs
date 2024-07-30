#if TOOLS
using Godot;
using System;
using System.Collections.Generic;

[Tool]
public partial class TileMapToolEditorUI : Control {
	[Export] public TextureRect _previewTextureRect;
	[Export] public Control _tileButtonsHolder;
	[Export] public Label _smartTileMapPathLabel;
	[Export] public Label _modeLabel;

	private Texture2D _previewTexture;

	private string _smartTileMapPath = null;
	private Dictionary<Vector2, Button> _tileButtons = new Dictionary<Vector2, Button>();
	private int _mode = 0;

	private string[] _modeKeys = new string[] {
		"RoofsTopLeft",
		"RoofsTop",
		"RoofsTopRight",
		"RoofsLeft",
		"RoofsMiddle",
		"RoofsRight",
		"RoofsBottomLeft",
		"RoofsBottom",
		"RoofsBottomRight",
		"RoofsInsideTopLeft",
		"RoofsInsideTopRight",
		"RoofsInsideBottomLeft",
		"RoofsInsideBottomRight",
		"WallsLeft",
		"WallsMiddle",
		"WallsRight",
		"WallsInsideLeft",
		"WallsInsideRight",
		"WallHidden",
		"ShadowLeft",
		"ShadowMiddle",
		"ShadowRight",
		"ShadowInsideLeft",
		"ShadowInsideRight",
		"Floor",
 	};

	public override void _Ready() {
		_modeLabel.Text = _modeKeys[_mode];
	}

	public void SelectTexture() {
		string[] paths = EditorInterface.Singleton.GetSelectedPaths();

		if (paths.Length != 1) return;

		string path = paths[0];

		_previewTexture = ResourceLoader.Load<Texture2D>(path);

		_previewTextureRect.Texture = _previewTexture;

		Vector2 textureSize = _previewTexture.GetSize();

		_previewTextureRect.CustomMinimumSize = textureSize * 2;

		foreach (Button button in _tileButtons.Values) {
			button.QueueFree();
		}

		_tileButtons.Clear();

		for (int x = 0; x < textureSize.X / 16; x++) {
			for (int y = 0; y < textureSize.Y / 16; y++) {
				Vector2 tilePosition = new Vector2(x, y);

				Button button = new Button() {
					CustomMinimumSize = new Vector2(32f, 32f),
					Position = tilePosition * 32f,
					Modulate = new Color("#ffffff55")
				};

				_tileButtonsHolder.AddChild(button);

				_tileButtons.Add(tilePosition, button);

				button.Pressed += () => TileButtonPressed(tilePosition);
			}
		}
	}

	public void SelectSmartTileMap() {
		string[] paths = EditorInterface.Singleton.GetSelectedPaths();

		if (paths.Length != 1) return;

		string path = paths[0];

		_smartTileMapPath = path;

		_smartTileMapPathLabel.Text = _smartTileMapPath;
	}

	public void NextMode() {
		_mode++;

		if (_mode >= _modeKeys.Length) _mode = 0;

		_modeLabel.Text = _modeKeys[_mode];
	}

	public void TileButtonPressed(Vector2 position) {
		if (_smartTileMapPath == null) return;

		SmartTileset smartTileset = ResourceLoader.Load<SmartTileset>(_smartTileMapPath);

		string propertyName = _modeKeys[_mode];

		if (smartTileset.Get(propertyName).VariantType == Variant.Type.PackedVector2Array) {
			Vector2[] _originalValue = (Vector2[])smartTileset.Get(propertyName).Obj;
			Vector2[] _newValue = new Vector2[_originalValue.Length + 1];

			Array.Copy(_originalValue, _newValue, _originalValue.Length);
			_newValue[_newValue.Length - 1] = position;

			smartTileset.Set(propertyName, _newValue);
		} else {
			smartTileset.Set(propertyName, position);
		}

		ResourceSaver.Singleton.Save(smartTileset, _smartTileMapPath);
	}
}
#endif