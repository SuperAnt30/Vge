#version 330 core

layout (location = 0) in vec3 v_position;
layout (location = 1) in float v_fog;

out vec4 a_color;

uniform mat4 view;
uniform vec3 color;
uniform vec3 colorfog;

void main()
{
    if (v_fog == 0) {
        a_color = vec4(color, 1);
    }
    else {
        a_color = vec4(colorfog, 1);
    }
	gl_Position = view * vec4(v_position, 1.0);
}