extends Node2D


@export var PROJECTILE_RESOURCE: PackedScene


const SWING_SPEED = 24.0


var _tracked_target_swing_rotation


func _ready():
	_tracked_target_swing_rotation = get_parent().get_parent().get_node("NetworkNode").tracked_state(-45.0)


func _on_handled_early_state():
	_tracked_target_swing_rotation.value = _tracked_target_swing_rotation.old_value


func _on_updated(input: TrackedValue):
	if input.value == null:
		return

	look_at(global_position + input.value.point_direction)

	if global_rotation > -PI / 2 && global_rotation < PI / 2:
		scale.y = 1
	else:
		scale.y = -1


func _process(_delta):
	$Swing.rotation = lerp_angle($Swing.rotation, _tracked_target_swing_rotation.value, SWING_SPEED * _delta)


func shoot():
	var instance: Node2D = NetworkManager.spawn(PROJECTILE_RESOURCE)

	instance.global_position = global_position
	instance.global_rotation = global_rotation
	instance.global_position += instance.global_transform.x * 10
	
	get_parent().add_child(instance)

	print(_tracked_target_swing_rotation._values)

	print(NetworkManager.current_tick)

	_tracked_target_swing_rotation.value = -_tracked_target_swing_rotation.value
