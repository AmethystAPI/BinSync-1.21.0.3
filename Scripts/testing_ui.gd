extends Control


@export var PlayerScene: PackedScene
@export var SpawnNode: Node


func _ready():
	NetworkManager.started.connect(_on_started)
	NetworkManager.recorded_input.connect(_on_recorded_input)


func _on_host_button_pressed():
	NetworkManager.host()


func _on_join_button_pressed():
	NetworkManager.join()


func _on_start_button_pressed():
	NetworkManager.start()

func _on_started():
	var spawn_x = 200

	print('Spawning players ', NetworkManager.players)

	for id in NetworkManager.players:
		var player = NetworkManager.spawn(PlayerScene, id)

		player.global_position = Vector2(spawn_x, 200)
		
		SpawnNode.add_child(player)

		spawn_x += 100

func _on_recorded_input(input: TrackedValue):
	input.value = {
		'movement': Vector2(Input.get_axis("move_left", "move_right"), Input.get_axis("move_up", "move_down")).normalized()
	}