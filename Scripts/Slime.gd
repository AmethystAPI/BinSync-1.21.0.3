extends CharacterBody2D


enum State { IDLE, JUMP, HURT, LANDED }


const SPEED = 30.0


const KNOCKBACK_POWER = 300.0
const KNOCKBACK_DECAY = 12.0
const KNOCKBACK_MINIMUM = 10


var _state = State.IDLE


var _health = 2
var _knockback = Vector2.ZERO


func _go_to_state(state: State):
	_state = state

	if _state == State.IDLE:
		$"Jump Delay".start(RandomNumberGenerator.new().randf_range(0.5, 1))
		$"AnimationPlayer".play("idle")

	if _state == State.JUMP:
		$"AnimationPlayer".play("jump")

	if _state == State.HURT:
		$"AnimationPlayer".play("hurt")
		$"Jump Delay".stop()


func _ready():
	_go_to_state(State.IDLE)


func _idle():
	if _state != State.IDLE:
		return

	velocity = Vector2.ZERO


func _jump():
	if _state != State.JUMP:
		return
		
	var closestPlayer: CharacterBody2D = null
	
	for player in GameManager.Players:
		if closestPlayer == null or player.global_position.distance_to(global_position) < closestPlayer.global_position.distance_to(global_position):
			closestPlayer = player
			
	var direction = (closestPlayer.global_position - global_position).normalized()
	
	velocity = direction * SPEED

	if velocity.x > 0:
		$Sprite.scale.x = 1
	elif velocity.x < 0:
		$Sprite.scale.x = -1
	

func _landed():
	if _state != State.LANDED:
		return

	velocity = Vector2.ZERO


func _hurt(delta):
	if _state != State.HURT:
		return
	
	velocity = _knockback
	_knockback = _knockback.lerp(Vector2.ZERO, delta * KNOCKBACK_DECAY)
	
	if velocity.x > 0:
		$Sprite.scale.x = -1
	elif velocity.x < 0:
		$Sprite.scale.x = 1

	if _knockback.length() < KNOCKBACK_MINIMUM:
		_go_to_state(State.IDLE)


func _physics_process(delta):
	_idle()
	_jump()
	_landed()
	_hurt(delta)

	move_and_slide()


func _on_jump_delay_timeout():
	_go_to_state(State.JUMP)


func _land_jump():
	_go_to_state(State.LANDED)


func _end_jump():
	pass
	_go_to_state(State.IDLE)


func hurt(damage, source_position):	
	if _state != State.IDLE and _state != State.LANDED:
		return

	_health -= damage
	_knockback = (global_position - source_position).normalized() * KNOCKBACK_POWER
	
	if _health <= 0:
		queue_free()

	_go_to_state(State.HURT)
