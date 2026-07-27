#version 330 core

layout (location = 0) in vec3 v_position;
layout (location = 1) in float v_color;

out vec4 a_color;
out float a_transparency;

uniform float transparency;
uniform mat4 view;
uniform mat4 model;
uniform vec3 color;

void main()
{
    a_transparency = transparency;
    if (v_color < 0.16) {
        a_color = vec4(1.0, 1.0, 1.0, color.x);
    }
    else if (v_color < 0.33) {
        a_color = vec4(1.0, 1.0, 1.0, color.y);
    }
    else if (v_color < 0.5) {
        a_color = vec4(1.0, 1.0, 1.0, color.z);
    }
    else {
        a_color = vec4(1.0, 1.0, 1.0, v_color);
    }
	gl_Position = view * model* vec4(v_position, 1.0);
}