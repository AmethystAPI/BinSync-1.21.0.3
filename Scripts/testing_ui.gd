extends Control


@export var PlayerScene: PackedScene
@export var SpawnNode: Node


func _ready():
	NetworkManager.started.connect(_on_started)


func _on_host_button_pressed():
	NetworkManager.host()


func _on_join_button_pressed():
	NetworkManager.join()


func _on_start_button_pressed():
	NetworkManager.start()

func _on_started():
	var spawn_x = 200

	for id in NetworkManager.players:
		var player = NetworkManager.spawn_with_authority(PlayerScene, id)
		SpawnNode.add_child(player)
		
		player.global_position = Vector2(spawn_x, 200)

		spawn_x += 100