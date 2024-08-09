shader_type canvas_item;

uniform vec4 border_color : source_color = vec4(1.0);
varying float base_alpha;

void vertex() {
	// Called for every vertex the material is visible on.
	base_alpha = COLOR.a;
}

void fragment() {
	if(COLOR.a == 0.0) {
		ivec2 size = textureSize(TEXTURE, 0);
	vec2 pixelUV = 1.0f / vec2(size);

	float alpha = texture(TEXTURE, vec2(UV.x + pixelUV.x, UV.y)).a;
	alpha = max(alpha, texture(TEXTURE, vec2(UV.x - pixelUV.x, UV.y)).a);
	alpha = max(alpha, texture(TEXTURE, vec2(UV.x, UV.y + pixelUV.y)).a);
	alpha = max(alpha, texture(TEXTURE, vec2(UV.x, UV.y - pixelUV.y)).a);

	alpha = max(alpha,  texture(TEXTURE, vec2(UV.x  + pixelUV.x, UV.y + pixelUV.y)).a);
	alpha = max(alpha, texture(TEXTURE, vec2(UV.x - pixelUV.x, UV.y + pixelUV.y)).a);
	alpha = max(alpha, texture(TEXTURE, vec2(UV.x + pixelUV.x, UV.y - pixelUV.y)).a);
	alpha = max(alpha,  texture(TEXTURE, vec2(UV.x - pixelUV.x, UV.y - pixelUV.y)).a);

		if(alpha > 0.0) {
			COLOR = vec4(border_color.r, border_color.g, border_color.b, border_color.a * base_alpha);
		}
	}
}

//void light() {
	// Called for every pixel for every light affecting the CanvasItem.
	// Uncomment to replace the default light processing function with this one.
//}
