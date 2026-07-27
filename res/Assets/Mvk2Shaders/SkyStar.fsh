#version 330 core

in vec4 a_color;
in float a_transparency;

out vec4 f_color;

void main()
{
    float transparency = a_color.a * a_transparency;
    if (transparency < 0.05) discard;
    f_color = vec4(a_color.xyz, transparency);
}