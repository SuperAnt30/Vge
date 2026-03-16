#version 330 core

layout(location = 0) in vec3 v_position;
layout(location = 1) in float v_color;

out vec4 a_color;

uniform mat4 view;
uniform vec3 pos;
uniform vec4 color;
uniform float scale;

void main()
{
    a_color = color * vec4(v_color, v_color, v_color, color.w);
    mat4 modelMatrix = mat4(scale, 0, 0, 0, 0, scale, 0, 0, 0, 0, scale, 0, pos.x, pos.y, pos.z, 1);
    gl_Position = view * modelMatrix * vec4(v_position, 1.0);
}