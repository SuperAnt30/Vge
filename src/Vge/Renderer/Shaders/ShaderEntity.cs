using System.Collections.Generic;
using Vge.Util;
using WinGL.OpenGL;

namespace Vge.Renderer.Shaders
{
    public class ShaderEntity : ShaderProgram
    {
        public ShaderEntity(GL gl)
        {
            string vsh = FileAssets.ReadString(Options.PathShaders + "Entity.vsh");
            string fsh = FileAssets.ReadString(Options.PathShaders + "Entity.fsh");

            Create(gl, vsh, fsh,
                new Dictionary<uint, string> {
                    { 0, "v_position" },
                    { 1, "v_texCoord" },
                    { 2, "v_jointId" },
                    { 3, "v_clothId" }
                });
        }
    }
}
