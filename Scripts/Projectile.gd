extends CharacterBody2D

@export var SPEED = 200

func _physics_process(delta):
	velocity = global_transform.x * SPEED
	
	move_and_slide()

func _on_area_2d_body_entered(body):
	if not body.is_in_group("Entities"):
		return
		
	body.hurt(1, global_position)


func _on_destroy_timer_timeout():
	queue_free()
