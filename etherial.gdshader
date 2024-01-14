shader_type canvas_item;

uniform vec3 color: source_color;
uniform vec3 star_color: source_color;
uniform sampler3D noise;

float random (vec2 st) {
    return fract(sin(dot(st.xy, vec2(12.9898,78.233))) * 43758.5453123);
}

void fragment() {
	float aspect = SCREEN_PIXEL_SIZE.x / SCREEN_PIXEL_SIZE.y;
	
	float scale = floor(1.0 / SCREEN_PIXEL_SIZE.x / 4.0);
	
	float distortion = texture(noise, vec3(SCREEN_UV, mod(TIME  * 0.05, 1.0))).r - 0.5;
	
	vec2 sample_position = floor(
			(vec2(SCREEN_UV.x, SCREEN_UV.y * aspect) + distortion * 0.02)
			 * scale
		) / scale;

	vec3 pixel_color = color;
	
	if(random(sample_position) >= 0.997) {
		pixel_color = star_color;
	}
	
	COLOR = vec4(pixel_color.rgb, COLOR.a);
}