#version 330 core

layout(location = 0) in vec3 v_position;
layout(location = 1) in vec2 v_texCoord;

out vec2 a_texCoord;
out vec2 a_light;
out vec2 a_depth;

uniform mat4 view;

uniform vec3 pos;
uniform vec2 light;
uniform vec2 depth;

void main()
{
	a_texCoord = v_texCoord;
    a_light = light;
    a_depth = depth;
    gl_Position = view * vec4(pos + v_position, 1.0); 
}