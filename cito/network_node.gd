extends Node
class_name NetworkNode


signal handled_early_state
signal updated(input: TrackedValue)
signal applied_state
signal recorded_state


var id = -1


var _tracked_states = []

var _tracked_authority = tracked_state(1)
var _tracked_spawned = tracked_state(true)
var _spawned_tick = -1
var _despawned_tick = -1

var _old_parent
var _spawned_cache = true


func _process(delta):
	if not _tracked_spawned.value:
		return

	for value in _tracked_states:
		value._process(delta)


func tracked_state(initial_value, interpolate_event = null, should_update_provider = null) -> TrackedValue:	
	if should_update_provider == null:
		should_update_provider = func(value, old_value): return false

	var value = TrackedValue.new(initial_value, interpolate_event, should_update_provider)

	_tracked_states.append(value)

	return value

	
func has_authority() -> bool:
	return _tracked_authority.value == NetworkManager.local_player()


func give_authority(new_authority):
	_tracked_authority.value = new_authority


func despawn():
	_tracked_spawned.value = false

	_despawned_tick = NetworkManager.current_tick
	
	_spawned_cache = false

	_old_parent = get_parent().get_parent()

	if _old_parent != null:
		_old_parent.remove_child(get_parent())

var respawned = false

func _respawn():
	if _spawned_cache:
		return

	respawned = true

	if _old_parent != null:
		_old_parent.add_child(get_parent())
	
	_spawned_cache = true


func _spawn():
	_spawned_tick = NetworkManager.current_tick
	

func _handle_early_state():
	_tracked_spawned.value = _tracked_spawned.old_value
	_tracked_authority.value = _tracked_authority.old_value

	handled_early_state.emit()


func _apply_state():
	if NetworkManager.current_tick < _spawned_tick:
		NetworkManager._network_nodes.erase(id)

		get_parent().queue_free()

		return

	if not _tracked_spawned.value:
		return

	_respawn()

	applied_state.emit()


func _update():
	if not _tracked_spawned.value:
		return

	var player_index = NetworkManager.players.find(_tracked_authority.value)
	var tracked_input = NetworkManager._player_tracked_inputs[player_index]

	updated.emit(tracked_input)


@rpc("any_peer", "call_remote", "reliable")
func _update_state(tick, states_to_update, updated_values):
	for index in range(states_to_update.size()):
		_tracked_states[states_to_update[index]]._update_value(tick, updated_values[index])


func _record_state():
	if _tracked_authority.value == null:
		_tracked_authority.value = _tracked_authority.old_value
	
	if _tracked_spawned.value == null:
		_tracked_spawned.value = _tracked_spawned.old_value

	if not _tracked_spawned.value:
		if NetworkManager.current_tick - _despawned_tick > floor(NetworkManager.MAX_MESSAGE_DELAY * NetworkManager.TICKS_PER_SECOND):
			get_parent().queue_free()

			NetworkManager._network_nodes.erase(id)

		return

	recorded_state.emit()

	var states_to_update = []
	var updated_values = []

	for index in range(_tracked_states.size()):
		var state = _tracked_states[index]

		if state._should_update():
			state._set_updated()

			states_to_update.append(index)
			updated_values.append(state.value)

	if not states_to_update.is_empty():
		_update_state.rpc(NetworkManager.current_tick, states_to_update, updated_values)
