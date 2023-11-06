extends Control


func _process(delta):
	$RichTextLabel.text = str(NetworkManager.current_tick)
