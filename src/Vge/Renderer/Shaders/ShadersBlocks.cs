using System;
using System.Runtime.CompilerServices;
using Vge.Util;
using WinGL.OpenGL;
using WinGL.Util;

namespace Vge.Renderer.Shaders
{
    public class ShadersBlocks : IDisposable
    {
        private readonly GL gl;
        /// <summary>
        /// Шейдоры с низким качеством
        /// </summary>
        private readonly ShaderBlocks _shaderLow;
        /// <summary>
        /// Шейдоры с высоким качеством
        /// </summary>
        private readonly ShaderBlocks _shaderHigh;
        /// <summary>
        /// Шейдоры карты глубины
        /// </summary>
        private readonly ShaderBlocks _shaderDepthMap;

        /// <summary>
        /// Активный шейдор, обычный или для теней
        /// </summary>
        private ShaderBlocks _shderAction;
        /// <summary>
        /// Графика качественнее, красивее
        /// </summary>
        private bool _qualitatively;

        public ShadersBlocks(GL gl, string nameLow, string nameHigh, string nameDepthMap)
        {
            this.gl = gl;
            _shaderLow = new ShaderBlocks(gl, nameLow);
            _shaderHigh = new ShaderBlocks(gl, nameHigh);
            _shaderDepthMap = new ShaderBlocks(gl, nameDepthMap);
            _qualitatively = Options.Shadow;
        }

        public void Init()
        {
            // Для карты глубины
            _shaderDepthMap.Bind();
            // Активация текстуры атласа с размытостью (с Mipmap)
            gl.Uniform1(_shaderDepthMap.GetUniformLocation("atlas_blurry"),
                Gi.ActiveTextureAatlasBlurry);
            // Активация текстуры атласа с резкостью (без Mipmap)
            gl.Uniform1(_shaderDepthMap.GetUniformLocation("atlas_sharpness"),
                Gi.ActiveTextureAatlasSharpness);

            // Для максимального качества
            _shaderHigh.Bind();
            // Активация текстуры атласа с размытостью (с Mipmap)
            gl.Uniform1(_shaderHigh.GetUniformLocation("atlas_blurry"),
                Gi.ActiveTextureAatlasBlurry);
            // Активация текстуры атласа с резкостью (без Mipmap)
            gl.Uniform1(_shaderHigh.GetUniformLocation("atlas_sharpness"),
                Gi.ActiveTextureAatlasSharpness);
            // Активация текстуры карты света
            gl.Uniform1(_shaderHigh.GetUniformLocation("light_map"),
                Gi.ActiveTextureLightMap);
            // Активация текстуры карты теней
            gl.Uniform1(_shaderHigh.GetUniformLocation("depth_map"),
                Gi.ActiveTextureShadowMap);

            // Для простого качества
            _shaderLow.Bind();
            // Активация текстуры атласа с размытостью (с Mipmap)
            gl.Uniform1(_shaderLow.GetUniformLocation("atlas_blurry"),
                Gi.ActiveTextureAatlasBlurry);
            // Активация текстуры атласа с резкостью (без Mipmap)
            gl.Uniform1(_shaderLow.GetUniformLocation("atlas_sharpness"),
                Gi.ActiveTextureAatlasSharpness);
            // Активация текстуры карты света
            gl.Uniform1(_shaderLow.GetUniformLocation("light_map"),
                Gi.ActiveTextureLightMap);
        }

        /// <summary>
        /// Изменено качество
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UpdateOptions() => _qualitatively = Options.Shadow;

        #region Bind Uniform

        /// <summary>
        /// Чистый бинд, так-как ранее что-то могло перебиндить
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Bind() => _shderAction.Bind();

        /// <summary>
        /// Занести начальные юниформы
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void BindUniformBigin(float x, float y, float z, int takt, float wind,
            float overview, Vector3 colorFog, float torchInHand)
        {
            _shderAction = _qualitatively ? _shaderHigh : _shaderLow;
            _shderAction.Bind();
            _shderAction.SetUniformMatrix4("view", Gi.MatrixView);
            _shderAction.SetUniform1("takt", takt);
            _shderAction.SetUniform1("animOffset", Ce.ShaderAnimOffset);
            // Ветер, значение от -1 до 1
            _shderAction.SetUniform1("wind", wind);
            _shderAction.SetUniform3("player", x, y, z);
            _shderAction.SetUniform1("overview", overview);
            _shderAction.SetUniform3("colorfog", colorFog.X, colorFog.Y, colorFog.Z);
            _shderAction.SetUniform1("torch", torchInHand);
            _shderAction.SetUniform1("brightness", Gi.BlockBrightness);
            _shderAction.SetUniform3("lightDir", Gi.ViewLightDir.X, Gi.ViewLightDir.Y, Gi.ViewLightDir.Z);

            if (_qualitatively)
            {
                _shderAction.SetUniformMatrix4("lightMatrix", Gi.MatrixViewDepthMap);
            }
        }

        /// <summary>
        /// Занести начальные юниформы для карты глубины
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void BindUniformBiginDepthMap(float x, float y, float z, int takt, float wind)
        {
            _shaderDepthMap.Bind();
            _shaderDepthMap.SetUniformMatrix4("view", Gi.MatrixViewDepthMap);
            _shaderDepthMap.SetUniform1("takt", takt);
            _shaderDepthMap.SetUniform1("animOffset", Ce.ShaderAnimOffset);
            // Ветер, значение от -1 до 1
            _shaderDepthMap.SetUniform1("wind", wind);
            _shaderDepthMap.SetUniform3("player", x, y, z);

            _shderAction = _shaderDepthMap;
        }

        /// <summary>
        /// Внести данные в юниформы
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UniformDataChunk(int x, int y)
            => _shderAction.SetUniform2("chunk", x, y);

        /// <summary>
        /// Внести Ветер, значение от -1 до 1
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UniformDataWind(float wind)
            => _shderAction.SetUniform1("wind", wind);

        #endregion

        public void Dispose()
        {
            _shderAction = null;
            _shaderLow.Delete();
            _shaderHigh.Delete();
            _shaderDepthMap.Delete();
        }
    }
}
