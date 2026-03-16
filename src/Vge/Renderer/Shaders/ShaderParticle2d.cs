using System.Collections.Generic;
using Vge.Util;
using WinGL.OpenGL;

namespace Vge.Renderer.Shaders
{
    public class ShaderParticle2d : ShaderProgram
    {
        public ShaderParticle2d(GL gl)
        {
            this.gl = gl;
            string vsh = FileAssets.ReadStringToShader("EntityParticle2d.vsh");
            string fsh = FileAssets.ReadStringToShader("EntityParticle2d.fsh");

            Create("EntityParticle2d", vsh, fsh,
                new Dictionary<uint, string> {
                    { 0, "v_position" },
                    { 1, "v_texCoord" },
                    { 2, "v_color" }
                });
        }
    }
}
