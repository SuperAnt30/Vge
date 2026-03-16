#version 330 core

in vec2 a_texCoord;
in vec4 a_color;

out vec4 f_color;

uniform int param;
uniform vec2 light;
uniform sampler2D light_map;
uniform sampler2D u_texture0;

void main()
{
    vec4 light_color;
    if (param >> 1 == 1) {
        light_color = vec4(1);
    }
    else {
        light_color = texture(light_map, light);
    }
    if ((param & 1) == 1) {
        // Имеется ли текстура
        vec4 tex_color = texture(u_texture0, a_texCoord);
        if (tex_color.a < 0.05) discard;
        light_color = light_color * tex_color;
    }
    f_color = a_color * light_color;
}