shader_type canvas_item;

uniform vec4 outline_color : source_color = vec4(0.0);
uniform vec4 highlight_color : source_color = vec4(1.0);

void fragment() {
	if( length(COLOR - outline_color) <= 0.01) {
		COLOR = highlight_color;
	}
}
