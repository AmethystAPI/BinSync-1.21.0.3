extends SGCharacterBody2D


enum State { IDLE, JUMP, HURT, LANDED }


var SPEED = SGFixed.from_int(30)

var KNOCKBACK_POWER = SGFixed.from_int(300)
var KNOCKBACK_DECAY = SGFixed.from_int(12)
var KNOCKBACK_MINIMUM = SGFixed.from_int(10)


var _tracked_position
var _tracked_knockback
var _tracked_state
var _tracked_jump_timer
var _tracked_health


func _ready():
	_tracked_position = $NetworkNode.tracked_state(get_global_fixed_position())
	_tracked_knockback = $NetworkNode.tracked_state(SGFixedVector2.new())
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
	_hurt()

	$"Damage Area/CollisionShape2D".disabled = _tracked_state.value != State.LANDED

	move_and_slide()

	sync_to_physics_engine()


func _on_recorded_state():
	_tracked_position.value = get_global_fixed_position()


func _on_applied_state(input: TrackedValue):
	set_global_fixed_position(_tracked_position.value)

	_resume_state()

	$"Damage Area/CollisionShape2D".disabled = _tracked_state.value != State.LANDED

	sync_to_physics_engine()

func _idle():
	if _tracked_state.value != State.IDLE:
		return

	velocity = SGFixedVector2.new()

	_tracked_jump_timer.value -= SGFixed.to_float(NetworkManager.delta())

	if _tracked_jump_timer.value <= 0:
		_go_to_state(State.JUMP)


func _jump():
	if _tracked_state.value != State.JUMP:
		return
		
	var closestPlayer: SGCharacterBody2D = null
	
	for player in Player.Players:
		if closestPlayer == null or player.get_global_fixed_position().distance_to(get_global_fixed_position()) < closestPlayer.get_global_fixed_position().distance_to(get_global_fixed_position()):
			closestPlayer = player
			
	var direction = closestPlayer.get_global_fixed_position().sub(get_global_fixed_position()).normalized()
	
	velocity = direction.mul(SPEED).mul(NetworkManager.delta())

	if velocity.x > 0:
		$Sprite.scale.x = 1
	elif velocity.x < 0:
		$Sprite.scale.x = -1

	_tracked_jump_timer.value -= SGFixed.to_float(NetworkManager.delta())

	if _tracked_jump_timer.value <= 0:
		_go_to_state(State.LANDED)
	

func _landed():
	if _tracked_state.value != State.LANDED:
		return

	velocity = SGFixedVector2.new()

	_tracked_jump_timer.value -= SGFixed.to_float(NetworkManager.delta())

	if _tracked_jump_timer.value <= 0:
		_go_to_state(State.IDLE)


func _hurt():
	if _tracked_state.value != State.HURT:
		return
	
	velocity = _tracked_knockback.value.mul(NetworkManager.delta())
	_tracked_knockback.value = _tracked_knockback.value.linear_interpolate(SGFixedVector2.new(), SGFixed.mul(NetworkManager.delta(), KNOCKBACK_DECAY))
	
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

	_tracked_knockback.value = get_global_fixed_position().sub(source_position).normalized().mul(KNOCKBACK_POWER)
	
	if _tracked_health.value <= 0:
		$NetworkNode.despawn()

	_go_to_state(State.HURT)
