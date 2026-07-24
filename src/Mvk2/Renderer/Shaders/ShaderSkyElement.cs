using System.Collections.Generic;
using Vge.Util;
using WinGL.OpenGL;

namespace Mvk2.Renderer.Shaders
{
    public class ShaderSkyElement : ShaderProgram
    {
        public ShaderSkyElement(GL gl)
        {
            this.gl = gl;
            string vsh = FileAssets.ReadStringToShader("SkyElement.vsh");
            string fsh = FileAssets.ReadStringToShader("SkyElement.fsh");

            Create("SkyElement", vsh, fsh,
                new Dictionary<uint, string> {
                    { 0, "v_position" },
                    { 1, "v_texCoord" }
                });
        }
    }
}
