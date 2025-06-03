#version 330 core

in vec2 a_light; 
in vec2 a_texCoord;
in vec2 a_depth;

out vec4 f_color;

uniform sampler2DArray sampler_small;
uniform sampler2DArray sampler_big;
uniform sampler2D light_map;

void main()
{
    vec3 uv = vec3(a_texCoord, a_depth.x);
	vec4 tex_color = texture(a_depth.y == 0 ? sampler_small : sampler_big, uv);
    if (tex_color.a < 0.1) discard;
    f_color = tex_color * texture(light_map, a_light);
}