#version 330 core

layout(location = 0) in vec3 v_position;
layout(location = 1) in vec3 v_normal;
layout(location = 2) in vec2 v_texCoord;
layout(location = 3) in int v_jointId;
layout(location = 4) in int v_clothId;

out vec2 a_texCoord;
out vec2 a_light;
out float a_depth;
out float a_eyeLips;

out vec4 a_fragToLight;
out vec3 a_normal;
out vec3 a_lightDir;
out float a_brightness;

uniform mat4 view;
uniform mat4 lightMatrix;
uniform vec3 lightDir;
uniform float brightness;

uniform vec3 pos;
uniform vec2 light;
uniform float depth;
uniform float anim;
uniform int eyeLips;
uniform mat4x3 elementTransforms[24];

void main()
{
    a_brightness = brightness;
    a_lightDir = lightDir;
    
    int jointId = v_jointId & 0xFF;
    a_eyeLips = float((v_jointId >> 8) & 0xFF);
    if (a_eyeLips != 0)
    {
        if (a_eyeLips > 2)
        {
            int lips = eyeLips >> 1;
            if (lips == a_eyeLips - 3) a_eyeLips = 0;
        }
        else
        {
            int eye = eyeLips & 1;
            if (eye == a_eyeLips - 1) a_eyeLips = 0;
        }
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
      a_fragToLight = lightMatrix * vec4(pos + v_position, 1.0); 
      a_normal = v_normal; 
    }	
	else
	{
	  // Матрица модели, расположения в мире
      mat4 modelMatrix = mat4(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, pos.x, pos.y, pos.z, 1);
      a_fragToLight = lightMatrix * modelMatrix * mat4(elementTransforms[jointId]) * vec4(v_position, 1.0);
      a_normal = vec3(mat4(elementTransforms[jointId]) * vec4(v_normal, 1.0));
      gl_Position = view * modelMatrix * mat4(elementTransforms[jointId]) * vec4(v_position, 1.0);
	}
}