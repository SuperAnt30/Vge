#version 330 core
 
in vec4 a_color;
in vec2 a_texCoord;
in float fog_factor;
in vec3 fog_color;
in vec2 a_light;

out vec4 f_color;

uniform sampler2D atlas;
uniform sampler2D light_map;

void main()
{
    vec4 tex_color = texture(atlas, a_texCoord);
	if (tex_color.a < 0.1) discard;
//    vec4 light_color = texture(light_map, a_light);
//    vec4 color = a_color * tex_color * light_color;
	vec4 color = a_color * tex_color;
    vec3 col3 = vec3(color);
    col3 = mix(col3, fog_color, fog_factor);
    f_color = vec4(col3, color.a);
}