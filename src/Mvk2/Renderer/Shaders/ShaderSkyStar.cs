using System.Collections.Generic;
using Vge.Util;
using WinGL.OpenGL;

namespace Mvk2.Renderer.Shaders
{
    public class ShaderSkyStar : ShaderProgram
    {
        public ShaderSkyStar(GL gl)
        {
            this.gl = gl;
            string vsh = FileAssets.ReadStringToShader("SkyStar.vsh");
            string fsh = FileAssets.ReadStringToShader("SkyStar.fsh");

            Create("SkyStar", vsh, fsh,
                new Dictionary<uint, string> {
                    { 0, "v_position" },
                    { 1, "v_color" }
                });
        }
    }
}
