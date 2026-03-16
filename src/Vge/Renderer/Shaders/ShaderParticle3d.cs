using System.Collections.Generic;
using Vge.Util;
using WinGL.OpenGL;

namespace Vge.Renderer.Shaders
{
    public class ShaderParticle3d : ShaderProgram
    {
        public ShaderParticle3d(GL gl)
        {
            this.gl = gl;
            string vsh = FileAssets.ReadStringToShader("EntityParticle3d.vsh");
            string fsh = FileAssets.ReadStringToShader("EntityParticle3d.fsh");

            Create("EntityParticle3d", vsh, fsh,
                new Dictionary<uint, string> {
                    { 0, "v_position" },
                    { 1, "v_color" }
                });
        }
    }
}
