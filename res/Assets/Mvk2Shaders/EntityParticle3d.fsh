#version 330 core

in vec4 a_color;
flat in vec2 a_light;
flat in int a_param;

out vec4 f_color;

uniform sampler2D light_map;

void main()
{
    // param: бит 2 — без освещения (полная яркость)
    if ((a_param & 2) != 0) {
        f_color = a_color;
    }
    else {
        f_color = a_color * texture(light_map, a_light);
    }
}
