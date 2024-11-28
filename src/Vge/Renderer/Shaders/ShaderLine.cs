using System.Collections.Generic;
using Vge.Util;
using WinGL.OpenGL;

namespace Vge.Renderer.Shaders
{
    public class ShaderLine : ShaderProgram
    {
        public ShaderLine(GL gl)
        {
            string vsh = FileAssets.ReadString(Options.PathShaders + "Line.vsh");
            string fsh = FileAssets.ReadString(Options.PathShaders + "Line.fsh");

            Create(gl, vsh, fsh,
                new Dictionary<uint, string> {
                    { 0, "v_position" },
                    { 1, "v_color" }
                });
        }
    }
}
