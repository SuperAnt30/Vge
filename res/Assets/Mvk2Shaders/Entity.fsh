#version 330 core
 
//in vec4 a_color;
in vec2 a_light;
in vec2 a_texCoord;

out vec4 f_color;

uniform sampler2D atlas;
uniform sampler2D light_map;

void main()
{
    vec4 tex_color = texture(atlas, a_texCoord);
    if (tex_color.a < 0.1) discard;
    vec4 light_color = texture(light_map, a_light);
    f_color = tex_color * light_color;
}