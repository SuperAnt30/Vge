using System.Collections.Generic;
using Vge.Util;
using WinGL.OpenGL;

namespace Vge.Renderer.Shaders
{
    public class ShaderEntity : ShaderProgram
    {
        /// <summary>
        /// Шейдор сущности для карты теней
        /// </summary>
        public readonly bool IsShadowMap;

        public ShaderEntity(GL gl, bool isShadowMap, string name)
        {
            this.gl = gl;
            IsShadowMap = isShadowMap;
            string vsh = FileAssets.ReadString(Options.PathShaders + name + ".vsh");
            string fsh = FileAssets.ReadString(Options.PathShaders + name + ".fsh");

            Create(vsh, fsh,
                new Dictionary<uint, string> {
                    { 0, "v_position" },
                    { 1, "v_texCoord" },
                    { 2, "v_jointId" },
                    { 3, "v_clothId" }
                });
        }
    }
}
