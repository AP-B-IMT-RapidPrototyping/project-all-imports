shader_type spatial;
render_mode unshaded;

uniform vec4 glow_color : source_color = vec4(1.0, 0.8, 0.0, 1.0); // Goud/oranje
uniform float glow_strength : hint_range(0.0, 1.0) = 0.0;

void fragment() {
    // Basis kleur van het object
    vec3 base_color = vec3(0.8, 0.8, 0.8);
    
    // Mix met glow kleur op basis van strength
    vec3 final_color = mix(base_color, glow_color.rgb, glow_strength);
    
    // Emissie toevoegen voor het "gloeien"
    EMISSION = glow_color.rgb * glow_strength * 0.5;
    ALBEDO = final_color;
}