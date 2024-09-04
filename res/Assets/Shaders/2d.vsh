#version 330 core

layout (location = 0) in vec2 v_position;
layout (location = 1) in vec2 v_texCoord;

out vec4 a_color;
out vec2 a_texCoord;

uniform mat4 projview;
uniform float biasX;
uniform float biasY;
uniform vec4 color;

void main()
{
	a_color = color;
	a_texCoord = v_texCoord;
	gl_Position = projview * vec4(v_position.x + biasX, v_position.y + biasY, 0, 1.0);
}