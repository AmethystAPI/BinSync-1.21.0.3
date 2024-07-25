shader_type canvas_item;

uniform vec4 color : source_color = vec4(1.0);

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
	ivec2 size = textureSize(TEXTURE, 0);
	vec2 pixelUV = 1.0f / vec2(size);

	//COLOR = texture(TEXTURE, UV);

	//float size = 200.0;

	if(COLOR.a == 0.0) {
		float alpha = texture(TEXTURE, vec2(UV.x + pixelUV.x, UV.y)).a;
		alpha = max(alpha, texture(TEXTURE, vec2(UV.x - pixelUV.x, UV.y)).a);
		alpha = max(alpha, texture(TEXTURE, vec2(UV.x, UV.y + pixelUV.y)).a);
		alpha = max(alpha, texture(TEXTURE, vec2(UV.x, UV.y - pixelUV.y)).a);

		alpha = max(alpha,  texture(TEXTURE, vec2(UV.x  + pixelUV.x, UV.y + pixelUV.y)).a);
		alpha = max(alpha, texture(TEXTURE, vec2(UV.x - pixelUV.x, UV.y + pixelUV.y)).a);
		alpha = max(alpha, texture(TEXTURE, vec2(UV.x + pixelUV.x, UV.y - pixelUV.y)).a);
		alpha = max(alpha,  texture(TEXTURE, vec2(UV.x - pixelUV.x, UV.y - pixelUV.y)).a);

		if(alpha > 0.0) {
			//COLOR = vec4(1.0, 0.0, 0.0, alpha);
		}
	}
}

//void light() {
	// Called for every pixel for every light affecting the CanvasItem.
	// Uncomment to replace the default light processing function with this one.
//}
