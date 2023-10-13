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


func _go_to_state(state: State):
	_state = state

	if _state == State.HURT:
		$AnimatedSprite.play("hurt")

	if _state == State.DASH:
		velocity = Vector2(Input.get_axis("move_left", "move_right"), Input.get_axis("move_up", "move_down")).normalized() * DASH_SPEED

		$"Dash Timer".start()


func _ready():
	GameManager.Players.append(self)
	

func _input(event):
	if event.is_action_pressed("shoot"):		
		$Sword.shoot()

	if event.is_action_pressed("dash"):
		if _state != State.DEFAULT:
			return

		_go_to_state(State.DASH)


func _default():
	if _state != State.DEFAULT:
		return
		
	var movement = Vector2(Input.get_axis("move_left", "move_right"), Input.get_axis("move_up", "move_down")).normalized()
	
	velocity = movement * SPEED

	var mouse_position_offset = get_global_mouse_position() - global_position
	
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


func _physics_process(delta):
	_default()
	_hurt(delta)
	_dash()

	move_and_slide()


func _on_dash_timer_timeout():
	_go_to_state(State.DEFAULT)


func hurt(damage, source_position):
	if _state != State.DEFAULT:
		return

	_health -= damage
	_knockback = (global_position - source_position).normalized() * KNOCKBACK_POWER

	_go_to_state(State.HURT)