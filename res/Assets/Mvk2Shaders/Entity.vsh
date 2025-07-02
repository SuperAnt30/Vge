#version 330 core

layout(location = 0) in vec3 v_position;
layout(location = 1) in vec2 v_texCoord;
layout(location = 2) in int v_jointId;
layout(location = 3) in int v_clothId;

out vec2 a_texCoord;
out vec2 a_light;
out vec2 a_depth;

uniform mat4 view;

uniform vec3 pos;
uniform vec2 light;
uniform vec2 depth;
uniform mat4x3 elementTransforms[24];

void main()
{
	a_texCoord = v_texCoord;
    a_light = light;
	a_depth = depth;
    
	if (depth.y < 2)
	{
	  gl_Position = view * vec4(pos + v_position, 1.0); 
    }	
	else
	{
	  a_depth.y -= 2;
	  // Матрица модели, расположения в мире
      mat4 modelMatrix = mat4(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, pos.x, pos.y, pos.z, 1);
      gl_Position = view * modelMatrix * mat4(elementTransforms[v_jointId]) * vec4(v_position, 1.0);
	}
}