using System.Collections.Generic;
using Vge.Util;
using WinGL.OpenGL;

namespace Vge.Renderer.Shaders
{
    public class ShaderGuiColor : ShaderProgram
    {
        public ShaderGuiColor(GL gl, string name)
        {
            this.gl = gl;
            string vsh = FileAssets.ReadStringToShader(name + ".vsh");
            string fsh = FileAssets.ReadStringToShader(name + ".fsh");

            Create(name, vsh, fsh,
                new Dictionary<uint, string> {
                    { 0, "v_position" },
                    { 1, "v_texCoord" },
                    { 2, "v_color" }
                });
        }
    }
}
