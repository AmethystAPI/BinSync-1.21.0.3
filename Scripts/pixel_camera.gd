extends Camera2D
class_name PixelCamera

const SCREEN_OFFSET_FRACTION = 8
const LERP_SPEED = 16

static var target_position: Vector2

func _process(delta):
	if Player.LocalPlayer == null:
		return

	var mouse_offset = get_global_mouse_position() - Player.LocalPlayer.get_node("ClientPlayer").global_position
	# target_position = target_position.lerp(Player.LocalPlayer.global_position + mouse_offset / SCREEN_OFFSET_FRACTION, LERP_SPEED * delta)
	target_position = target_position.lerp(Player.LocalPlayer.get_node("ClientPlayer").global_position, LERP_SPEED * delta)
	# target_position = Player.LocalPlayer.get_node("ClientPlayer").global_position # + mouse_offset / SCREEN_OFFSET_FRACTION
	
	print("update pixel camera ", Player.LocalPlayer.get_node("ClientPlayer").global_position)

	global_position = target_position.floor()