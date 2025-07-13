#version 330 core

layout(location = 0) in vec3 v_position;
layout(location = 1) in vec2 v_texCoord;
layout(location = 2) in int v_jointId;
layout(location = 3) in int v_clothId;

out vec2 a_texCoord;
out vec2 a_light;
out float a_depth;
out float a_eye;

uniform mat4 view;

uniform vec3 pos;
uniform vec2 light;
uniform float depth;
uniform float anim;
uniform int eyeOpen;
uniform mat4x3 elementTransforms[24];

void main()
{
    int jointId = v_jointId & 0xFF;
    a_eye = float((v_jointId >> 8) & 0xFF);
    if (a_eye != 0)
    {
        if (eyeOpen == a_eye) a_eye = 0;
    }
	a_texCoord = v_texCoord;
    a_light = light;
    if (v_clothId == -1)
    {
        a_depth = depth;
    }
    else 
    {
        a_depth = float(v_clothId);
    }
    
	if (anim < 1)
	{
	  gl_Position = view * vec4(pos + v_position, 1.0); 
    }	
	else
	{
	  // Матрица модели, расположения в мире
      mat4 modelMatrix = mat4(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, pos.x, pos.y, pos.z, 1);
      gl_Position = view * modelMatrix * mat4(elementTransforms[jointId]) * vec4(v_position, 1.0);
	}
}