extends CharacterBody2D


enum State { IDLE, JUMP, HURT, LANDED }


const SPEED = 30.0


const KNOCKBACK_POWER = 300.0
const KNOCKBACK_DECAY = 12.0
const KNOCKBACK_MINIMUM = 10


var _tracked_position
var _tracked_knockback


var _state = State.IDLE

var _health = 8


func _go_to_state(state: State):
	_state = state

	if _state == State.IDLE:
		# $"Jump Delay".start(RandomNumberGenerator.new().randf_range(0.5, 1))
		$"AnimationPlayer".play("idle")

	if _state == State.JUMP:
		$"AnimationPlayer".play("jump")

	if _state == State.HURT:
		$"AnimationPlayer".play("hurt")
		$"Jump Delay".stop()


func _ready():
	_tracked_position = $NetworkNode.tracked_state(global_position)
	_tracked_knockback = $NetworkNode.tracked_state(Vector2.ZERO)

	_go_to_state(State.IDLE)


func _on_handled_early_state():
	_tracked_knockback.value = _tracked_knockback.old_value


func _on_updated(input: TrackedValue):
	_idle()
	# _jump()
	# _landed()
	_hurt(NetworkManager.delta())

	move_and_slide()


func _on_recorded_state():
	_tracked_position.value = global_position


func _on_applied_state():
	global_position = _tracked_position.value


func _idle():
	if _state != State.IDLE:
		return

	velocity = Vector2.ZERO


func _jump():
	if _state != State.JUMP:
		return
		
	var closestPlayer: CharacterBody2D = null
	
	for player in Player.Players:
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
	
	velocity = _tracked_knockback.value
	_tracked_knockback.value = _tracked_knockback.value.lerp(Vector2.ZERO, delta * KNOCKBACK_DECAY)
	
	if velocity.x > 0:
		$Sprite.scale.x = -1
	elif velocity.x < 0:
		$Sprite.scale.x = 1

	if _tracked_knockback.value.length() < KNOCKBACK_MINIMUM:
		_go_to_state(State.IDLE)


func _on_jump_delay_timeout():
	_go_to_state(State.JUMP)


func _land_jump():
	_go_to_state(State.LANDED)


func _end_jump():
	_go_to_state(State.IDLE)


func hurt(damage, source_position):	
	if _state != State.IDLE and _state != State.LANDED:
		return

	_health -= damage
	_tracked_knockback.value = (global_position - source_position).normalized() * KNOCKBACK_POWER
	
	if _health <= 0:
		$NetworkNode.despawn()

	_go_to_state(State.HURT)
