using System.Collections.Generic;
using Godot;
using Networking;
using Riptide;

public partial class TrinketRealm : Node2D, NetworkPointUser {
	public static TrinketRealm Me;

	[Export] public PackedScene[] TrinketScenes = new PackedScene[0];
	[Export] public PackedScene[] WeaponScenes = new PackedScene[0];
	[Export] public ColorRect TrinketBackground;
	[Export] public PackedScene[] EnemyScenes = new PackedScene[0];
	[Export] public TextureRect Icon;
	[Export] public Label Description;
	[Export] public Label DifficultyLabel;
	[Export] public Button AcceptButton;
	[Export] public Button SacraficeButton;
	[Export] public Control UI;

	public NetworkPoint NetworkPoint { get; set; } = new NetworkPoint();

	private float _trinketBackgroundTargetAlpha = 0f;
	private int _playersToComplete = 0;
	private PackedScene _currentTrinket;
	private float _localDifficulty = 1;
	private float _globalDifficulty = 0f;
	private List<PackedScene> _trinketPool = new List<PackedScene>();

	public override void _Ready() {
		Me = this;

		NetworkPoint.Setup(this);

		NetworkPoint.Register(nameof(PlayerEnterRpc), PlayerEnterRpc);
		NetworkPoint.Register(nameof(PlayerLeaveRpc), PlayerLeaveRpc);
		NetworkPoint.Register(nameof(AcceptTrinketRpc), AcceptTrinketRpc);
		NetworkPoint.Register(nameof(IncreaseDifficultyRpc), IncreaseDifficultyRpc);
	}

	public override void _Process(double delta) {
		Color color = Me.TrinketBackground.Modulate;
		color.A = Mathf.Lerp(color.A, _trinketBackgroundTargetAlpha, 1f * (float)delta);

		Me.TrinketBackground.Modulate = color;
	}

	public static void EnterTrinketRealm(Altar altar) {
		Me.TrinketBackground.GlobalPosition = altar.GlobalPosition - Me.TrinketBackground.Size / 2f;

		Me._trinketBackgroundTargetAlpha = 1f;

		Me.NetworkPoint.SendRpcToServer(nameof(PlayerEnterRpc));

		Me.ChooseTrinket();

		Me.UI.Visible = true;

		Me._localDifficulty = 1f;
	}

	private void PlayerEnterRpc(Message message) {
		if (_playersToComplete != 0) return;

		if (!NetworkManager.IsHost) return;

		_playersToComplete = NetworkManager.LocalServer.Clients.Length;

		_globalDifficulty = 0f;
	}


	private void ChooseTrinket() {
		if (_trinketPool.Count == 0) _trinketPool = new List<PackedScene>(TrinketScenes);

		RandomNumberGenerator random = new RandomNumberGenerator();

		PackedScene trinketScene = _trinketPool[random.RandiRange(0, _trinketPool.Count - 1)];

		_trinketPool.Remove(trinketScene);

		_currentTrinket = trinketScene;

		Trinket trinket = trinketScene.Instantiate<Trinket>();

		Icon.Texture = trinket.Icon;
		Description.Text = trinket.Description;
		DifficultyLabel.Text = "Difficulty: " + Mathf.Floor(_localDifficulty);
	}

	public void OnAccepted() {
		NetworkPoint.SendRpcToServer(nameof(IncreaseDifficultyRpc), message => message.AddFloat(_localDifficulty));


		NetworkPoint.BounceRpcToClients(nameof(AcceptTrinketRpc), message => {
			message.AddString(_currentTrinket.ResourcePath);
			message.AddUInt(NetworkManager.LocalClient.Id);
		});

		LeaveTinketRealm();
	}

	private void IncreaseDifficultyRpc(Message message) {
		float difficulty = message.GetFloat();

		_globalDifficulty += difficulty;
	}

	private void AcceptTrinketRpc(Message message) {
		string trinketPath = message.GetString();

		PackedScene trinketScene = ResourceLoader.Load<PackedScene>(trinketPath);
		Trinket trinket = trinketScene.Instantiate<Trinket>();

		AddChild(trinket);

		uint targetId = message.GetUInt();

		if (targetId != NetworkManager.LocalClient.Id) return;

		Player.LocalPlayer.Equip(trinket);
	}

	public void OnSacraficed() {
		ChooseTrinket();

		_localDifficulty *= 1.5f;

		if (_localDifficulty > 20f) {
			NetworkPoint.SendRpcToServer(nameof(IncreaseDifficultyRpc), message => message.AddFloat(_localDifficulty));

			LeaveTinketRealm();
		}
	}

	public static void LeaveTinketRealm() {
		Me._trinketBackgroundTargetAlpha = 0f;

		Me.UI.Visible = false;

		Me.NetworkPoint.SendRpcToServer(nameof(PlayerLeaveRpc));
	}

	private void PlayerLeaveRpc(Message message) {
		_playersToComplete--;

		if (_playersToComplete != 0) return;

		if (!NetworkManager.IsHost) return;

		GD.Print(_globalDifficulty);

		Room.Current.SpawnEnemies(_globalDifficulty, true);
	}
}
