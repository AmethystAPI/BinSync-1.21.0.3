extends Node


signal recorded_input(input: TrackedValue)
signal started


const TICKS_PER_SECOND: float = 120
const MAX_MESSAGE_DELAY: float = 10


var players = []
var current_tick: int = 0


var _player_tracked_inputs = []

var _local_initial_tick_time: float = -1
var _initial_tick_time: float = -1
var _earliest_updated_tick = 1

var _is_setup = false

var _network_nodes = {}
var _next_network_node_id

var _debug_is_host = false
var _debug_mode = false
var _enter_debug_tick = 0


func _ready():
	_next_network_node_id = TrackedValue.new(0)


func _input(event):
	if event.is_action_pressed("network_debug"):
		_debug_mode = not _debug_mode

		if _debug_mode:
			_enter_debug_tick = current_tick
			print("Debug mode enabled")
		else:
			print("Debug mode disabled")

	if not _debug_mode:
		return

	if event.is_action_pressed("rewind_tick"):
		if current_tick == _enter_debug_tick - MAX_MESSAGE_DELAY * TICKS_PER_SECOND:
			return

		current_tick -= 1

		print("Rewound to tick: ", current_tick, " out of ", _enter_debug_tick)

		for node in _network_nodes.values():
			node._apply_state()

	if event.is_action_pressed("step_tick"):
		if current_tick == _enter_debug_tick:
			return

		current_tick += 1

		print("Stepped to tick: ", current_tick, " out of ", _enter_debug_tick)
				
		for node in _network_nodes.values():
			node._update()


func _process(_delta):
	var current_tick_time = Time.get_ticks_msec()

	if not _is_setup:
		return

	if _debug_mode:
		return

	var newest_tick = floor((current_tick_time - _initial_tick_time) / 1000.0 * TICKS_PER_SECOND) + 1

	var start_tick = min(current_tick + 1, _earliest_updated_tick)

	if start_tick > newest_tick:
		return

	if current_tick > start_tick - 1:
		# print(_id_debug(), "Rolling back from ", current_tick, " to ", start_tick - 1)

		current_tick = start_tick - 1

		for node in _network_nodes.values():
			node._apply_state()

	for tick in range(start_tick, newest_tick + 1):
		# print(_id_debug(), "Tick: ", tick)

		current_tick = tick

		_next_network_node_id.value = _next_network_node_id.old_value

		for index in range(players.size()):
			var tracked_input = _player_tracked_inputs[index]

			if players[index] == local_player():
				if tick == newest_tick:
					recorded_input.emit(tracked_input)

					tracked_input._set_updated()

					if tracked_input._should_update():
						# _update_input.rpc(tick, local_player(), tracked_input.value)

						var stored_tick = tick
						var stored_input = tracked_input.value

						var delay = func():
							await get_tree().create_timer(60.0 / 1000).timeout

							_update_input.rpc(stored_tick, local_player(), stored_input)

						delay.call()

			tracked_input.value = tracked_input.old_value

		for node in _network_nodes.values():
			node._handle_early_state()
				
		for node in _network_nodes.values():
			node._update()

		for node in _network_nodes.values():
			node._record_state()

	_earliest_updated_tick = newest_tick + 1


func local_player() -> int:
		return multiplayer.get_unique_id()


func is_host() -> bool:
		return multiplayer.is_server()


func delta() -> int:
	return SGFixed.div(SGFixed.ONE, SGFixed.from_float(TICKS_PER_SECOND))


func host() -> void:
	var peer = ENetMultiplayerPeer.new()

	print("Hosting on port 25566")
	
	var error = peer.create_server(25566, 2)
	if error != OK:
		print("Error hosting: " + str(error))
		return

	peer.get_host().compress(ENetConnection.COMPRESS_ZLIB)
 
	multiplayer.set_multiplayer_peer(peer)

	_debug_is_host = true


func join(address: String) -> void:
	var peer = ENetMultiplayerPeer.new()

	print("Connecting to ", address, " with port 25566")
	
	var error = peer.create_client(address, 25566)
	if error != OK:
		print("Error joining: " + str(error))
		return

	peer.get_host().compress(ENetConnection.COMPRESS_ZLIB)
	
	multiplayer.set_multiplayer_peer(peer)


func start() -> void:
	_setup.rpc([1] + Array(multiplayer.get_peers()))


func spawn(scene: PackedScene, authority: int = 1) -> Node:
	var node = scene.instantiate()
	var network_node = node.get_node("NetworkNode")

	network_node.id = _next_network_node_id.value
	_next_network_node_id.value += 1

	_network_nodes[network_node.id] = network_node

	network_node.give_authority(authority)
	network_node._spawn()

	return node

# We need to have an initial tick time that will be relatively identical across clients when doing Time.get_ticks_msec() - initial_tick_time
# So, we will ping the server and see what their time is. once we recieve their time we know that it is probably half the time since we requested the time out of date
# So, we can then use a corrected version of that time based on the time since we requested, to offset our initial time to match it up close to the server
# It won't be perfect but it will be close enough on a good connection
@rpc("any_peer", "call_local", "reliable")
func _setup(current_players):
	# await get_tree().create_timer(60.0 / 1000.0).timeout

	players = current_players

	for player in current_players:
		_player_tracked_inputs.append(TrackedValue.new(null))	

	_local_initial_tick_time = Time.get_ticks_msec()

	_ping_server.rpc_id(1, multiplayer.get_unique_id())


@rpc("any_peer", "call_local", "reliable")
func _ping_server(id):
	# await get_tree().create_timer(60.0 / 1000.0).timeout

	_ping_client.rpc_id(id, Time.get_ticks_msec())


@rpc("authority", "call_local", "reliable")
func _ping_client(time):
	var time_since_ping = Time.get_ticks_msec() - _local_initial_tick_time
	var predicted_time_offset = time_since_ping / 2
	var predicted_time = time + predicted_time_offset
	
	_initial_tick_time = Time.get_ticks_msec() - predicted_time

	_is_setup = true

	started.emit()

	print("Calculated initial tick time as ", _initial_tick_time, " with a latency of ", time_since_ping)

@rpc("any_peer", "call_remote", "reliable")
func _update_input(tick, player, input):
	if tick < current_tick - MAX_MESSAGE_DELAY * TICKS_PER_SECOND:
		return

	var player_index = players.find(player)
	var tracked_input = _player_tracked_inputs[player_index]

	tracked_input._update_value(tick, input)


func _id_debug():
	return "(" + str(multiplayer.get_unique_id()) + ") "