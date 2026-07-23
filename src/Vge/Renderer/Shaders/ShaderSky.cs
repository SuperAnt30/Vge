using System.Collections.Generic;
using Vge.Util;
using WinGL.OpenGL;

namespace Vge.Renderer.Shaders
{
    public class ShaderSky : ShaderProgram
    {
        public ShaderSky(GL gl)
        {
            this.gl = gl;
            string vsh = FileAssets.ReadStringToShader("Sky.vsh");
            string fsh = FileAssets.ReadStringToShader("Sky.fsh");

            Create("Sky", vsh, fsh,
                new Dictionary<uint, string> {
                    { 0, "v_position" },
                    { 1, "v_fog" }
                });
        }
    }
}
