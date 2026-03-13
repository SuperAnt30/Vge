#version 330 core

layout (location = 0) in vec3 v_position;
layout (location = 1) in vec4 v_color;

out vec4 a_color;
out vec2 a_light;

uniform mat4 view;
uniform mat4 rotateMatrix;
uniform vec3 pos;
uniform vec2 light;
uniform int param;
uniform vec3 color;

void main()
{
    a_light = light;
    //a_color = v_color;
    //mat4 modelMatrix = mat4(scale, 0, 0, 0, 0, scale, 0, 0, 0, 0, scale, 0, pos.x, pos.y, pos.z, 1);
    
    if (param == 1) {
        a_color = vec4(color, v_color.w);
        mat4 modelMatrix = mat4(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, pos.x, pos.y, pos.z, 1);
        gl_Position = view * modelMatrix * rotateMatrix * vec4(v_position, 1.0);
        //gl_Position = view * rotateMatrix * vec4(pos + v_position, 1.0);
    }
    else {
        a_color = v_color * vec4(color, v_color.w);
        gl_Position = view * vec4(pos + v_position, 1.0);
        //gl_Position = view * modelMatrix * vec4(v_position, 1.0);
    }
    
    //gl_Position = view * vec4(pos + v_position, 1.0);
}