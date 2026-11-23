#version 330 core

layout(location = 0) in vec3 v_position;
layout(location = 1) in vec3 v_normal;
layout(location = 2) in vec2 v_texCoord;
layout(location = 3) in int v_jointId;
layout(location = 4) in int v_clothId;

out vec2 a_texCoord;
out vec2 a_light;

out vec4 a_fragToLight; //
out vec3 a_normal;
out vec3 a_lightDir;
out float a_brightness;

uniform mat4 view;
uniform mat4 lightMatrix; //
uniform vec3 lightDir;
uniform float brightness;

uniform vec3 pos;
uniform float scale;
uniform vec2 light;

void main()
{
    
    a_lightDir = lightDir;
    
	a_texCoord = v_texCoord;
    a_light = light;
    // Определяем яркость от блока
    float lightBlock = 1.0375 - light.x * 1.2;
    if (lightBlock < 0) lightBlock = 0;
    a_brightness = brightness * lightBlock;
    
    gl_Position = view * vec4(pos + v_position, 1.0);
    a_fragToLight = lightMatrix * vec4(pos + v_position, 1.0); //
    a_normal = v_normal; 
}