shader_type canvas_item;

uniform vec3 color: source_color;
uniform vec3 color2: source_color;
uniform vec3 star_color: source_color;
uniform sampler3D noise;
uniform vec2 size;

vec3 get_nebula_value(float offset, vec2 uv, float time) {
	float t = texture(noise, vec3(mod(uv / 1.5, 1.0), mod(time  * 0.08, 1.0))).r;
	
	return mix(color, color2, t);
}

float get_distortion(vec2 uv, float slice) {
	return texture(noise, vec3(mod(uv, 1.0), mod(slice, 1.0))).r - 0.5;
}

float random (vec2 st) {
    return fract(sin(dot(st.xy, vec2(12.9898,78.233))) * 43758.5453123);
}

float star_value(float offset, vec2 uv, float time) {
	float distortion = get_distortion(uv, time * 0.06);
	
	vec2 sample_position = floor(
		(
			vec2(
				uv.x + time * 0.008, uv.y - time * 0.008
			) + distortion * 0.04
		) * 300.0 + offset
	) / 300.0;
		
	float threshold = 0.004;
	
	return max(0, random(sample_position) - 1.0 + threshold) / threshold;
}

void fragment() {
	vec2 corrected_uv = UV * size / 300.0;
	
	float distortion = get_distortion(corrected_uv / 2.0, TIME * 0.05);
	
	vec2 distorted_uv = corrected_uv + distortion;
	
	float offset = 50.0;
	float limit = 400.0;
	float transition_speed = 0.2;

	float time_base = floor(TIME * transition_speed) * offset;

	float star_value1 = star_value(mod(time_base, limit), corrected_uv, TIME);
	float star_value2 = star_value(mod(time_base + offset, limit), corrected_uv, TIME);

	vec3 nebula_value = get_nebula_value(0.0, corrected_uv, TIME);
	
	//COLOR = vec4(nebula_value, 1.0);	
	
	float mixed_value = mix(star_value1, star_value2, mod(TIME * transition_speed, 1.0));
	
	COLOR = vec4(mix(nebula_value, star_color, mixed_value), COLOR.a);
}