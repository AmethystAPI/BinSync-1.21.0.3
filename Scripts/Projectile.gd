extends SGCharacterBody2D


@export var SPEED = SGFixed.from_int(200)


var _tracked_position
var _tracked_timer


func _ready():
	_tracked_position = $NetworkNode.tracked_state(get_global_fixed_position())
	_tracked_timer = $NetworkNode.tracked_state(0.2)


func _on_updated(input: TrackedValue):
	velocity = get_global_fixed_transform().x.mul(SPEED).mul(NetworkManager.delta())

	_tracked_timer.value = _tracked_timer.old_value - SGFixed.to_float(NetworkManager.delta())

	if _tracked_timer.value <= 0:
		$NetworkNode.despawn()

	move_and_slide()

	$"Damage Area".sync_to_physics_engine()
	sync_to_physics_engine()

	for body in $"Damage Area".get_overlapping_bodies():
		if body is RID:
			continue

		if not body.is_in_group("Entities"):
			continue
			
		body.hurt(1, get_global_fixed_position())


func _on_recorded_state():
	_tracked_position.value = get_global_fixed_position()


func _on_applied_state(input: TrackedValue):
	set_global_fixed_position(_tracked_position.value)

	$"Damage Area".sync_to_physics_engine()
	sync_to_physics_engine()