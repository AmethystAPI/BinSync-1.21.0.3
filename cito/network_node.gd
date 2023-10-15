extends Node
class_name NetworkNode


signal recorded_state(state: Dictionary, old_state: Variant)
signal ticked(state: Dictionary)


var id = -1

var authority = 1

var created_tick = -1
var despawned_tick = -1

var scene_child = false

var spawned = true


var _old_parent = null


func _ready():
	NetworkManager._register_network_node(self)


func has_registered() -> bool:
	return id != -1


func has_authority() -> bool:
	return authority == NetworkManager.local_player


func give_authority(client_id: int) -> void:
	authority = client_id


func input_state(state_name: String, value, default_value, state: Dictionary, old_state: Variant) -> void:
	if has_authority():
		state[state_name] = value
	elif old_state != null:
		state[state_name] = old_state[state_name]
	else:
		state[state_name] = default_value


func despawn() -> void:
	if not spawned:
		return

	spawned = false

	despawned_tick = NetworkManager.current_tick

	_old_parent = get_parent().get_parent()

	_old_parent.remove_child(get_parent())


func _record_state(old_state):
	var state = {}

	recorded_state.emit(state, old_state)

	return state


func _tick(state):
	ticked.emit(state)


func _respawn() -> void:
	if spawned:
		return

	spawned = true

	_old_parent.add_child(get_parent())