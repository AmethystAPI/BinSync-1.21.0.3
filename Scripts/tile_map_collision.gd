extends Node


@export var colliding_layers = ["Wall", "Wall Blocking"]


var resources = []


func _ready():
	var tile_map: TileMap = get_parent()

	var width = int(tile_map.tile_set.tile_size.x)
	var height = int(tile_map.tile_set.tile_size.y)

	var extents = SGFixed.vector2(SGFixed.from_int(width / 2), SGFixed.from_int(height / 2))
	var shape_transform = SGFixed.transform2d(0, extents)

	for layer in tile_map.get_layers_count():
		if not colliding_layers.has(tile_map.get_layer_name(layer)):
			continue

		for tile_position in tile_map.get_used_cells(layer):
			var shape = SGPhysics2DServer.shape_create(SGPhysics2DServer.SHAPE_RECTANGLE)

			SGPhysics2DServer.rectangle_set_extents(shape, extents)
			SGPhysics2DServer.shape_set_transform(shape, shape_transform)

			resources.push_back(shape)

			var object = SGPhysics2DServer.collision_object_create(SGPhysics2DServer.OBJECT_BODY, SGPhysics2DServer.BODY_STATIC)

			SGPhysics2DServer.collision_object_add_shape(object, shape)
			SGPhysics2DServer.collision_object_set_transform(object, SGFixed.transform2d(0,
				SGFixed.vector2(SGFixed.from_int(int(tile_position.x) * width), SGFixed.from_int(int(tile_position.y) * height))))

			SGPhysics2DServer.collision_object_set_data(object, "tile_map-%sx%s" % [int(tile_position.x), int(tile_position.y)])
			resources.push_back(object)

			SGPhysics2DServer.world_add_collision_object(SGPhysics2DServer.get_default_world(), object)


func _exit_tree():
	for rid in resources:
		SGPhysics2DServer.free_rid(rid)