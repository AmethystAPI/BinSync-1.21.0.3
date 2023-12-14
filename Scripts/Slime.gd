extends SGCharacterBody2D


var SPEED = SGFixed.from_int(20)

var KNOCKBACK_POWER = SGFixed.from_int(300)
var KNOCKBACK_DECAY = SGFixed.from_int(12)
var KNOCKBACK_MINIMUM = SGFixed.from_int(10)


var _tracked_position: TrackedValue

func _ready():
	$NetworkNode.recorded_state.connect(_on_recorded_state)
	$NetworkNode.applied_state.connect(_on_applied_state)

	_tracked_position = $NetworkNode.tracked_state(get_global_fixed_position())


func _on_recorded_state():
	_tracked_position.value = get_global_fixed_position()


func _on_applied_state(input: TrackedValue):
	set_global_fixed_position(_tracked_position.value)

	sync_to_physics_engine()