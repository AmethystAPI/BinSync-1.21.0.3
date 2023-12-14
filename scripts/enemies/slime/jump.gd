extends State


@export var network_node: NetworkNode


var SPEED: int = SGFixed.from_int(40)
const JUMP_TIME = 0.6
const JUMP_HEIGHT = 16


var _slime: SGCharacterBody2D
var _tracked_jump_timer: TrackedValue


func _ready():
	_slime = get_parent().get_parent()

	_tracked_jump_timer = network_node.tracked_state(0)


func _enter():
	_tracked_jump_timer.value = SGFixed.from_int(0)


func _on_handled_early_state():
	_tracked_jump_timer.value = _tracked_jump_timer.old_value


func _on_updated(_input: TrackedValue):
	_tracked_jump_timer.value += NetworkManager.delta()

	var sprite: Sprite2D = _slime.get_node("Sprite")

	var animation_progress = min(SGFixed.to_float(_tracked_jump_timer.value) / JUMP_TIME, 1)

	if animation_progress <= JUMP_TIME / 2.0:
		sprite.position = Vector2.UP * sin(animation_progress * PI) * JUMP_HEIGHT
	else:
		sprite.position = Vector2.UP * pow(sin(animation_progress * PI), 0.8) * JUMP_HEIGHT

	var target: Player = Player.Players[0]

	for player in Player.Players:
		if player.get_global_fixed_position().distance_to(_slime.get_global_fixed_position()) > target.get_global_fixed_position().distance_to(_slime.get_global_fixed_position()):
			continue
		
		target = player

	var jump_timer_over = _tracked_jump_timer.value >= SGFixed.from_float(JUMP_TIME)

	if jump_timer_over:
		go_to_state("Idle")
	else:
		var direction: SGFixedVector2 = target.get_global_fixed_position().sub(_slime.get_global_fixed_position()).normalized()
		_slime.velocity = direction.mul(SPEED).mul(NetworkManager.delta())
		
	if jump_timer_over:
		_tracked_jump_timer.value = SGFixed.from_float(0)

	_slime.move_and_slide()
	_slime.sync_to_physics_engine()


func _on_reverted_state(_input: TrackedValue):
	var sprite: Sprite2D = _slime.get_node("Sprite")

	sprite.position = Vector2.ZERO