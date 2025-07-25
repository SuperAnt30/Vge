﻿using System;
using System.Runtime.CompilerServices;
using Vge.Util;
using WinGL.OpenGL;

namespace Vge.Renderer.Shaders
{
    /// <summary>
    /// Шейдора для сущностей
    /// </summary>
    public class ShadersEntity : IDisposable
    {
        private readonly GL gl;
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

        public ShadersEntity(GL gl, string nameLow, string nameHigh, string nameDepthMap)
        {
            this.gl = gl;
            _shaderLow = new ShaderEntity(gl, nameLow);
            _shaderHigh = new ShaderEntity(gl, nameHigh);
            _shaderDepthMap = new ShaderEntity(gl, nameDepthMap);
            _qualitatively = Options.Qualitatively;
        }

        public void Init()
        {
            // Для карты глубины
            _shaderDepthMap.Bind();
            gl.Uniform1(_shaderDepthMap.GetUniformLocation("sampler_small"),
                Gi.ActiveTextureSamplerSmall);
            gl.Uniform1(_shaderDepthMap.GetUniformLocation("sampler_big"),
                Gi.ActiveTextureSamplerBig);

            // Для максимального качества
            _shaderHigh.Bind();
            gl.Uniform1(_shaderHigh.GetUniformLocation("sampler_small"),
                Gi.ActiveTextureSamplerSmall);
            gl.Uniform1(_shaderHigh.GetUniformLocation("sampler_big"),
                Gi.ActiveTextureSamplerBig);
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
        /// Внести данные в юниформы
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UniformData(float x, float y, float z, 
            float lightBlock, float lightSky, float depth, float anim, int eyeLips)
        {
            _shderAction.SetUniform3("pos", x, y, z);
            _shderAction.SetUniform1("depth", depth);
            _shderAction.SetUniform1("anim", anim);
            _shderAction.SetUniform1("eyeLips", eyeLips);
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
