extends State


var _slime: SGCharacterBody2D


func _ready():
	_slime = get_parent().get_parent()


func _on_updated(input: TrackedValue):
	if _slime.fixed_position.distance_to(Player.LocalPlayer.fixed_position) > SGFixed.from_int(32):
		go_to_state("Jump")