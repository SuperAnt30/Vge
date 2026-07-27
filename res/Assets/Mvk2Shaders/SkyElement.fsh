#version 330 core

in vec2 a_texCoord;
in float a_transparency;

out vec4 f_color;

uniform sampler2D u_texture0;

void main()
{
    vec4 tex_color = texture(u_texture0, a_texCoord);
    tex_color.a = tex_color.a * a_transparency;
    if (tex_color.a < 0.05) discard;
    f_color = tex_color;
}