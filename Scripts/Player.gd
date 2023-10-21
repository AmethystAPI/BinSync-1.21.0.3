extends CharacterBody2D
class_name Player


enum State { DEFAULT, HURT, DASH }


static var LocalPlayer: Player
static var Players = []


const SPEED = 100.0
const DASH_SPEED = 300.0


const KNOCKBACK_POWER = 300.0
const KNOCKBACK_DECAY = 12.0
const KNOCKBACK_MINIMUM = 10


var _tracked_position
var _tracked_state
var _tracked_dash_timer

var _health = 6

var _knockback = Vector2.ZERO


func _ready():
	_tracked_position = $NetworkNode.tracked_state(global_position)
	_tracked_state = $NetworkNode.tracked_state(State.DEFAULT)
	_tracked_dash_timer = $NetworkNode.tracked_state(0.0)

	if $NetworkNode.has_authority():
		LocalPlayer = self

	Players.append(self)


# func _process(delta):
	# $ClientPlayer.global_position = _tracked_position.interpolated_value


func _go_to_state(state: State):
	if state == State.HURT:
		$ClientPlayer/AnimatedSprite.play("hurt")

	if state == State.DASH:
		_tracked_dash_timer.value = 0.15

	_tracked_state.value = state


func _resume_to_state():
	if _tracked_state.value == State.HURT:
		$ClientPlayer/AnimatedSprite.play("hurt")


func _on_handled_state_early():
	_tracked_state.value = _tracked_state.old_value
	_tracked_dash_timer.value = _tracked_dash_timer.old_value


func _on_updated(input: TrackedValue):
	if input.value == null:
		return

	_default(input)
	_hurt(NetworkManager.delta())
	_dash(input)

	if input.value.shoot and not input.old_value.shoot:
		$ClientPlayer/Sword.shoot()

	move_and_slide()
	

func _on_recorded_state():
	_tracked_position.value = global_position


func _on_applied_state():
	global_position = _tracked_position.value

	_resume_to_state()


func _default(input: TrackedValue):
	if _tracked_state.value != State.DEFAULT:
		return
		
	velocity = input.value.movement * SPEED

	if input.value.point_direction.x > 0:
		$ClientPlayer/AnimatedSprite.scale.x = 1
	elif input.value.point_direction.x < 0:
		$ClientPlayer/AnimatedSprite.scale.x = -1
	
	if velocity.length() > 0:
		$ClientPlayer/AnimatedSprite.play("run")
	else:
		$ClientPlayer/AnimatedSprite.play("idle")


func _hurt(delta):
	if _tracked_state.value != State.HURT:
		return
	
	velocity = _knockback
	_knockback = _knockback.lerp(Vector2.ZERO, delta * KNOCKBACK_DECAY)
	
	if velocity.x > 0:
		$ClientPlayer/AnimatedSprite.scale.x = -1
	elif velocity.x < 0:
		$ClientPlayer/AnimatedSprite.scale.x = 1

	if _knockback.length() < KNOCKBACK_MINIMUM:
		_go_to_state(State.DEFAULT)


func _dash(input: TrackedValue):
	if _tracked_state.value != State.DASH:
		return

	velocity = input.movement * DASH_SPEED

	$ClientPlayer/AnimatedSprite.play("idle")

	_tracked_dash_timer.value -= NetworkManager.delta()

	if _tracked_dash_timer.value <= 0:
		_go_to_state(State.DEFAULT)


func hurt(damage, source_position):
	if _tracked_state.value != State.DEFAULT:
		return

	_health -= damage
	_knockback = (global_position - source_position).normalized() * KNOCKBACK_POWER

	_go_to_state(State.HURT)


func _interpolate_position(real_value, current_value, ticks_since_update, delta):
	return current_value.lerp(real_value, min(delta * 24, 1))