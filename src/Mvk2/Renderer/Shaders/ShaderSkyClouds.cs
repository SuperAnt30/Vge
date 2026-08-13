using System.Collections.Generic;
using Vge.Util;
using WinGL.OpenGL;

namespace Mvk2.Renderer.Shaders
{
    public class ShaderSkyClouds : ShaderProgram
    {
        public ShaderSkyClouds(GL gl)
        {
            this.gl = gl;
            string vsh = FileAssets.ReadStringToShader("SkyClouds.vsh");
            string fsh = FileAssets.ReadStringToShader("SkyClouds.fsh");

            Create("SkyClouds", vsh, fsh,
                new Dictionary<uint, string> {
                    { 0, "v_position" },
                    { 1, "v_texCoord" },
                    { 2, "v_alpha" },
                });
        }
    }
}
