extends Camera2D
class_name PlayerCamera


const SCREEN_OFFSET_FRACTION = 8
const LERP_SPEED = 4


var smoothed_player_position = Vector2(0, 0)


func _process(delta):
	if Player.LocalPlayer == null:
		return

	var mouse_offset = get_global_mouse_position() - Player.LocalPlayer.get_node("ClientPlayer").global_position

	smoothed_player_position = smoothed_player_position.lerp(Player.LocalPlayer.global_position, LERP_SPEED * delta)

	global_position = smoothed_player_position + mouse_offset / SCREEN_OFFSET_FRACTION