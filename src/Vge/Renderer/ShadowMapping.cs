using System;
using System.Runtime.CompilerServices;
using Vge.Renderer.Shaders;
using Vge.Util;
using WinGL.OpenGL;

namespace Vge.Renderer
{
    /// <summary>
    /// Объект отвечает за всенаправленных карт теней
    /// </summary>
    public class ShadowMapping : IDisposable
    {
        /// <summary>
        /// Сколько чанков в обзоре, для карты теней. 
        /// Квадрат 5*5 минус углы = 21
        /// Квадрат 7*7 минус углы = 45
        /// Квадрат 9*9 минус углы = 77
        /// </summary>
        public const int CountChunkShadowMap = 45;
        /// <summary>
        /// Размер ортогональной матрицы
        /// </summary>
        public const int SizeOrthShadowMap = 64;
        /// <summary>
        /// Размер стороны текстуры карты теней (квадрат) 1024*1024
        /// 2048 4096 8192
        /// </summary>
        public const int SizeTextureShadowMap = 4096;

        /// <summary>
        /// Включена ли тень
        /// </summary>
        public bool IsShadow { get; private set; }
        /// <summary>
        /// Объект OpenGL для элемента управления
        /// </summary>
        private readonly GL gl;
        /// <summary>
        /// Шейдоры для отладки карты теней
        /// </summary>
        private readonly ShaderDepthMap _shShadowMap;
        /// <summary>
        /// Буфер карты теней
        /// </summary>
        private uint _shadowMapFbo;
        /// <summary>
        /// Текстура карты теней
        /// </summary>
        private uint _shadowMapTexture;
        /// <summary>
        /// Сетка для отладки карты теней
        /// </summary>
        private MeshQuad _meshShadowMap;

        public ShadowMapping(GL gl)
        {
            this.gl = gl;
            _shShadowMap = new ShaderDepthMap(gl);
            ReloadInit();
        }

        /// <summary>
        /// Инициализация при создании игры
        /// </summary>
        public void Init()
        {
            // Активация текстура и буфер карты теней
            uint[] id = new uint[1];
            gl.GenFramebuffers(1, id);
            _shadowMapFbo = id[0];
            gl.GenTextures(1, id);
            _shadowMapTexture = id[0];
            gl.ActiveTexture(GL.GL_TEXTURE0 + (uint)Gi.ActiveTextureShadowMap);
            gl.BindTexture(GL.GL_TEXTURE_2D, _shadowMapTexture);
            gl.TexImage2D(GL.GL_TEXTURE_2D, 0, GL.GL_DEPTH_COMPONENT, SizeTextureShadowMap, SizeTextureShadowMap, 0, GL.GL_DEPTH_COMPONENT, GL.GL_FLOAT, IntPtr.Zero);
            gl.TexParameter(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_WRAP_S, GL.GL_CLAMP_TO_BORDER);
            gl.TexParameter(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_WRAP_T, GL.GL_CLAMP_TO_BORDER);
            gl.TexParameterv(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_BORDER_COLOR, new float[] { 1.0f, 1.0f, 1.0f, 1.0f });
            gl.TexParameter(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MIN_FILTER, GL.GL_NEAREST);
            gl.TexParameter(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MAG_FILTER, GL.GL_NEAREST);
            gl.BindTexture(GL.GL_TEXTURE_2D, 0);
            gl.BindFramebuffer(GL.GL_FRAMEBUFFER, _shadowMapFbo);
            gl.FramebufferTexture2D(GL.GL_FRAMEBUFFER, GL.GL_DEPTH_ATTACHMENT, GL.GL_TEXTURE_2D, _shadowMapTexture, 0);
            gl.DrawBuffer(GL.GL_NONE);
            gl.ReadBuffer(GL.GL_NONE);
            gl.BindFramebuffer(GL.GL_FRAMEBUFFER, 0);

            if (gl.CheckFramebufferStatus(GL.GL_FRAMEBUFFER) != GL.GL_FRAMEBUFFER_COMPLETE)
            {
                // Ошибка кадрового буфера для теней
                throw new Exception(Sr.ShadowFramebufferError);
            }
        }

        /// <summary>
        /// Перезапуск качества графики
        /// </summary>
        public void ReloadInit()
        {
            IsShadow = Options.Qualitatively;
            Gi.BlockBrightness = IsShadow ? 0.3f : 0.15f;
            Gi.EntityBrightness = 0.3f;
        }

        /// <summary>
        /// Инициализация для отладки карты теней
        /// </summary>
        public void InitDebug()
        {
            // Для отладки карта теней
            _shShadowMap.Bind();
            gl.BindTexture(GL.GL_TEXTURE_2D, _shadowMapTexture);
            // Активация текстуры карты теней
            gl.Uniform1(_shShadowMap.GetUniformLocation("shadow_map"),
                Gi.ActiveTextureShadowMap);
            _meshShadowMap = new MeshQuad(gl, Gi.Width - 512, 0, Gi.Width, 512);
        }

        /// <summary>
        /// Вначале перед сценой для теней, возвращает имеется ли тень
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool RenderSceneBegin()
        {
            if (IsShadow)
            {
                gl.Viewport(0, 0, SizeTextureShadowMap, SizeTextureShadowMap);
                gl.BindFramebuffer(GL.GL_FRAMEBUFFER, _shadowMapFbo);
                gl.Clear(GL.GL_DEPTH_BUFFER_BIT);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Прорисовать квадрат тени для отладки
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawQuadDebug(float[] ortho)
        {
            _shShadowMap.Bind();
            _shShadowMap.SetUniformMatrix4("projview", ortho);
            _meshShadowMap.Draw();
        }

        public void Dispose()
        {
            if (_shadowMapFbo != 0)
            {
                gl.DeleteFramebuffers(1, new uint[] { _shadowMapFbo });
                _shadowMapFbo = 0;
            }
            if (_shadowMapTexture != 0)
            {
                gl.DeleteTextures(1, new uint[] { _shadowMapTexture });
                _shadowMapTexture = 0;
            }
            if (_meshShadowMap != null)
            {
                _meshShadowMap.Dispose();
            }
            _shShadowMap.Delete();
        }
    }
}
