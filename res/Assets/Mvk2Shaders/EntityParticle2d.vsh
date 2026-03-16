#version 330 core

layout(location = 0) in vec2 v_position;
layout(location = 1) in vec2 v_texCoord;

out vec2 a_texCoord;
out vec4 a_color;

uniform mat4 view;
uniform mat4 rotateMatrix;
uniform vec3 pos;
uniform vec4 color;
uniform float scale;

void main()
{
    a_texCoord = v_texCoord;
    a_color = color;
    mat4 modelMatrix = mat4(scale, 0, 0, 0, 0, scale, 0, 0, 0, 0, scale, 0, pos.x, pos.y, pos.z, 1);
    gl_Position = view * modelMatrix * rotateMatrix * vec4(v_position.x, v_position.y, 0, 1.0);
}