// Displays a grid overlay on top of a tilemap, and highlights the grid near the mouse cursor
// Based on https://godotshaders.com/shader/repeated-texture-overlay-for-tilemaps/ by jess.codes
shader_type canvas_item;

// The grid is just a repeated texture
// The texture is drawn on top of the tilemap, so you can use transparency in the grid texture
// For pixel perfect games, you need the grid texture to have the same size as the tiles, and the grid cannot be perfectly centered on your tiles (or you have double width edges)
// If you don't need pixel perfect, remove `filter_nearest` and use a hollow square texture that's twice the width/height of the tilemap
uniform sampler2D overlay_texture: repeat_enable, filter_nearest;

// Scale for sampling the grid texture, should correspond to 1 / tileset_size_in_px
uniform float scale = 0.015625; // 1/16
// The opacity of the grid overlay
// Set to 0 to hide the base grid and only use the mouse highlight
uniform float base_opacity = 0.1;
// The extra opacity added to the grid overlay near the mouse
// Set to 0 to disable mouse highlight
uniform float highlight_opacity = 0.4;
// The distance to the mouse after which no extra opacity is added
uniform float distance_scaling = 50.0;
// The color on which the grid is allowed to be shown. This allows details in your tilemap to be unaffected
uniform vec3 floor_color: source_color;
uniform bool show_overlay = false; // Whether to show the overlay at all, can be toggled by the playfield when placing or not placing
// The local mouse position on the tilemap layer. Needs to be set by an external script:
// func _process(delta: float) -> void:
//	(tilemap_layer.material as ShaderMaterial).set_shader_parameter("mouse_position", tilemap_layer.get_local_mouse_position())
uniform vec2 mouse_position;

varying vec2 world_position;

void vertex(){
	// calculate the world position for use in the fragment shader
	world_position = (MODEL_MATRIX * vec4(VERTEX, 0.0, 1.0)).xy;
}

void fragment() {

	vec4 color = texture(TEXTURE, UV);
	// Only process pixels with the specificed floor color
	// We use distance because floats cannot be compared for equality due to precision errors, and we ignore transparent pixels
	if ( color.a > 0.1 && show_overlay) {
		vec4 grid_color = texture(overlay_texture, world_position * scale);
		float distance_to_mouse = distance(mouse_position, world_position);
		// We get a distance from [0, 1] that indicates how close the pixel is to the mouse
		float normalized = 1.0 - (clamp(distance_to_mouse, 0, distance_scaling) / distance_scaling);
		// Mix the regular color with the grid to preserve the original texture and just "add" the grid on top
		COLOR = mix(color, grid_color, grid_color.a * (base_opacity + highlight_opacity * normalized));
	}
}

