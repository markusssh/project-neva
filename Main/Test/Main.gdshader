shader_type canvas_item;

uniform float border_thickness = 0.05;
uniform float max_darkness_dec = 0.05;

void fragment() {
	float place_above_sin_func = sin(UV.x * 4.0) - UV.y * 2.0 * sin(TIME / 1.0);
	if (place_above_sin_func <= border_thickness && place_above_sin_func > 0.0) {
		COLOR.rgb -= vec3(min(border_thickness - place_above_sin_func, max_darkness_dec));
	}
	else if (place_above_sin_func <= 0.0) {
		COLOR.rgb -= vec3(max_darkness_dec);
	}
}