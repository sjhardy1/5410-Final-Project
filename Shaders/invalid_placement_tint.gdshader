shader_type canvas_item;

uniform vec4 tint_color : source_color = vec4(1.0, 0.18, 0.18, 0.85);
uniform float tint_strength = 0.65;

void fragment() {
    vec4 tex = texture(TEXTURE, UV);
    vec3 blended = mix(tex.rgb, tint_color.rgb, clamp(tint_strength, 0.0, 1.0));
    COLOR = vec4(blended, tex.a);
}
