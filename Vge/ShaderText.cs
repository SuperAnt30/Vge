using System.Collections.Generic;
using WinGL.OpenGL;

namespace Vge
{
    public class ShaderText : ShaderProgram
    {
        public ShaderText(GL gl)
        {
            string vertexShaderSource = @"#version 330 core

layout (location = 0) in vec2 v_position;
layout (location = 1) in vec2 v_texCoord;
layout (location = 2) in vec3 v_color3;

out vec4 a_color;
out vec2 a_texCoord;

uniform mat4 projview;

void main(){
    //float v_color = 0;// v_color3.x;
    float r = v_color3.x;
    float g = v_color3.y;
    float b = v_color3.z;
    //float r = fract(v_color);
    //float g = fract((v_color - r) / 100.0);
    //float b = (v_color - r - g * 100.0) / 10000.0;
	a_color = vec4(r, g, b, 1.0);
	a_texCoord = v_texCoord;
	gl_Position = projview * vec4(v_position.xy, 0, 1.0);
}";
            string fragmentShaderSource = @"#version 330 core

in vec4 a_color;
in vec2 a_texCoord;
out vec4 f_color;

uniform sampler2D u_texture0;

void main(){
	f_color = a_color * texture(u_texture0, a_texCoord);
}";

            Create(gl, vertexShaderSource, fragmentShaderSource,
                new Dictionary<uint, string> {
                    { 0, "v_position" },
                    { 1, "v_texCoord" },
                    { 2, "v_color3" }
                });
        }
            
    }
}
