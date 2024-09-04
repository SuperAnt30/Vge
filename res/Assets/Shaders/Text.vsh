#version 330 core

layout (location = 0) in vec2 v_position;
layout (location = 1) in vec2 v_texCoord;
layout (location = 2) in vec3 v_color3;

out vec4 a_color;
out vec2 a_texCoord;

uniform mat4 projview;
uniform float si;

void main()
{
	a_color = vec4(v_color3, 1.0);
	a_texCoord = v_texCoord;
	gl_Position = projview * vec4(v_position.x * si, v_position.y * si, 0, 1.0);
}