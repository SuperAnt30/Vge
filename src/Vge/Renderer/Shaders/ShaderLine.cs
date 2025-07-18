﻿using System.Collections.Generic;
using Vge.Util;
using WinGL.OpenGL;

namespace Vge.Renderer.Shaders
{
    public class ShaderLine : ShaderProgram
    {
        public ShaderLine(GL gl)
        {
            this.gl = gl;
            string vsh = FileAssets.ReadStringToShader("Line.vsh");
            string fsh = FileAssets.ReadStringToShader("Line.fsh");

            Create("Line", vsh, fsh,
                new Dictionary<uint, string> {
                    { 0, "v_position" },
                    { 1, "v_color" }
                });
        }
    }
}
