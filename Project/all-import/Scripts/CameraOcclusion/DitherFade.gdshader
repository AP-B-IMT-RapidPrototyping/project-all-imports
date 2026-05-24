shader_type spatial;
render_mode depth_draw_opaque, cull_back, diffuse_burley, specular_schlick_ggx;

// Tint shown when an object is occluding the player. Defaults to a near-neutral
// grey so the silhouette is still readable. Set alpha < 1 in code to fade.
uniform vec4 albedo : source_color = vec4(0.6, 0.6, 0.6, 1.0);
uniform sampler2D albedo_texture : source_color, hint_default_white;

// 1.0 = fully visible, 0.0 = fully invisible. In between, Bayer-matrix dither
// discards a screen-space subset of pixels.
uniform float fade : hint_range(0.0, 1.0) = 1.0;

// Bayer 4x4 ordered dither matrix (values 0..15 normalised to 0..1).
const float bayer4x4[16] = float[16](
	 0.0/16.0,  8.0/16.0,  2.0/16.0, 10.0/16.0,
	12.0/16.0,  4.0/16.0, 14.0/16.0,  6.0/16.0,
	 3.0/16.0, 11.0/16.0,  1.0/16.0,  9.0/16.0,
	15.0/16.0,  7.0/16.0, 13.0/16.0,  5.0/16.0
);

void fragment() {
	vec4 sampled = texture(albedo_texture, UV) * albedo;
	ALBEDO = sampled.rgb;

	// Screen-space pixel coords -> 4x4 cell index.
	ivec2 p = ivec2(mod(FRAGCOORD.xy, 4.0));
	float threshold = bayer4x4[p.y * 4 + p.x];

	if (fade < threshold) {
		discard;
	}
}
