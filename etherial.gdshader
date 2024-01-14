shader_type canvas_item;

uniform vec3 color: source_color;
uniform vec3 star_color: source_color;
uniform sampler3D noise;

float random (vec2 st) {
    return fract(sin(dot(st.xy, vec2(12.9898,78.233))) * 43758.5453123);
}

float star_value(float offset, vec2 screen_pixel_size, vec2 screen_uv, float time) {
	float aspect = screen_pixel_size.x / screen_pixel_size.y;
	
	float scale = floor(1.0 / screen_pixel_size.x / 4.0);
	
	float distortion = texture(noise, vec3(screen_uv, mod(time  * 0.1, 1.0))).r - 0.5;
	
	vec2 sample_position = floor(
			(vec2(screen_uv.x + time * 0.01, screen_uv.y * aspect - time * 0.01) + distortion * 0.04)
			 * scale + offset
		) / scale;
		
	float threshold = 0.003;
	
	return max(0, random(sample_position) - 1.0 + threshold) / threshold;
}

void fragment() {
	vec3 pixel_color = color;
	
	float offset = 50.0;
	float limit = 400.0;
	float transition_speed = 0.2;
	
	float time_base = floor(TIME * transition_speed) * offset;
	
	float star_value1 = star_value(mod(time_base, limit), SCREEN_PIXEL_SIZE, SCREEN_UV, TIME);
	float star_value2 = star_value(mod(time_base + offset, limit), SCREEN_PIXEL_SIZE, SCREEN_UV, TIME);
	
	float mixed_value = mix(star_value1, star_value2, mod(TIME * transition_speed, 1.0));
	
	COLOR = vec4(mix(color, star_color, mixed_value), COLOR.a);
}