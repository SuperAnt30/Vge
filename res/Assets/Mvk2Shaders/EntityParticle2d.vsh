#version 330 core

layout(location = 0) in vec2 v_position;
layout(location = 1) in vec2 v_texCoord;

out vec2 a_texCoord;
out vec4 a_color;

uniform int param;
uniform mat4 view;
uniform mat4 rotateMatrix;
uniform vec3 pos;
uniform vec4 color;
uniform vec4 uv;
uniform float scale;

void main()
{
    if ((param & 1) == 1) {
        if (v_texCoord.x == 0) a_texCoord.x = uv.x;
        else a_texCoord.x = uv.z;
        if (v_texCoord.y == 0) a_texCoord.y = uv.y;
        else a_texCoord.y = uv.w;
    }
    else a_texCoord = v_texCoord;
    a_color = color;
    mat4 modelMatrix = mat4(scale, 0, 0, 0, 0, scale, 0, 0, 0, 0, scale, 0, pos.x, pos.y, pos.z, 1);
    gl_Position = view * modelMatrix * rotateMatrix * vec4(v_position.x, v_position.y, 0, 1.0);
}