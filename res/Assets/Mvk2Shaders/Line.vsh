#version 330 core

layout (location = 0) in vec3 v_position;
layout (location = 1) in vec4 v_color;

out vec4 a_color;

uniform mat4 view;
uniform vec3 pos;

void main()
{
	a_color = v_color;
	gl_Position = view * vec4(pos + v_position, 1.0);
}