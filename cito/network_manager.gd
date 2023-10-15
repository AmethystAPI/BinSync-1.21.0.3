extends Node

const ADDRESS = "127.0.0.1"
const PORT = 25566
const MAX_PLAYERS = 2
const TICKS_PER_SECOND: float = 60
const MAX_MESSAGE_DELAY: float = 2

var clients = []
var local_client_id = -1

var is_host: bool = false

var current_tick: int = 0
var _initial_tick_time: float = -1
var _earliest_updated_tick = 0

var _network_nodes = {}
var _last_network_node_id = -1

var _states = []
var _pruned_states = 0


func _id_debug():
	return "(" + str(multiplayer.get_unique_id()) + ") "


func _setup_callbacks():
	multiplayer.peer_connected.connect(_on_peer_connected)
	multiplayer.peer_disconnected.connect(_on_peer_disconnected)


func _on_peer_connected(id):
	clients.append(id)

	if is_host:
		if clients.size() == 2:
			_setup(Time.get_ticks_msec())

		_setup.rpc_id(id, _initial_tick_time)


func _on_peer_disconnected(id):
	clients.remove_at(clients.find(id))


func host() -> void:
	_setup_callbacks()

	var peer = ENetMultiplayerPeer.new()
	
	var error = peer.create_server(PORT, MAX_PLAYERS)
	if error != OK:
		print("Error hosting: " + str(error))
		return

	peer.get_host().compress(ENetConnection.COMPRESS_ZLIB)
 
	multiplayer.set_multiplayer_peer(peer)

	is_host = true

	local_client_id = multiplayer.get_unique_id()

	_on_peer_connected(local_client_id)


func join() -> void:
	_setup_callbacks()

	var peer = ENetMultiplayerPeer.new()
	
	var error = peer.create_client(ADDRESS, PORT)
	if error != OK:
		print("Error joining: " + str(error))
		return
		
	peer.get_host().compress(ENetConnection.COMPRESS_ZLIB)
	
	multiplayer.set_multiplayer_peer(peer)

	local_client_id = multiplayer.get_unique_id()

	_on_peer_connected(local_client_id)


@rpc("any_peer", "call_local", "reliable")
func _setup(host_initial_tick_time: float):
	_initial_tick_time = host_initial_tick_time


func _get_state(tick: int):
	return _states[tick - _pruned_states]

func _set_state(tick: int, state):
	_states[tick - _pruned_states] = state

func _states_count():
	return _states.size() + _pruned_states

@rpc("any_peer", "call_remote", "reliable")
func _update_state(network_node_id, tick, state):
	if tick <= _pruned_states:
		print("Received too old state update")
		
		return

	if tick >= _states_count():
		for i in range(tick, _states_count() + 1):
			_states.append({})

	if not _get_state(tick).has(network_node_id):
		_get_state(tick)[network_node_id] = {
			"prediction": false,
			"state": null,
		}

	if  _get_state(tick)[network_node_id].state != null and _get_state(tick)[network_node_id].state.hash() != state.hash():
		_earliest_updated_tick = min(_earliest_updated_tick, tick)

	_get_state(tick)[network_node_id].state = state
	_get_state(tick)[network_node_id].prediction = false


# This tick is currently from a roleback and this node had not been created yet so we'll despawn it untill it is created again
func _despawn_future_nodes():
	for node in _network_nodes.values():
		if node.created_tick >= current_tick and not node.scene_child:
			node.despawn()
			
			_last_network_node_id = min(_last_network_node_id, node.id - 1)


# This tick is currently a roleback and the node has be despawned after this tick so we'll respawn it for the roleback
func _respawn_current_nodes():
	if current_tick >= _states_count():
		return

	for id in _get_state(current_tick).keys():
		_network_nodes[id].respawn()


func _record_state():
	var tick_state = {}

	for node in _network_nodes.values():
		if not node.spawned:
			continue

		var old_state = null

		if current_tick > 0 and _get_state(current_tick - 1).has(node.id):
			old_state = _get_state(current_tick - 1)[node.id].state

		if current_tick < _states_count() and _get_state(current_tick).has(node.id) and not _get_state(current_tick)[node.id].prediction:
			tick_state[node.id] = _get_state(current_tick)[node.id]

			continue

		tick_state[node.id] = {
			"prediction": not node.has_authority(),
			"state": node.record_state(old_state),
		}

	if current_tick >= _states_count():
		_states.append(tick_state)
	else:
		_set_state(current_tick, tick_state)


# send updates but only when not rolling back
func _send_state_updates():
	if current_tick < _states_count() - 1:
		return

	for id in _get_state(current_tick).keys():
		var node = _network_nodes[id]

		if node.has_authority():
			# _update_state.rpc(id, current_tick, _get_state(current_tick)[id].state)
			
			var tick = current_tick

			var delay = func():
				await get_tree().create_timer(0.1).timeout
				_update_state.rpc(id, tick, _get_state(tick)[id].state)
			
			delay.call()


func _tick_nodes():
	for id in _get_state(current_tick).keys():
		var node = _network_nodes[id]

		node.tick(_get_state(current_tick)[id].state)


func _tick():
	_despawn_future_nodes()
	_respawn_current_nodes()
	_record_state()
	_send_state_updates()
	_tick_nodes()


func _process(_delta):
	var current_tick_time = Time.get_ticks_msec()

	if _initial_tick_time == -1:
		return
		
	if current_tick_time < _initial_tick_time:
		return

	var target_tick: int = floor((current_tick_time - _initial_tick_time) / 1000.0 * TICKS_PER_SECOND)

	if current_tick == target_tick:
		return

	var start_tick = min(current_tick, _earliest_updated_tick)

	for tick in range(start_tick, target_tick):
		current_tick = tick
		
		_tick()

	current_tick = target_tick
	_earliest_updated_tick = target_tick

	while _states.size() > MAX_MESSAGE_DELAY * TICKS_PER_SECOND:
		_states.remove_at(0)
		_pruned_states += 1

	for id in _network_nodes.keys():
		var node = _network_nodes[id]

		if node.spawned:
			continue

		if node.despawned_tick > _pruned_states:
			continue

		_network_nodes.erase(id)
		node.get_parent().queue_free()


func spawn(scene: PackedScene) -> Node:
	var id = _last_network_node_id + 1

	if _network_nodes.has(id):
		_network_nodes[id].spawned = true
		register_network_node(_network_nodes[id])

		return _network_nodes[id].get_parent()

	var node = scene.instantiate()

	return node


func register_network_node(node: NetworkNode) -> void:
	node.id = _last_network_node_id + 1
	node.created_tick = current_tick
	node.scene_child = _initial_tick_time == -1

	_last_network_node_id += 1

	_network_nodes[node.id] = node