extends Node2D


@export var PROJECTILE_RESOURCE: PackedScene


const SWING_SPEED = 24.0


var _target_swing_rotation = -45.0

func _process(_delta):
	look_at(get_global_mouse_position())

	if global_rotation > -PI / 2 && global_rotation < PI / 2:
		scale.y = 1
	else:
		scale.y = -1

	$Swing.rotation = lerp_angle($Swing.rotation, _target_swing_rotation, SWING_SPEED * _delta)


func shoot():
	var instance: Node2D = PROJECTILE_RESOURCE.instantiate()
	
	get_parent().add_child(instance)
	
	instance.global_position = global_position
	instance.global_rotation = global_rotation
	instance.global_position += instance.global_transform.x * 10

	_target_swing_rotation = -_target_swing_rotation
