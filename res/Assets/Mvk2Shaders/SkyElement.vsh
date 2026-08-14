#version 330 core

layout(location = 0) in vec3 v_position;
layout(location = 1) in vec2 v_texCoord;

out vec2 a_texCoord;
out float a_transparency;

uniform float transparency;
uniform mat4 view;
uniform mat4 model;

void main()
{
    a_texCoord = v_texCoord;
    a_transparency = transparency;
    gl_Position = view * model * vec4(v_position, 1.0);
}