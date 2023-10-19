extends CharacterBody2D
class_name Player


enum State { DEFAULT, HURT, DASH }


const SPEED = 100.0
const DASH_SPEED = 300.0


const KNOCKBACK_POWER = 300.0
const KNOCKBACK_DECAY = 12.0
const KNOCKBACK_MINIMUM = 10


var _tracked_position


var _state = State.DEFAULT
var _health = 6
var _knockback = Vector2.ZERO


func _go_to_state(state: State):
	_state = state

	if _state == State.HURT:
		$AnimatedSprite.play("hurt")

	if _state == State.DASH:
		# velocity = state.movement * DASH_SPEED

		$"Dash Timer".start()


func _ready():
	_tracked_position = $NetworkNode.tracked_state(global_position, _interpolate_position)
	print(global_position)

	GameManager.Players.append(self)


func _process(delta):
	$ClientPlayer.global_position = _tracked_position.interpolated_value


func _on_updated(input: TrackedValue):
	if input.value == null:
		return

	_default(input)
	# _hurt(1.0 / NetworkManager.TICKS_PER_SECOND)
	# _dash()

	move_and_slide()
	
func _on_recorded_state():
	_tracked_position.value = global_position

func _on_applied_state():
	global_position = _tracked_position.value

func _default(input: TrackedValue):
	if _state != State.DEFAULT:
		return
		
	velocity = input.value.movement * SPEED

	# var mouse_position_offset = state.mouse_position_offset
	
	# if mouse_position_offset.x > 0:
	# 	$AnimatedSprite.scale.x = 1
	# elif mouse_position_offset.x < 0:
	# 	$AnimatedSprite.scale.x = -1
	
	if velocity.length() > 0:
		$ClientPlayer/AnimatedSprite.play("run")
	else:
		$ClientPlayer/AnimatedSprite.play("idle")


func _hurt(delta):
	if _state != State.HURT:
		return
	
	velocity = _knockback
	_knockback = _knockback.lerp(Vector2.ZERO, delta * KNOCKBACK_DECAY)
	
	if velocity.x > 0:
		$AnimatedSprite.scale.x = -1
	elif velocity.x < 0:
		$AnimatedSprite.scale.x = 1

	if _knockback.length() < KNOCKBACK_MINIMUM:
		_go_to_state(State.DEFAULT)


func _dash():
	if _state != State.DASH:
		return

	$AnimatedSprite.play("idle")


func _on_dash_timer_timeout():
	_go_to_state(State.DEFAULT)


func hurt(damage, source_position):
	if _state != State.DEFAULT:
		return

	_health -= damage
	_knockback = (global_position - source_position).normalized() * KNOCKBACK_POWER

	_go_to_state(State.HURT)

func _interpolate_position(real_value, current_value, ticks_since_update, delta):
	return current_value.lerp(real_value, min(delta * 24, 1))