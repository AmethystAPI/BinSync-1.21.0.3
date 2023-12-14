extends State


var _slime: SGCharacterBody2D


func _ready():
	_slime = get_parent().get_parent()


func _on_updated(input: TrackedValue):
	_slime.velocity = SGFixed.vector2(0, 0)
	_slime.sync_to_physics_engine()

	if _slime.fixed_position.distance_to(Player.LocalPlayer.fixed_position) > SGFixed.from_int(32):
		go_to_state("Jump")