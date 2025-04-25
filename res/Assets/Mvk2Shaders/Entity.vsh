#version 330 core

layout(location = 0) in vec3 v_position;
layout(location = 1) in vec2 v_texCoord;
layout(location = 2) in float v_jointId;

//out vec4 a_color;
out vec2 a_texCoord;

uniform mat4 view;
uniform vec3 pos;
uniform mat4x3 elementTransforms[24];

void main()
{
    // Матрица модели, расположения в мире
    mat4 modelMatrix = mat4(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, pos.x, pos.y, pos.z, 1);
    
    //a_color = vec4(1.0);
	a_texCoord = v_texCoord;
    
    int id = int(v_jointId);
    gl_Position = view * modelMatrix * mat4(elementTransforms[id]) * vec4(v_position, 1.0); 
}