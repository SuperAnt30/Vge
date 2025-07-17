using System.Collections.Generic;
using Vge.Util;
using WinGL.OpenGL;

namespace Vge.Renderer.Shaders
{
    public class ShaderEntity : ShaderProgram
    {
        public ShaderEntity(GL gl, string name)
        {
            this.gl = gl;
            string vsh = FileAssets.ReadStringToShader(Options.PathShaders + name + ".vsh");
            string fsh = FileAssets.ReadStringToShader(Options.PathShaders + name + ".fsh");

            Create(name, vsh, fsh,
                new Dictionary<uint, string> {
                    { 0, "v_position" },
                    { 1, "v_texCoord" },
                    { 2, "v_jointId" },
                    { 3, "v_clothId" }
                });
        }
    }
}
