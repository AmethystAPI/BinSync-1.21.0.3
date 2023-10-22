extends SGFixedNode2D


@export var PROJECTILE_RESOURCE: PackedScene


const SWING_SPEED = 24.0


var _tracked_rotation
var _tracked_target_swing_rotation


func _ready():
	_tracked_rotation = get_parent().get_node("NetworkNode").tracked_state(get_global_fixed_rotation())
	_tracked_target_swing_rotation = get_parent().get_node("NetworkNode").tracked_state(-45.0)


func _on_handled_early_state():
	_tracked_target_swing_rotation.value = _tracked_target_swing_rotation.old_value


func _on_updated(input: TrackedValue):
	if input.value == null:
		return

	set_global_fixed_rotation(SGFixed.from_float_vector2(input.value.point_direction).angle())

	if SGFixed.to_float(get_global_fixed_rotation()) > -PI / 2 && SGFixed.to_float(get_global_fixed_rotation()) < PI / 2:
		scale.y = 1
	else:
		scale.y = -1


func _process(_delta):
	$Swing.rotation = lerp_angle($Swing.rotation, _tracked_target_swing_rotation.value, SWING_SPEED * _delta)


func shoot():
	# print(NetworkManager._id_debug(), ' ', NetworkManager.current_tick, ' shoot ', get_global_fixed_position().to_float(), ' ', SGFixed.to_float(get_global_fixed_rotation()))

	var instance: SGCharacterBody2D = NetworkManager.spawn(PROJECTILE_RESOURCE)

	instance.set_global_fixed_position(get_global_fixed_position().add(get_global_fixed_transform().x.mul(SGFixed.from_int(14))))
	instance.set_global_fixed_rotation(get_global_fixed_rotation())
	
	get_parent().get_parent().add_child(instance)

	_tracked_target_swing_rotation.value = -_tracked_target_swing_rotation.value
