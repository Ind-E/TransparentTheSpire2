shader_type canvas_item;

render_mode blend_premul_alpha;

uniform float brightness_boost = 5.0;
uniform float alpha_exp = 2.5;

void fragment() {
    vec4 tex = texture(TEXTURE, UV);

    float a = max(max(tex.r, tex.g), tex.b);

    a = pow(a, alpha_exp);

    vec3 final_rgb = tex.rgb * brightness_boost * COLOR.rgb;
    float final_a = a * COLOR.a;

    COLOR = vec4(final_rgb, final_a);
}

