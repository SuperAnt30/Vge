using System.Collections.Generic;
using WinGL.OpenGL;

namespace Vge.Renderer.Shaders
{
    public class ShaderShadowMap : ShaderProgram
    {
        public ShaderShadowMap(GL gl)
        {
            this.gl = gl;

            string vsh = @"#version 330 core
layout (location = 0) in vec2 v_position;
layout (location = 1) in vec2 v_texCoord;
out vec2 a_texCoord;
uniform mat4 projview;
void main()
{
	a_texCoord = v_texCoord;
	gl_Position = projview * vec4(v_position.xy, 0, 1.0);
}";
            string fsh = @"#version 330 core
in vec2 a_texCoord;
out vec4 f_color;
uniform sampler2D shadow_map;
float zNear = 0.1; 
float zFar  = 100.0; 
float LinearizeDepth(float depth) 
{
    float z = depth * 2.0 - 1.0; 
    return (2.0 * zNear * zFar) / (zFar + zNear - z * (zFar - zNear));	
}
void main()
{
    float depth = LinearizeDepth(texture(shadow_map, a_texCoord).r) / zFar;
    f_color = vec4(vec3(depth), 1.0);
}
";

            Create(vsh, fsh,
                new Dictionary<uint, string> {
                    { 0, "v_position" },
                    { 1, "v_texCoord" }
                });
        }
    }
}
