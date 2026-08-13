#version 330 core

in vec2 a_texCoord;
in float a_transparency;
in float a_few;
in vec3 a_color;

out vec4 f_color;

uniform sampler2D u_texture0;

void main()
{
    if (a_transparency < 0.05) discard;
    vec4 tex_color = texture(u_texture0, a_texCoord);
    if (tex_color.x < a_few) discard;
    
    f_color = vec4(a_color, a_transparency);
}