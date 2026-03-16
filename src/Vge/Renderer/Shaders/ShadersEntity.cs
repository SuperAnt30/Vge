using System;
using System.Runtime.CompilerServices;
using Vge.Entity.Particle;
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
        /// Шейдоры для спрайтов частичек
        /// </summary>
        private readonly ShaderParticle2d _shaderParticle2d;
        /// <summary>
        /// Шейдоры для объёмных частичек
        /// </summary>
        private readonly ShaderParticle3d _shaderParticle3d;

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
            _shaderParticle2d = new ShaderParticle2d(gl);
            _shaderParticle3d = new ShaderParticle3d(gl);
            _qualitatively = Options.Shadow;
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

            // Для частичек 2d
            _shaderParticle2d.Bind();
            // Активация текстуры карты света
            gl.Uniform1(_shaderParticle2d.GetUniformLocation("light_map"),
                Gi.ActiveTextureLightMap);

            // Для частичек 3d
            _shaderParticle3d.Bind();
            // Активация текстуры карты света
            gl.Uniform1(_shaderParticle3d.GetUniformLocation("light_map"),
                Gi.ActiveTextureLightMap);

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
        public void UpdateOptions() => _qualitatively = Options.Shadow;

        #region Bind Uniform

        /// <summary>
        /// Занести начальные юниформы
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void BindUniformBegin()
        {
            _flagActionDepthMap = false;
            _shderAction = _qualitatively ? _shaderHigh : _shaderLow;
            _shderAction.Bind();
            _shderAction.SetUniformMatrix4("view", Gi.MatrixView);
            _shderAction.SetUniform1("scale", 1f);
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
        public void BindUniformBeginDepthMap()
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
        public void BindUniformBeginGui()
        {
            _shaderGui.Bind();
            _shaderGui.SetUniformMatrix4("view", Gi.Ortho);
            _shaderGui.SetUniform1("brightness", Gi.EntityBrightness);
            _shaderGui.SetUniform1("depth", -1f); // Нужен когда хотим сущность продемонстрировать
            UniformVolumeGui(true);
        }

        /// <summary>
        /// Внести позицию для претмета Gui
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UniformPosGui(float x, float y)
            => _shaderGui.SetUniform2("pos", x, y);

        /// <summary>
        /// Внести значение объёмный ли предмет или нет
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UniformVolumeGui(bool volume = false)
        {
            if (volume)
            {
                _shaderGui.SetUniform3("lightDir", -.700140059f, -.140028015f, -.700140059f); // -5, -1, -5 нормализован
                _shaderGui.SetUniform1("scale", -(float)Gi.Si);
            }
            else
            {
                _shaderGui.SetUniform3("lightDir", 0, 0, -1);
                _shaderGui.SetUniform1("scale", (float)Gi.Si);
            }
        }

        /// <summary>
        /// Занести юниформы для GUI анимированной сущности
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void BindUniformAnimationGui(float x, float y, float scale, float depth, float anim, int eyeMouth)
        {
            _shderAction.Bind();
            _shderAction.SetUniformMatrix4("view", Gi.Ortho);
            _shderAction.SetUniform1("brightness", Gi.EntityBrightness);
            _shderAction.SetUniform3("lightDir", -.700140059f, -.140028015f, -.700140059f); // -5, -1, -5 нормализован
            _shderAction.SetUniform1("scale", -scale);
            _shderAction.SetUniform3("pos", x, y, 0);
            _shderAction.SetUniform1("depth", depth);
            _shderAction.SetUniform1("anim", anim);
            _shderAction.SetUniform1("eyeMouth", eyeMouth);
            _shderAction.SetUniform2("light", .96875f, .96875f);
        }

        /// <summary>
        /// Занести начальные юниформы для частичек
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void BindUniformParticle2d(float yaw, float pitch)
        {
            _shaderParticle2d.Bind();
            Mat4 mat = Mat4.Identity();
            mat.RotateY(-yaw);
            mat.RotateX(pitch);
            _shaderParticle2d.SetUniformMatrix4("view", Gi.MatrixView);
            _shaderParticle2d.SetUniformMatrix4("rotateMatrix", mat.ToArray());
        }

        /// <summary>
        /// Занести начальные юниформы для частичек
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void BindUniformParticle3d()
        {
            _shaderParticle3d.Bind();
            _shaderParticle3d.SetUniformMatrix4("view", Gi.MatrixView);
        }

        /// <summary>
        /// Получить объект шейдеров к частичкам
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ShaderProgram GetShaderParticle(EntityParticle entity)
        {
            if (entity.IsCube) return _shaderParticle3d;
            return _shaderParticle2d;
        }

        #endregion

        public void Dispose()
        {
            _shderAction = null;
            _shaderLow.Delete();
            _shaderHigh.Delete();
            _shaderDepthMap.Delete();
            _shaderParticle2d.Delete();
            _shaderParticle3d.Delete();
        }
    }
}
