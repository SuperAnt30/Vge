using System.Collections.Generic;
using Vge.Util;
using WinGL.OpenGL;

namespace Vge
{
    public class Shader2d : ShaderProgram
    {
        public Shader2d(GL gl)
        {
            string vsh = FileAssets.ReadString(Options.ToPathShaders() + "2d.vsh");
            string fsh = FileAssets.ReadString(Options.ToPathShaders() + "2d.fsh");

            Create(gl, vsh, fsh,
                new Dictionary<uint, string> {
                    { 0, "v_position" },
                    { 1, "v_texCoord" }
                });
        }
    }
}
