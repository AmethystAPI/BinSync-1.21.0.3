extends State


@export var network_node: NetworkNode


var KNOCKBACK_POWER = SGFixed.from_int(300)
var KNOCKBACK_DECAY = SGFixed.from_int(12)
var KNOCKBACK_MINIMUM = SGFixed.from_int(10)


var _tracked_knockback: TrackedValue
var _slime: SGCharacterBody2D


func _ready():
	_slime = get_parent().get_parent()

	_tracked_knockback = network_node.tracked_state(SGFixed.vector2(0, 0))


func start(damage: int, source_position: SGFixedVector2):
	_slime._tracked_health.value -= damage

	_tracked_knockback.value = _slime.get_global_fixed_position().sub(source_position).normalized().mul(KNOCKBACK_POWER)
	
	if _slime._tracked_health.value <= 0:
		network_node.despawn()


func _on_handled_early_state():
	_tracked_knockback.value = _tracked_knockback.old_value


func _on_updated(_input: TrackedValue):
	_slime.velocity = _tracked_knockback.value.mul(NetworkManager.delta())
	_tracked_knockback.value = _tracked_knockback.value.linear_interpolate(SGFixedVector2.new(), SGFixed.mul(NetworkManager.delta(), KNOCKBACK_DECAY))
	
	if _tracked_knockback.value.length() < KNOCKBACK_MINIMUM:
		go_to_state("Idle")

	_slime.move_and_slide()
	_slime.sync_to_physics_engine()