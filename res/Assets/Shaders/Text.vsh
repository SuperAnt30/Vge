#version 330 core

layout (location = 0) in vec2 v_position;
layout (location = 1) in vec2 v_texCoord;
layout (location = 2) in vec3 v_color;

out vec4 a_color;
out vec2 a_texCoord;

uniform mat4 projview;

void main()
{
	a_color = vec4(v_color, 1.0);
	a_texCoord = v_texCoord;
	gl_Position = projview * vec4(v_position.xy, 0, 1.0);
}