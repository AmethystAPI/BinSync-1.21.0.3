extends CharacterBody2D
class_name Player


enum State { DEFAULT, HURT, DASH }


const SPEED = 100.0
const DASH_SPEED = 300.0


const KNOCKBACK_POWER = 300.0
const KNOCKBACK_DECAY = 12.0
const KNOCKBACK_MINIMUM = 10


var _state = State.DEFAULT
var _health = 6
var _knockback = Vector2.ZERO

var _shoot_presses = 0
var _dash_pressed = false


func _go_to_state(state: State):
	_state = state

	if _state == State.HURT:
		$AnimatedSprite.play("hurt")

	if _state == State.DASH:
		velocity = Vector2(Input.get_axis("move_left", "move_right"), Input.get_axis("move_up", "move_down")).normalized() * DASH_SPEED

		$"Dash Timer".start()


func _input(event):
	if event.is_action_pressed("shoot"):
		_shoot_presses += 1

	if event.is_action_pressed("dash"):
		_dash_pressed = true


func _ready():
	GameManager.Players.append(self)

	
func _record_state(state, old_state):
	state.state = _state
	state.position = global_position
	state.knockback = _knockback
	state.health = _health

	if $NetworkNode.has_authority():
		state.movement = Vector2(Input.get_axis("move_left", "move_right"), Input.get_axis("move_up", "move_down")).normalized()
		state.mouse_position_offset = get_global_mouse_position() - global_position
		
		state.shoot_presses = _shoot_presses
		_shoot_presses = 0

		state.dash_pressed = _dash_pressed
		_dash_pressed = false
	elif old_state != null:
		state.movement = old_state.movement
		state.mouse_position_offset = old_state.mouse_position_offset
		state.shoot_presses = 0
		state.dash_pressed = false
	else:
		state.movement = Vector2.ZERO
		state.mouse_position_offset = Vector2.ZERO
		state.shoot_presses = 0
		state.dash_pressed = false


func _on_tick(state):
	_state = state.state
	global_position = state.position
	_knockback = state.knockback
	_health = state.health

	if state.dash_pressed:
		if _state != State.DEFAULT:
			return

		_go_to_state(State.DASH)

	for i in range(state.shoot_presses):
		$Sword.shoot()

	_default(state)
	_hurt(1.0 / NetworkManager.TICKS_PER_SECOND)
	_dash()

	move_and_slide()


func _default(state):
	if _state != State.DEFAULT:
		return
		
	var movement = state.movement
	
	velocity = movement * SPEED

	var mouse_position_offset = state.mouse_position_offset
	
	if mouse_position_offset.x > 0:
		$AnimatedSprite.scale.x = 1
	elif mouse_position_offset.x < 0:
		$AnimatedSprite.scale.x = -1
	
	if velocity.length() > 0:
		$AnimatedSprite.play("run")
	else:
		$AnimatedSprite.play("idle")


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
