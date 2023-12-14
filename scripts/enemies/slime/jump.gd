extends State


@export var network_node: NetworkNode


var SPEED: int = SGFixed.from_int(20)


var _slime: SGCharacterBody2D


func _ready():
	_slime = get_parent().get_parent()

func _on_updated(_input: TrackedValue):
	var target: Player = Player.Players[0]

	for player in Player.Players:
		if player.get_global_fixed_position().distance_to(_slime.get_global_fixed_position()) > target.get_global_fixed_position().distance_to(_slime.get_global_fixed_position()):
			continue
		
		target = player

	if _slime.get_global_fixed_position().distance_to(target.get_global_fixed_position()) < SGFixed.from_int(32):
		go_to_state("Idle")
	else:
		var direction: SGFixedVector2 = target.get_global_fixed_position().sub(_slime.get_global_fixed_position()).normalized()
		_slime.velocity = direction.mul(SPEED).mul(NetworkManager.delta())
		
	_slime.move_and_slide()
	_slime.sync_to_physics_engine()