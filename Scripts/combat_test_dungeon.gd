extends Node2D


@export var PlayerScene: PackedScene


func _ready():
	NetworkManager.started.connect(_on_started)
	NetworkManager.recorded_input.connect(_on_recorded_input)

	multiplayer.connection_failed.connect(func():
		NetworkManager.join("127.0.0.1")
	)

	multiplayer.peer_connected.connect(func(id):
		print("Peer connected: ", id)
	)


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
		

func _on_recorded_input(input: TrackedValue):
	input.value = {
		"movement": Vector2(Input.get_axis("move_left", "move_right"), Input.get_axis("move_up", "move_down")).normalized(),
		"shoot": Input.is_action_pressed("shoot"),
		"dash": Input.is_action_pressed("dash"),
		"spawn": Input.is_action_pressed("spawn"),
		"point_direction": (Player.LocalPlayer.get_global_mouse_position() - Player.LocalPlayer.global_position).normalized()
	}