extends SGCharacterBody2D


@export var stateMachine: StateMachine


var SPEED = SGFixed.from_int(20)

var KNOCKBACK_POWER = SGFixed.from_int(300)
var KNOCKBACK_DECAY = SGFixed.from_int(12)
var KNOCKBACK_MINIMUM = SGFixed.from_int(10)


var _tracked_position: TrackedValue
var _tracked_health: TrackedValue

func _ready():
	$NetworkNode.handled_early_state.connect(_on_handled_early_state)
	$NetworkNode.recorded_state.connect(_on_recorded_state)
	$NetworkNode.applied_state.connect(_on_applied_state)

	_tracked_position = $NetworkNode.tracked_state(get_global_fixed_position())
	_tracked_health = $NetworkNode.tracked_state(3)


func _on_handled_early_state():
	_tracked_health.value = _tracked_health.old_value


func _on_recorded_state():
	_tracked_position.value = get_global_fixed_position()


func _on_applied_state(input: TrackedValue):
	set_global_fixed_position(_tracked_position.value)

	sync_to_physics_engine()


func hurt(damage, source_position):
	if stateMachine.tracked_current_state.value != "Idle":
		return

	stateMachine.go_to_state("Hurt")
	stateMachine.current_state.start(damage, source_position)