extends Camera2D
class_name PixelCamera

const SCREEN_OFFSET_FRACTION = 8

static var target_position: Vector2

func _process(delta):
	var mouse_offset = get_global_mouse_position() - get_parent().global_position
	target_position = mouse_offset / SCREEN_OFFSET_FRACTION
	
	position = target_position.floor()
