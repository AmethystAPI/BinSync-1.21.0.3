extends State


@export var network_node: NetworkNode


var _slime: SGCharacterBody2D
var _tracked_idle_timer: TrackedValue


func _ready():
	_slime = get_parent().get_parent()

	_tracked_idle_timer = network_node.tracked_state(0)


func _enter():
	_tracked_idle_timer.value = SGFixed.from_int(0)


func _on_handled_early_state():
	_tracked_idle_timer.value = _tracked_idle_timer.old_value


func _on_updated(_input: TrackedValue):
	_tracked_idle_timer.value += NetworkManager.delta()

	_slime.velocity = SGFixed.vector2(0, 0)
	_slime.sync_to_physics_engine()

	var target: Player = Player.Players[0]

	for player in Player.Players:
		if player.get_global_fixed_position().distance_to(_slime.get_global_fixed_position()) > target.get_global_fixed_position().distance_to(_slime.get_global_fixed_position()):
			continue
		
		target = player

	var idle_timer_over = _tracked_idle_timer.value >= SGFixed.from_float(1.0)

	if idle_timer_over:
		go_to_state("Jump")

	if idle_timer_over:
		_tracked_idle_timer.value = SGFixed.from_float(0.0)