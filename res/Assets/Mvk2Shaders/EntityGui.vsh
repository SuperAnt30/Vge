#version 330 core

layout(location = 0) in vec3 v_position;
layout(location = 1) in vec3 v_normal;
layout(location = 2) in vec2 v_texCoord;
layout(location = 3) in int v_jointId;
layout(location = 4) in int v_clothId;

out vec2 a_texCoord;
out vec3 a_normal;
out vec3 a_lightDir;
out float a_brightness;
out float a_depth;

uniform mat4 view;
uniform vec3 lightDir;
uniform float brightness;

uniform vec2 pos;
uniform float scale;
uniform float depth;

void main()
{
    a_texCoord = v_texCoord;
    a_brightness = brightness;
    a_lightDir = lightDir;
    a_depth = depth;
    
    mat4 mS = mat4(scale, 0, 0, 0, 0, scale, 0, 0, 0, 0, scale, 0, pos.x, pos.y, 0, 1);
    gl_Position = view * mS * vec4(v_position, 1.0);
    a_normal = v_normal; 
}