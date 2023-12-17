extends SGArea2D


@export var lifetime: float = 0.5


var _tracked_lifetime_timer: TrackedValue


func _ready():
	$NetworkNode.handled_early_state.connect(_on_handled_early_state)
	$NetworkNode.updated.connect(_on_updated)
	$NetworkNode.applied_state.connect(_on_applied_state)

	_tracked_lifetime_timer = $NetworkNode.tracked_state(SGFixed.from_float(lifetime))


func _on_handled_early_state():
	_tracked_lifetime_timer.value = _tracked_lifetime_timer.old_value
	
	
func _on_updated(input: TrackedValue):
	_tracked_lifetime_timer.value -= NetworkManager.delta()

	sync_to_physics_engine()
	
	for body in get_overlapping_bodies():
		print(body)

		if body is Player:
			body.hurt(1, get_global_fixed_position())

	if _tracked_lifetime_timer.value <= SGFixed.from_int(0):
		$NetworkNode.despawn()


func _on_applied_state(input: TrackedValue):
	sync_to_physics_engine()