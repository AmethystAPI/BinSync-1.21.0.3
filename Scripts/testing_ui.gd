extends Control


@export var PlayerScene: PackedScene
@export var SlimeScene: PackedScene
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
	var spawn_x = 150

	for id in NetworkManager.players:
		var player: SGCharacterBody2D = NetworkManager.spawn(PlayerScene, id)

		player.set_global_fixed_position(SGFixed.from_float_vector2(Vector2(spawn_x, 150)))
		
		SpawnNode.add_child(player)

		spawn_x += 50

	for i in range(20):
		var slime: SGCharacterBody2D = NetworkManager.spawn(SlimeScene)

		slime.set_global_fixed_position(SGFixed.from_float_vector2(Vector2(spawn_x, 150)))
		
		SpawnNode.add_child(slime)

		spawn_x += 50

func _on_recorded_input(input: TrackedValue):
	input.value = {
		"movement": Vector2(Input.get_axis("move_left", "move_right"), Input.get_axis("move_up", "move_down")).normalized(),
		"shoot": Input.is_action_pressed("shoot"),
		"dash": Input.is_action_pressed("dash"),
		"point_direction": (Player.LocalPlayer.get_global_mouse_position() - Player.LocalPlayer.global_position).normalized()
	}