extends Node2D


@export var PlayerScene: PackedScene


func _ready():
	NetworkManager.started.connect(_on_started)
	NetworkManager.recorded_input.connect(_on_recorded_input)

	NetworkManager.host()
	NetworkManager.start()


func _on_started():
	for id in NetworkManager.players:
		var player: SGCharacterBody2D = NetworkManager.spawn(PlayerScene, id)

		print('spawned ', player)

		add_child(player)


func _on_recorded_input(input: TrackedValue):
	input.value = {
		"movement": Vector2(Input.get_axis("move_left", "move_right"), Input.get_axis("move_up", "move_down")).normalized(),
		"shoot": Input.is_action_pressed("shoot"),
		"dash": Input.is_action_pressed("dash"),
		"spawn": Input.is_action_pressed("spawn"),
		"point_direction": (Player.LocalPlayer.get_global_mouse_position() - Player.LocalPlayer.global_position).normalized()
	}