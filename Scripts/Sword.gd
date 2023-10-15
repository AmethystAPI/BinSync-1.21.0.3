extends Node2D


@export var PROJECTILE_RESOURCE: PackedScene


const SWING_SPEED = 24.0


var _target_swing_rotation = -45.0


func _record_state(state, old_state):
	state.target_swing = _target_swing_rotation

	if get_parent().get_node("NetworkNode").has_authority():
		state.mouse_position = get_global_mouse_position()
	elif old_state != null:
		state.mouse_position = old_state.mouse_position
	else:
		state.mouse_position = Vector2.ZERO

func _on_tick(state):
	_target_swing_rotation = state.target_swing  

	look_at(state.mouse_position)

	if global_rotation > -PI / 2 && global_rotation < PI / 2:
		scale.y = 1
	else:
		scale.y = -1


func _process(_delta):
	$Swing.rotation = lerp_angle($Swing.rotation, _target_swing_rotation, SWING_SPEED * _delta)


func shoot():
	var instance: Node2D = NetworkManager.spawn(PROJECTILE_RESOURCE)
	
	get_parent().add_child(instance)
	
	instance.global_position = global_position
	instance.global_rotation = global_rotation
	instance.global_position += instance.global_transform.x * 10

	_target_swing_rotation = -_target_swing_rotation
