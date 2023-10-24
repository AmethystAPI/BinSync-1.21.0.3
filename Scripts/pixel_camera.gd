extends Camera2D
class_name PixelCamera


const SCREEN_OFFSET_FRACTION = 8
const LERP_SPEED = 16


var smoothed_player_position = Vector2(0, 0)


func _process(delta):
	if Player.LocalPlayer == null:
		return

	var mouse_offset = get_global_mouse_position() - Player.LocalPlayer.get_node("ClientPlayer").global_position

	# global_position = global_position.lerp(Player.LocalPlayer.get_node("ClientPlayer").global_position + mouse_offset / SCREEN_OFFSET_FRACTION, LERP_SPEED * delta)
	# global_position = Player.LocalPlayer.get_node("ClientPlayer").global_position + mouse_offset / SCREEN_OFFSET_FRACTION

	smoothed_player_position = smoothed_player_position.lerp(Player.LocalPlayer.global_position, LERP_SPEED * delta)

	global_position = smoothed_player_position + mouse_offset / SCREEN_OFFSET_FRACTION