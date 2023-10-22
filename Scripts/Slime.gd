extends CharacterBody2D


enum State { IDLE, JUMP, HURT, LANDED }


const SPEED = 30.0


const KNOCKBACK_POWER = 300.0
const KNOCKBACK_DECAY = 12.0
const KNOCKBACK_MINIMUM = 10


var _tracked_position
var _tracked_knockback
var _tracked_state
var _tracked_jump_timer
var _tracked_health


func _ready():
	_tracked_position = $NetworkNode.tracked_state(global_position)
	_tracked_knockback = $NetworkNode.tracked_state(Vector2.ZERO)
	_tracked_state = $NetworkNode.tracked_state(State.IDLE)
	_tracked_jump_timer = $NetworkNode.tracked_state(1.0)
	_tracked_health = $NetworkNode.tracked_state(3)

	_go_to_state(State.IDLE)


func _go_to_state(state: State):
	if state == State.IDLE:
		_tracked_jump_timer.value = 1.0
		$"AnimationPlayer".play("idle")

	if state == State.JUMP:
		_tracked_jump_timer.value = 0.6
		$"AnimationPlayer".play("jump")

	if state == State.LANDED:
		_tracked_jump_timer.value = 0.3
		$"AnimationPlayer".play("jump")

	if state == State.HURT:
		$"AnimationPlayer".play("hurt")

	_tracked_state.value = state


func _resume_state():
	if _tracked_state.value == State.IDLE:
		$"AnimationPlayer".play("idle")

	if _tracked_state.value == State.JUMP:
		$"AnimationPlayer".play("jump")

	if _tracked_state.value == State.HURT:
		$"AnimationPlayer".play("hurt")


func _on_handled_early_state():
	_tracked_knockback.value = _tracked_knockback.old_value
	_tracked_state.value = _tracked_state.old_value
	_tracked_jump_timer.value = _tracked_jump_timer.old_value
	_tracked_health.value = _tracked_health.old_value


func _on_updated(input: TrackedValue):
	_idle()
	_jump()
	_landed()
	_hurt(NetworkManager.delta())

	$"Damage Area/CollisionShape2D".disabled = _tracked_state.value == State.LANDED

	move_and_slide()


func _on_recorded_state():
	_tracked_position.value = global_position


func _on_applied_state():
	global_position = _tracked_position.value

	_resume_state()


func _idle():
	if _tracked_state.value != State.IDLE:
		return

	velocity = Vector2.ZERO

	_tracked_jump_timer.value -= NetworkManager.delta()

	if _tracked_jump_timer.value <= 0:
		_go_to_state(State.JUMP)


func _jump():
	if _tracked_state.value != State.JUMP:
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

	_tracked_jump_timer.value -= NetworkManager.delta()

	if _tracked_jump_timer.value <= 0:
		_go_to_state(State.LANDED)
	

func _landed():
	if _tracked_state.value != State.LANDED:
		return

	velocity = Vector2.ZERO

	_tracked_jump_timer.value -= NetworkManager.delta()

	if _tracked_jump_timer.value <= 0:
		_go_to_state(State.IDLE)


func _hurt(delta):
	if _tracked_state.value != State.HURT:
		return
	
	velocity = _tracked_knockback.value
	_tracked_knockback.value = _tracked_knockback.value.lerp(Vector2.ZERO, delta * KNOCKBACK_DECAY)
	
	if velocity.x > 0:
		$Sprite.scale.x = -1
	elif velocity.x < 0:
		$Sprite.scale.x = 1

	if _tracked_knockback.value.length() < KNOCKBACK_MINIMUM:
		_go_to_state(State.IDLE)


func hurt(damage, source_position):	
	if _tracked_state.value != State.IDLE and _tracked_state.value != State.LANDED:
		return

	_tracked_health.value -= damage
	_tracked_knockback.value = (global_position - source_position).normalized() * KNOCKBACK_POWER
	
	if _tracked_health.value <= 0:
		$NetworkNode.despawn()

	_go_to_state(State.HURT)
