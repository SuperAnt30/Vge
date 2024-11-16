#version 330 core

layout (location = 0) in vec2 v_position;
layout (location = 1) in vec4 v_color;

out vec4 a_color;

uniform mat4 projview;

void main()
{
	a_color = v_color;
	gl_Position = projview * vec4(v_position.xy, 0, 1.0);
}