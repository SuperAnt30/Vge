using System.Collections.Generic;
using WinGL.OpenGL;

namespace Vge
{
    public class Shader2d : ShaderProgram
    {
        public Shader2d(GL gl)
        {
            string vertexShaderSource = @"#version 330 core

layout (location = 0) in vec2 v_position;
layout (location = 1) in vec2 v_texCoord;

out vec4 a_color;
out vec2 a_texCoord;

uniform mat4 projview;
uniform float biasX;
uniform float biasY;
uniform vec4 color;

void main(){
	a_color = color;
	a_texCoord = v_texCoord;
	gl_Position = projview * vec4(v_position.x + biasX, v_position.y + biasY, 0, 1.0);
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
                    { 1, "v_texCoord" }
                });
        }
            
    }
}
