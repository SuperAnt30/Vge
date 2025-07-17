using System.Collections.Generic;
using WinGL.OpenGL;

namespace Vge.Renderer.Shaders
{
    public class ShaderDepthMap : ShaderProgram
    {
        public ShaderDepthMap(GL gl)
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
void main()
{
    float depth = texture(shadow_map, a_texCoord).r;
    f_color = vec4(vec3(depth), 1.0);
}
";

            Create("DepthMap", vsh, fsh,
                new Dictionary<uint, string> {
                    { 0, "v_position" },
                    { 1, "v_texCoord" }
                });
        }
    }
}
