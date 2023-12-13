extends Node2D


@export var PlayerScene: PackedScene
@export var EnemyScene: PackedScene


func _ready():
	NetworkManager.started.connect(_on_started)
	NetworkManager.recorded_input.connect(_on_recorded_input)

	# NetworkManager.host()
	# NetworkManager.start()

	# NetworkManager.join("104.33.194.150")


func _input(event):
	if event.is_action_pressed("host"):
		NetworkManager.host()
	
	if event.is_action_pressed("join"):
		NetworkManager.join("104.33.194.150")

	if event.is_action_pressed("join_local"):
		NetworkManager.join("192.168.86.37")

	if event.is_action_pressed("start"):
		NetworkManager.start()


func _on_started():
	for id in NetworkManager.players:
		var player: SGCharacterBody2D = NetworkManager.spawn(PlayerScene, id)

		add_child(player)

	var enemy: SGCharacterBody2D = NetworkManager.spawn(EnemyScene, multiplayer.get_unique_id())

	add_child(enemy)


func _on_recorded_input(input: TrackedValue):
	input.value = {
		"movement": Vector2(Input.get_axis("move_left", "move_right"), Input.get_axis("move_up", "move_down")).normalized(),
		"shoot": Input.is_action_pressed("shoot"),
		"dash": Input.is_action_pressed("dash"),
		"point_direction": (Player.LocalPlayer.get_global_mouse_position() - Player.LocalPlayer.global_position).normalized()
	}
