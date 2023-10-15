extends Node
class_name NetworkNode


var id = -1
var authority = 1
var created_tick = -1
var despawned_tick = -1
var scene_child = false
var spawned = true
var old_parent = null

var _tick_events = []
var _record_state_events = []


func _ready():
	NetworkManager.register_network_node(self)


func on_tick(event) -> void:
	_tick_events.append(event)


func on_record_state(event) -> void:
	_record_state_events.append(event)


func has_registered() -> bool:
	return id != -1


func has_authority() -> bool:
	return authority == NetworkManager.local_client_id


func tick(state) -> void:
	for event in _tick_events:
		event.call(state)


func record_state(old_state):
	var state = {}

	for event in _record_state_events:
		event.call(state, old_state)

	return state

func respawn() -> void:
	if spawned:
		return

	spawned = true

	old_parent.add_child(get_parent())

func despawn() -> void:
	if not spawned:
		return

	spawned = false

	despawned_tick = NetworkManager.current_tick

	old_parent = get_parent().get_parent()

	old_parent.remove_child(get_parent())