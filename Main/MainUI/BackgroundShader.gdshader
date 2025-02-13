shader_type canvas_item;

const vec4 highlight_color = vec4(242.0/255.0, 204.0/255.0, 143.0/255.0, 1.0);
uniform sampler2D noise_img;

void fragment() {
	vec2 uv = vec2(UV.x + TIME / 10.0, UV.y);
	
	float noise_r = texture(noise_img, uv).r;
	float noise_g = texture(noise_img, uv).g;
	float noise_b = texture(noise_img, uv).b;
	
	vec3 new_color = vec3(noise_r, noise_g, noise_b);
	COLOR.rgb = new_color;
	COLOR.a = texture(TEXTURE, UV).a;
}