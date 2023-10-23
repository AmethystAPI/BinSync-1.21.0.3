extends Camera2D
class_name PixelCamera

const SCREEN_OFFSET_FRACTION = 8

static var target_position: Vector2

func _process(delta):
	if Player.LocalPlayer == null:
		return

	var mouse_offset = get_global_mouse_position() - Player.LocalPlayer.global_position
	target_position = mouse_offset / SCREEN_OFFSET_FRACTION
	
	position = Player.LocalPlayer.global_position + target_position.floor()