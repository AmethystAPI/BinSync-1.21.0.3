using Godot;
using Networking;
using Riptide;
using System.Linq;

public partial class Room : Node2D, NetworkPointUser {
	[Export] public PackedScene[] EnemyScenes;
	[Export] public Node2D[] SpawnPoints;
	[Export] public Node2D[] Connections;
	[Export] public Vector2[] ConnectionDirections;

	public NetworkPoint NetworkPoint { get; set; } = new NetworkPoint();

	private int _aliveEnemies = 0;
	private Vector2 _exitDirection;

	public override void _Ready() {
		NetworkPoint.Setup(this);

		NetworkPoint.Register(nameof(SpawnEnemyRpc), SpawnEnemyRpc);
		NetworkPoint.Register(nameof(EndRpc), EndRpc);
	}

	public void AddEnemy() {
		_aliveEnemies++;
	}

	public void RemoveEnemy() {
		_aliveEnemies--;

		if (_aliveEnemies != 0) return;

		if (!NetworkManager.IsHost) return;

		End();
	}

	public virtual void Place() {
		SpawnEnemies();
	}

	public virtual void PlaceEntrance(Vector2 direction) {

	}

	public virtual void PlaceExit(Vector2 direction) {
		_exitDirection = direction;
	}

	private void SpawnEnemies() {
		if (SpawnPoints.Length == 0) return;

		float points = Game.Difficulty;

		while (points > 0) {
			Node2D spawnPoint = SpawnPoints[new RandomNumberGenerator().RandiRange(0, SpawnPoints.Length - 1)];

			NetworkPoint.SendRpcToClients(nameof(SpawnEnemyRpc), message => {
				message.AddFloat(spawnPoint.GlobalPosition.X);
				message.AddFloat(spawnPoint.GlobalPosition.Y);

				message.AddInt(new RandomNumberGenerator().RandiRange(0, EnemyScenes.Length - 1));
			});

			points--;
		}
	}

	private void SpawnEnemyRpc(Message message) {
		Vector2 position = new Vector2(message.GetFloat(), message.GetFloat());
		int enemySceneIndex = message.GetInt();

		Node2D enemy = NetworkManager.SpawnNetworkSafe<Node2D>(EnemyScenes[enemySceneIndex], "Enemy");

		AddChild(enemy);

		enemy.GlobalPosition = position;
	}

	protected void End() {
		if (!NetworkManager.IsHost) return;

		WorldGenerator.SpawnTrinkets();

		Game.Difficulty += Mathf.Sqrt(Player.Players.Count) / 3f;

		WorldGenerator.PlaceNextRoom(Connections[ConnectionDirections.ToList().IndexOf(_exitDirection)].GlobalPosition, _exitDirection);

		NetworkPoint.SendRpcToClients(nameof(EndRpc));
	}

	protected virtual void EndRpc(Message message) {
	}
}
