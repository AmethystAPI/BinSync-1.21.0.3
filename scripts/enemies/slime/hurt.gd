extends State


var KNOCKBACK_POWER = SGFixed.from_int(300)
var KNOCKBACK_DECAY = SGFixed.from_int(12)
var KNOCKBACK_MINIMUM = SGFixed.from_int(10)


var _slime: SGCharacterBody2D


func _ready():
	_slime = get_parent().get_parent()


func _on_updated(_input: TrackedValue):
	_slime.velocity = _slime._tracked_knockback.value.mul(NetworkManager.delta())
	_slime._tracked_knockback.value = _slime._tracked_knockback.value.linear_interpolate(SGFixedVector2.new(), SGFixed.mul(NetworkManager.delta(), KNOCKBACK_DECAY))
	
	if _slime._tracked_knockback.value.length() < KNOCKBACK_MINIMUM:
		go_to_state("Idle")

	_slime.move_and_slide()
	_slime.sync_to_physics_engine()