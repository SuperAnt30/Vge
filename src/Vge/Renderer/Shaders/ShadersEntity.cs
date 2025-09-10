using System;
using System.Runtime.CompilerServices;
using Vge.Util;
using WinGL.OpenGL;
using WinGL.Util;

namespace Vge.Renderer.Shaders
{
    /// <summary>
    /// Шейдора для сущностей
    /// </summary>
    public class ShadersEntity : IDisposable
    {
        private readonly GL gl;
        /// <summary>
        /// Шейдоры для Gui
        /// </summary>
        private readonly ShaderGuiEntity _shaderGui;
        /// <summary>
        /// Шейдоры с низким качеством
        /// </summary>
        private readonly ShaderEntity _shaderLow;
        /// <summary>
        /// Шейдоры с высоким качеством
        /// </summary>
        private readonly ShaderEntity _shaderHigh;
        /// <summary>
        /// Шейдоры карты глубины
        /// </summary>
        private readonly ShaderEntity _shaderDepthMap;

        /// <summary>
        /// Активный шейдор, обычный или для теней
        /// </summary>
        private ShaderEntity _shderAction;
        /// <summary>
        /// Активна ли карта глубины
        /// </summary>
        private bool _flagActionDepthMap;
        /// <summary>
        /// Графика качественнее, красивее
        /// </summary>
        private bool _qualitatively;

        public ShadersEntity(GL gl, string nameGui, string nameLow, string nameHigh, string nameDepthMap)
        {
            this.gl = gl;
            _shaderGui = new ShaderGuiEntity(gl, nameGui);
            _shaderLow = new ShaderEntity(gl, nameLow);
            _shaderHigh = new ShaderEntity(gl, nameHigh);
            _shaderDepthMap = new ShaderEntity(gl, nameDepthMap);
            _qualitatively = Options.Qualitatively;
        }

        public void Init()
        {
            // Для Gui
            _shaderGui.Bind();
            gl.Uniform1(_shaderGui.GetUniformLocation("sampler_small"),
                Gi.ActiveTextureSamplerSmall);
            gl.Uniform1(_shaderGui.GetUniformLocation("sampler_big"),
                Gi.ActiveTextureSamplerBig);
            gl.Uniform1(_shaderGui.GetUniformLocation("atlas"),
                Gi.ActiveTextureAatlasSharpness);

            // Для карты глубины
            _shaderDepthMap.Bind();
            gl.Uniform1(_shaderDepthMap.GetUniformLocation("sampler_small"),
                Gi.ActiveTextureSamplerSmall);
            gl.Uniform1(_shaderDepthMap.GetUniformLocation("sampler_big"),
                Gi.ActiveTextureSamplerBig);
            gl.Uniform1(_shaderDepthMap.GetUniformLocation("atlas"),
                Gi.ActiveTextureAatlasSharpness);

            // Для максимального качества
            _shaderHigh.Bind();
            gl.Uniform1(_shaderHigh.GetUniformLocation("sampler_small"),
                Gi.ActiveTextureSamplerSmall);
            gl.Uniform1(_shaderHigh.GetUniformLocation("sampler_big"),
                Gi.ActiveTextureSamplerBig);
            gl.Uniform1(_shaderHigh.GetUniformLocation("atlas"),
                Gi.ActiveTextureAatlasSharpness);
            // Активация текстуры карты света
            gl.Uniform1(_shaderHigh.GetUniformLocation("light_map"),
                Gi.ActiveTextureLightMap);
            // Активация текстуры карты глубины
            gl.Uniform1(_shaderHigh.GetUniformLocation("depth_map"),
                Gi.ActiveTextureShadowMap);

            // Для простого качества
            _shaderLow.Bind();
            gl.Uniform1(_shaderLow.GetUniformLocation("sampler_small"),
                Gi.ActiveTextureSamplerSmall);
            gl.Uniform1(_shaderLow.GetUniformLocation("sampler_big"),
                Gi.ActiveTextureSamplerBig);
            gl.Uniform1(_shaderLow.GetUniformLocation("atlas"),
                Gi.ActiveTextureAatlasSharpness);
            // Активация текстуры карты света
            gl.Uniform1(_shaderLow.GetUniformLocation("light_map"),
                Gi.ActiveTextureLightMap);
        }

        /// <summary>
        /// Изменено качество
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UpdateOptions() => _qualitatively = Options.Qualitatively;

        #region Bind Uniform

        /// <summary>
        /// Занести начальные юниформы
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void BindUniformBigin()
        {
            _flagActionDepthMap = false;
            _shderAction = _qualitatively ? _shaderHigh : _shaderLow;
            _shderAction.Bind();
            _shderAction.SetUniformMatrix4("view", Gi.MatrixView);
            _shderAction.SetUniform1("brightness", Gi.EntityBrightness);
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
        public void BindUniformBiginDepthMap()
        {
            _flagActionDepthMap = true;
            _shaderDepthMap.Bind();
            _shaderDepthMap.SetUniformMatrix4("view", Gi.MatrixViewDepthMap);
            _shderAction = _shaderDepthMap;
        }

        /// <summary>
        /// Обнулить для предметов
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void BindUniformItems()
        {
            _shderAction.SetUniform1("depth", 0f);
            _shderAction.SetUniform1("anim", 0f);
            _shderAction.SetUniform1("eyeMouth", 0);
        }

        /// <summary>
        /// Внести данные в юниформы
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UniformData(float x, float y, float z, 
            float lightBlock, float lightSky, float depth, float anim, int eyeMouth)
        {
            _shderAction.SetUniform3("pos", x, y, z);
            _shderAction.SetUniform1("depth", depth);
            _shderAction.SetUniform1("anim", anim);
            _shderAction.SetUniform1("eyeMouth", eyeMouth);
            if (!_flagActionDepthMap)
            {
                // Свет для карты теней не нужен
                _shderAction.SetUniform2("light", lightBlock, lightSky);
            }
        }

        /// <summary>
        /// Внести данные в юниформы для претмета
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UniformData(float x, float y, float z, float lightBlock, float lightSky)
        {
            _shderAction.SetUniform3("pos", x, y, z);
            if (!_flagActionDepthMap)
            {
                // Свет для карты теней не нужен
                _shderAction.SetUniform2("light", lightBlock, lightSky);
            }
        }

        /// <summary>
        /// Внести данные в юниформы
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UniformData(float[] elementTransforms)
            => _shderAction.SetUniformMatrix4x3("elementTransforms", elementTransforms, Ce.MaxAnimatedBones);

        /// <summary>
        /// Занести начальные юниформы для GUI
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void BindUniformBiginGui()
        {
            _shaderGui.Bind();
            _shaderGui.SetUniformMatrix4("view", Gi.Ortho);
            _shaderGui.SetUniform1("brightness", Gi.EntityBrightness);
            //_shaderGui.SetUniform3("lightDir", 0, 0, -1);
            _shaderGui.SetUniform3("lightDir", .9f, -.9f, 0);
            _shaderGui.SetUniform1("scale", (float)Gi.Si);
            _shaderGui.SetUniform1("depth", -1f); // Нужен когда хотим сущность продемонстрировать
        }

        /// <summary>
        /// Внести данные в юниформы для претмета Gui
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UniformDataGui(float x, float y, bool volume = false)
        {
            _shaderGui.SetUniform2("pos", x, y);
            if (volume)
            {
                _shaderGui.SetUniform3("lightDir", -0.707106769f, -0.707106769f, 0); // -1, -1, 0 нормализован
            }
            else
            {
                _shaderGui.SetUniform3("lightDir", 0, 0, -1);
            }
        }

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
