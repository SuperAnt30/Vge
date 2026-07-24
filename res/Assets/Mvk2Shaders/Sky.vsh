#version 330 core

layout (location = 0) in vec3 v_position;
layout (location = 1) in float v_fog;

out vec4 a_color;

uniform mat4 view;
uniform vec4 color;
uniform vec4 colorfog;

void main()
{
    if (v_fog == 0) {
        a_color = color;
    }
    else {
        a_color = colorfog;
    }
	gl_Position = view * vec4(v_position, 1.0);
}