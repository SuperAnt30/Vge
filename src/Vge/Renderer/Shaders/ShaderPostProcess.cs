using System.Collections.Generic;
using Vge.Util;
using WinGL.OpenGL;

namespace Vge.Renderer.Shaders
{
    public class ShaderPostProcess : ShaderProgram
    {
        public ShaderPostProcess(GL gl)
        {
            this.gl = gl;
            string vsh = FileAssets.ReadStringToShader("PostProcess.vsh");
            string fsh = FileAssets.ReadStringToShader("PostProcess.fsh");

            Create("PostProcess", vsh, fsh,
                new Dictionary<uint, string> { });
        }
    }
}
