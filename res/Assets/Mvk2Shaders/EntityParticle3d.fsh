#version 330 core

in vec4 a_color;

out vec4 f_color;

uniform int param;
uniform vec2 light;
uniform sampler2D light_map;

void main()
{
    if (param >> 1 == 1) {
        f_color = a_color;
    }
    else {
        f_color = a_color * texture(light_map, light);
    }
}