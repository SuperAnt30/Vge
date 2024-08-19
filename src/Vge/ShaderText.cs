using System.Collections.Generic;
using Vge.Util;
using WinGL.OpenGL;

namespace Vge
{
    public class ShaderText : ShaderProgram
    {
        public ShaderText(GL gl)
        {
            string vsh = FileAssets.ReadString(Options.PathShaders + "Text.vsh");
            string fsh = FileAssets.ReadString(Options.PathShaders + "Text.fsh");

            Create(gl, vsh, fsh,
                new Dictionary<uint, string> {
                    { 0, "v_position" },
                    { 1, "v_texCoord" },
                    { 2, "v_color3" }
                });
        }
    }
}
