extends SGArea2D


# func _ready():
# 	var target = get_parent().get_parent()
# 	get_parent().remove_child(self)
# 	target.add_child(self)
	
# 	print(target.name)


func _on_applied_state(input: TrackedValue):
	sync_to_physics_engine()


func _on_updated(input: TrackedValue):
	sync_to_physics_engine()


	for body in get_overlapping_bodies():
		if body is Player:
			body.hurt(1, get_global_fixed_position())