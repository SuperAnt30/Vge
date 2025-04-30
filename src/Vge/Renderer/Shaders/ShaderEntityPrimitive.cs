using System.Collections.Generic;
using Vge.Util;
using WinGL.OpenGL;

namespace Vge.Renderer.Shaders
{
    public class ShaderEntityPrimitive : ShaderProgram
    {
        public ShaderEntityPrimitive(GL gl)
        {
            string vsh = FileAssets.ReadString(Options.PathShaders + "EntityPrimitive.vsh");
            string fsh = FileAssets.ReadString(Options.PathShaders + "EntityPrimitive.fsh");

            Create(gl, vsh, fsh,
                new Dictionary<uint, string> {
                    { 0, "v_position" },
                    { 1, "v_texCoord" }
                });
        }
    }
}
