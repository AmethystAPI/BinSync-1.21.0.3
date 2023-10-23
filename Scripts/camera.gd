extends Camera2D

func _process(delta):
	var offset = PixelCamera.target_position - PixelCamera.target_position.floor()
	
	# position = offset * 4

	print("update real camera ", PixelCamera.target_position)
