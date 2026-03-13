using System.Collections.Generic;
using Vge.Util;
using WinGL.OpenGL;

namespace Vge.Renderer.Shaders
{
    public class ShaderParticle : ShaderProgram
    {
        public ShaderParticle(GL gl)
        {
            this.gl = gl;
            string vsh = FileAssets.ReadStringToShader("EntityParticle.vsh");
            string fsh = FileAssets.ReadStringToShader("EntityParticle.fsh");

            Create("EntityParticle", vsh, fsh,
                new Dictionary<uint, string> {
                    { 0, "v_position" },
                    { 1, "v_color" }
                });
        }
    }
}
