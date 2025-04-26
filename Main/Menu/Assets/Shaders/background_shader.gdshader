shader_type canvas_item;

uniform vec4 highlight_color: source_color;

uniform sampler3D noise_img_3d;
uniform sampler2D noise_img;
uniform float blur_size = 5.0;
uniform float edge_start = 0.1; // Начало перехода прозрачности
uniform float edge_end = 0.6;   // Конец перехода прозрачности

float value(vec3 color) {
    return (color.r + color.g + color.b) / 3.0;
}

void fragment() {
    vec2 pixel_size = 1.0 / vec2(textureSize(noise_img, 0));

    // Бокс-размытие 3x3
    vec3 col = vec3(0.0);
    for(int x = -1; x <= 1; x++) {
        for(int y = -1; y <= 1; y++) {
            col += texture(noise_img, UV + pixel_size * vec2(float(x), float(y)) * blur_size).rgb;
        }
    }
    col /= 9.0;

    // Рассчитываем прозрачность
    float val = value(col);
    float alpha = 1.0 - smoothstep(edge_start, edge_end, val);

    COLOR = highlight_color * vec4(1.0, 1.0, 1.0, alpha);
}