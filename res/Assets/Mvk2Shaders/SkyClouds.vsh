#version 330 core

layout(location = 0) in vec3 v_position;
layout(location = 1) in vec2 v_texCoord;
layout(location = 2) in float v_alpha;

out vec2 a_texCoord;
out float a_transparency;
out float a_few;
out vec3 a_color;

uniform float transparency;
uniform float few;
uniform mat4 view;
uniform float posY;
uniform vec2 pos;
uniform vec3 color;

void main()
{
    a_texCoord = vec2(v_texCoord.x + pos.x, v_texCoord.y + pos.y);
    a_transparency = transparency * v_alpha;
    a_few = few;
    a_color = color;
    
    gl_Position = view * vec4(v_position.x, v_position.y - posY, v_position.z, 1.0);
}