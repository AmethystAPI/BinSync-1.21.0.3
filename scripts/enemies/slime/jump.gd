extends State


@export var network_node: NetworkNode


var SPEED: int = SGFixed.from_int(20)


var _tracked_position: TrackedValue
var _slime: SGCharacterBody2D


func _ready():
	_slime = get_parent().get_parent()

	_tracked_position = network_node.tracked_state(_slime.fixed_position)


func _on_updated(_input: TrackedValue):
	var target: Player = Player.Players[0]

	for player in Player.Players:
		if player.fixed_position.distance_to(_slime.fixed_position) > target.fixed_position.distance_to(_slime.fixed_position):
			continue
		
		target = player

	if _slime.fixed_position.distance_to(target.fixed_position) < SGFixed.from_int(32):
		go_to_state("Idle")
	else:
		var direction: SGFixedVector2 = target.fixed_position.sub(_slime.fixed_position).normalized()
		_slime.velocity = direction.mul(SPEED).mul(NetworkManager.delta())

	_slime.move_and_slide()
	_slime.sync_to_physics_engine()


func _on_recorded_state():
	_tracked_position.value = _slime.fixed_position


func _on_resumed_state(_input: TrackedValue):
	_slime.set_global_fixed_position(_tracked_position.value)
