shader_type canvas_item;

uniform float inner_radius;

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
	if(sqrt((UV.x - 0.5) * (UV.x - 0.5) + (UV.y - 0.5) * (UV.y - 0.5)) > 0.5) {
		COLOR = vec4(COLOR.rgb, 0.0);
	}

	if(sqrt((UV.x - 0.5) * (UV.x - 0.5) + (UV.y - 0.5) * (UV.y - 0.5)) < inner_radius) {
		COLOR = vec4(COLOR.rgb, 0.0);
	}
}

//void light() {
	// Called for every pixel for every light affecting the CanvasItem.
	// Uncomment to replace the default light processing function with this one.
//}
