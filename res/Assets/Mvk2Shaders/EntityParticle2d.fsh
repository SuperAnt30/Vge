#version 330 core

in vec2 a_texCoord;
flat in vec4 a_color;
flat in vec2 a_light;
flat in int a_param;

out vec4 f_color;

uniform sampler2D light_map;
uniform sampler2D u_texture0;

void main()
{
    vec4 light_color;
    // param: бит 2 — без освещения (полная яркость)
    if ((a_param & 2) != 0) {
        light_color = vec4(1.0);
    }
    else {
        light_color = texture(light_map, a_light);
    }
    // param: бит 1 — есть текстура
    if ((a_param & 1) == 1) {
        // Имеется ли текстура
        vec4 tex_color = texture(u_texture0, a_texCoord);
        if (tex_color.a < 0.05) discard;
        light_color = light_color * tex_color;
    }
    f_color = a_color * light_color;
}
