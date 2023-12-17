extends SGCharacterBody2D


@export var stateMachine: StateMachine


var SPEED = SGFixed.from_int(20)

var KNOCKBACK_POWER = SGFixed.from_int(300)
var KNOCKBACK_DECAY = SGFixed.from_int(12)
var KNOCKBACK_MINIMUM = SGFixed.from_int(10)


var _tracked_position: TrackedValue
var _tracked_health: TrackedValue
var _tracked_knockback: TrackedValue

func _ready():
	$NetworkNode.handled_early_state.connect(_on_handled_early_state)
	$NetworkNode.recorded_state.connect(_on_recorded_state)
	$NetworkNode.applied_state.connect(_on_applied_state)

	_tracked_position = $NetworkNode.tracked_state(get_global_fixed_position())
	_tracked_health = $NetworkNode.tracked_state(100)
	_tracked_knockback = $NetworkNode.tracked_state(SGFixed.vector2(0, 0))


func _on_handled_early_state():
	_tracked_knockback.value = _tracked_knockback.old_value
	_tracked_health.value = _tracked_health.old_value


func _on_recorded_state():
	_tracked_position.value = get_global_fixed_position()


func _on_applied_state(input: TrackedValue):
	set_global_fixed_position(_tracked_position.value)

	sync_to_physics_engine()


func hurt(damage, source_position):
	_tracked_health.value -= damage

	_tracked_knockback.value = get_global_fixed_position().sub(source_position).normalized().mul(KNOCKBACK_POWER)
	
	if _tracked_health.value <= 0:
		$NetworkNode.despawn()

	stateMachine.go_to_state("Hurt")