class_name State
extends Node


var _state_machine: StateMachine


func go_to_state(state: String):
	_state_machine.go_to_state(state)


func _enter():
	pass


func _exit():
	pass


func _on_updated(_input: TrackedValue):
	pass


func _on_recorded_state():
	pass


func _on_resumed_state(_input: TrackedValue):
	pass


func _on_reverted_state(_input: TrackedValue):
	pass