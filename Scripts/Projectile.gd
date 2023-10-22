extends CharacterBody2D


@export var SPEED = 200


var _tracked_position
var _tracked_timer


func _ready():
	_tracked_position = $NetworkNode.tracked_state(global_position)
	_tracked_timer = $NetworkNode.tracked_state(0.2)


func _on_updated(input: TrackedValue):
	velocity = global_transform.x * SPEED

	_tracked_timer.value = _tracked_timer.old_value - NetworkManager.delta()

	if _tracked_timer.value <= 0:
		$NetworkNode.despawn()

	print(NetworkManager._id_debug(), "Looking for body on tick ", NetworkManager.current_tick)
	for body in $"Damage Area".get_overlapping_bodies():
		if not body.is_in_group("Entities"):
			continue

		print("found body ", body)
			
		body.hurt(1, global_position)
	
	move_and_slide()

func _on_recorded_state():
	_tracked_position.value = global_position


func _on_applied_state():
	global_position = _tracked_position.value