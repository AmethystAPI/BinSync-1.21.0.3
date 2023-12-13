class_name StateMachine
extends Node

@export var network_node: NetworkNode
@export var default_state: String = "Idle"


var _tracked_current_state: TrackedValue
var _current_state: State = null


func _ready():
	network_node.handled_early_state.connect(_on_handled_early_state)
	network_node.updated.connect(_on_updated)
	network_node.recorded_state.connect(_on_recorded_state)
	network_node.applied_state.connect(_on_applied_state)

	_tracked_current_state = network_node.tracked_state("none")
	
	go_to_state(default_state)


func go_to_state(state: String):
	var new_state = get_node(state)

	if new_state == null:
		push_error("Tried to go to state " + state + " from " + _tracked_current_state.value + " but it doesn't exist!")

		return

	_tracked_current_state.value = state

	if _current_state != null:
		_current_state._exit()

	_current_state = new_state

	_current_state._state_machine = self

	_current_state._enter()


func _on_handled_early_state():
	_tracked_current_state.value = _tracked_current_state.old_value


func _on_updated(input: TrackedValue):
	_current_state._on_updated(input)


func _on_recorded_state():
	_current_state._on_recorded_state()


func _on_applied_state(input: TrackedValue):
	_current_state._on_reverted_state(input)

	_current_state = get_node(_tracked_current_state.value)
	
	_current_state._on_resumed_state(input)
