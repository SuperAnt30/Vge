using System.Collections.Generic;
using Vge.Util;
using WinGL.OpenGL;

namespace Vge.Renderer.Shaders
{
    public class ShaderVoxel : ShaderProgram
    {
        public ShaderVoxel(GL gl)
        {
            string vsh = FileAssets.ReadString(Options.PathShaders + "Voxel.vsh");
            string fsh = FileAssets.ReadString(Options.PathShaders + "Voxel.fsh");

            Create(gl, vsh, fsh,
                new Dictionary<uint, string> {
                    { 0, "v_position" },
                    { 1, "v_texCoord" },
                    { 2, "v_rgbl" },
                    { 3, "v_anim" }
                });
        }
    }
}
