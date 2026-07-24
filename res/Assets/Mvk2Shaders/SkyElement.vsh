#version 330 core

layout(location = 0) in vec3 v_position;
layout(location = 1) in vec2 v_texCoord;

out vec2 a_texCoord;

uniform mat4 view;
uniform mat4 model;

void main()
{
    a_texCoord = v_texCoord;
   // mat4 modelMatrix = mat4(scale, 0, 0, 0, 0, scale, 0, 0, 0, 0, scale, 0, pos.x, pos.y, pos.z, 1);
   // gl_Position = view * modelMatrix * rotateMatrix * vec4(v_position, 1.0);
   gl_Position = view * model * vec4(v_position, 1.0);
}