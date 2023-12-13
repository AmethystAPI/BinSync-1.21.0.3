extends SGCharacterBody2D
class_name Player


enum States { DEFAULT, HURT, DASH }


@export var EnemyScene: PackedScene


static var LocalPlayer: Player
static var Players = []


var SPEED = SGFixed.from_int(70)
var DASH_SPEED = SGFixed.from_int(600)


var KNOCKBACK_POWER = SGFixed.from_int(300)
var KNOCKBACK_DECAY = SGFixed.from_int(12)
var KNOCKBACK_MINIMUM = SGFixed.from_int(10)


var _tracked_position
var _tracked_state
var _tracked_dash_timer
var _tracked_knockback

var _health = 6


func _ready():
	_tracked_position = $NetworkNode.tracked_state(get_global_fixed_position(), _interpolate_position)
	_tracked_state = $NetworkNode.tracked_state(States.DEFAULT)
	_tracked_dash_timer = $NetworkNode.tracked_state(0.0)
	_tracked_knockback = $NetworkNode.tracked_state(SGFixedVector2.new())

	if $NetworkNode.has_authority():
		LocalPlayer = self

	Players.append(self)

	if $NetworkNode.has_authority():
		LocalPlayer = self


func _process(delta):
	pass
	# $ClientPlayer.global_position = _tracked_position.interpolated_value.to_float()
	# $ClientPlayer.global_position

func _go_to_state(state: States):
	if state == States.HURT:
		$ClientPlayer/AnimatedSprite.play("hurt")

	if state == States.DASH:
		_tracked_dash_timer.value = SGFixed.from_float(0.05)

	_tracked_state.value = state


func _resume_state():
	if _tracked_state.value == States.HURT:
		$ClientPlayer/AnimatedSprite.play("hurt")


func _on_handled_state_early():
	_tracked_state.value = _tracked_state.old_value
	_tracked_dash_timer.value = _tracked_dash_timer.old_value
	_tracked_knockback.value = _tracked_knockback.old_value


func _on_updated(input: TrackedValue):
	if input.value == null:
		return

	_default(input)
	_hurt(NetworkManager.delta())
	_dash(input)

	# if input.value.shoot and not input.old_value.shoot:
	# 	$Sword.shoot()

	move_and_slide()

	sync_to_physics_engine()
	

func _on_recorded_state():
	_tracked_position.value = get_global_fixed_position()


func _on_applied_state(input: TrackedValue):
	set_global_fixed_position(_tracked_position.value)

	_resume_state()

	sync_to_physics_engine()


func _default(input: TrackedValue):
	if _tracked_state.value != States.DEFAULT:
		return
		
	velocity = SGFixed.from_float_vector2(input.value.movement).mul(SPEED).mul(NetworkManager.delta())

	if input.value.point_direction.x > 0:
		$ClientPlayer/AnimatedSprite.scale.x = 1
	elif input.value.point_direction.x < 0:
		$ClientPlayer/AnimatedSprite.scale.x = -1
	
	if velocity.length() > 0:
		$ClientPlayer/AnimatedSprite.play("run")
	else:
		$ClientPlayer/AnimatedSprite.play("idle")

	if input.value.dash and input.old_value != null and not input.old_value.dash:
		_go_to_state(States.DASH)


func _hurt(delta):
	if _tracked_state.value != States.HURT:
		return
	
	velocity = _tracked_knockback.value.mul(NetworkManager.delta())
	_tracked_knockback.value = _tracked_knockback.value.linear_interpolate(SGFixedVector2.new(), SGFixed.mul(NetworkManager.delta(), KNOCKBACK_DECAY))
	
	if velocity.x > 0:
		$ClientPlayer/AnimatedSprite.scale.x = -1
	elif velocity.x < 0:
		$ClientPlayer/AnimatedSprite.scale.x = 1

	if _tracked_knockback.value.length() < KNOCKBACK_MINIMUM:
		_go_to_state(States.DEFAULT)


func _dash(input: TrackedValue):
	if _tracked_state.value != States.DASH:
		return

	velocity = SGFixed.from_float_vector2(input.value.point_direction).mul(DASH_SPEED).mul(NetworkManager.delta())

	$ClientPlayer/AnimatedSprite.play("idle")

	_tracked_dash_timer.value -= NetworkManager.delta()

	if _tracked_dash_timer.value <= 0:
		_go_to_state(States.DEFAULT)


func hurt(damage, source_position):
	if _tracked_state.value != States.DEFAULT:
		return

	_health -= damage
	_tracked_knockback.value = get_global_fixed_position().sub(source_position).normalized().mul(KNOCKBACK_POWER)

	_go_to_state(States.HURT)


func _interpolate_position(real_value, current_value, ticks_since_update, delta):
	return current_value.linear_interpolate(real_value, SGFixed.from_float(min(delta * 24, 1)))