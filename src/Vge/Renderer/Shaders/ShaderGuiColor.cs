﻿using System.Collections.Generic;
using Vge.Util;
using WinGL.OpenGL;

namespace Vge.Renderer.Shaders
{
    public class ShaderGuiColor : ShaderProgram
    {
        public ShaderGuiColor(GL gl)
        {
            string vsh = FileAssets.ReadString(Options.PathShaders + "GuiColor.vsh");
            string fsh = FileAssets.ReadString(Options.PathShaders + "GuiColor.fsh");

            Create(gl, vsh, fsh,
                new Dictionary<uint, string> {
                    { 0, "v_position" },
                    { 1, "v_texCoord" },
                    { 2, "v_color" }
                });
        }
    }
}