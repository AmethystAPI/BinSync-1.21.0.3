class_name StateMachine
extends Node

@export var network_node: NetworkNode
@export var default_state: String = "Idle"


var tracked_current_state: TrackedValue
var current_state: State = null


func _ready():
	network_node.handled_early_state.connect(_on_handled_early_state)
	network_node.updated.connect(_on_updated)
	network_node.recorded_state.connect(_on_recorded_state)
	network_node.applied_state.connect(_on_applied_state)

	tracked_current_state = network_node.tracked_state(default_state)
	
	go_to_state(default_state)


func go_to_state(state: String):
	var new_state = get_node(state)

	if new_state == null:
		push_error("Tried to go to state " + state + " from " + tracked_current_state.value + " but it doesn't exist!")

		return

	tracked_current_state.value = state

	if current_state != null:
		current_state._exit()

	current_state = new_state

	current_state._state_machine = self

	current_state._enter()


func _on_handled_early_state():
	tracked_current_state.value = tracked_current_state.old_value

	current_state._on_handled_early_state()


func _on_updated(input: TrackedValue):
	current_state._on_updated(input)


func _on_recorded_state():
	current_state._on_recorded_state()


func _on_applied_state(input: TrackedValue):
	current_state._on_reverted_state(input)

	current_state = get_node(tracked_current_state.value)
	
	current_state._on_resumed_state(input)
