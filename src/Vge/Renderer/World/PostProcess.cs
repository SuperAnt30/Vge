using System;
using WinGL.OpenGL;

namespace Vge.Renderer.World
{
    /// <summary>
    /// Постобработка: мир рисуется во внеэкранный буфер (цвет + глубина),
    /// затем один полноэкранный проход выводит его на экран с эффектами.
    /// </summary>
    public class PostProcess : IDisposable
    {
        /// <summary>
        /// Текстура цвета сцены (блок UnitColor в шейдере постобработки)
        /// </summary>
        private uint _textureColor;
        /// <summary>
        /// Текстура глубины сцены (блок UnitDepth в шейдере постобработки)
        /// </summary>
        private uint _textureDepth;
        private int _width;
        private int _height;

        private readonly GL gl;
        private uint _fbo;
        /// <summary>
        /// Пустой VAO: в core-профиле без привязанного VAO DrawArrays не работает,
        /// а вершины полноэкранного треугольника строятся в шейдере из gl_VertexID
        /// </summary>
        private readonly uint _vaoEmpty;

        public PostProcess(GL gl)
        {
            this.gl = gl;
            uint[] id = new uint[1];
            gl.GenVertexArrays(1, id);
            _vaoEmpty = id[0];
        }

        /// <summary>
        /// Создать или пересоздать буфер под размер окна. Вызывать при старте и при изменении размера
        /// </summary>
        public void Resize(int width, int height)
        {
            if (width == _width && height == _height && _fbo != 0) return;
            _DeleteBuffers();
            _width = width;
            _height = height;

            uint[] id = new uint[1];

            // Цвет. RGBA8 хватает для виньетки, подводного эффекта, FXAA.
            // Для bloom/HDR понадобится GL_RGBA16F и тип GL_FLOAT
            gl.GenTextures(1, id);
            _textureColor = id[0];
            gl.BindTexture(GL.GL_TEXTURE_2D, _textureColor);
            gl.TexImage2D(GL.GL_TEXTURE_2D, 0, GL.GL_RGBA8, width, height, 0,
                GL.GL_RGBA, GL.GL_UNSIGNED_BYTE, IntPtr.Zero);
            _SetupTexture(GL.GL_LINEAR);

            // Глубина + трафарет. Текстура, а не renderbuffer — чтобы читать её в шейдере
            gl.GenTextures(1, id);
            _textureDepth = id[0];
            gl.BindTexture(GL.GL_TEXTURE_2D, _textureDepth);
            gl.TexImage2D(GL.GL_TEXTURE_2D, 0, GL.GL_DEPTH24_STENCIL8, width, height, 0,
                GL.GL_DEPTH_STENCIL, GL.GL_UNSIGNED_INT_24_8, IntPtr.Zero);
            _SetupTexture(GL.GL_NEAREST);

            gl.BindTexture(GL.GL_TEXTURE_2D, 0);

            gl.GenFramebuffersEXT(1, id);
            _fbo = id[0];
            gl.BindFramebuffer(GL.GL_FRAMEBUFFER, _fbo);
            gl.FramebufferTexture2D(GL.GL_FRAMEBUFFER, GL.GL_COLOR_ATTACHMENT0,
                GL.GL_TEXTURE_2D, _textureColor, 0);
            gl.FramebufferTexture2D(GL.GL_FRAMEBUFFER, GL.GL_DEPTH_STENCIL_ATTACHMENT,
                GL.GL_TEXTURE_2D, _textureDepth, 0);

            uint status = gl.CheckFramebufferStatus(GL.GL_FRAMEBUFFER);
            gl.BindFramebuffer(GL.GL_FRAMEBUFFER, 0);
            if (status != GL.GL_FRAMEBUFFER_COMPLETE)
            {
                throw new Exception("PostProcess: framebuffer не готов, статус 0x" + status.ToString("X"));
            }
        }

        /// <summary>
        /// Начать сцену: дальше всё рисуется во внеэкранный буфер
        /// </summary>
        public void BeginScene()
        {
            gl.BindFramebuffer(GL.GL_FRAMEBUFFER, _fbo);
            gl.Viewport(0, 0, _width, _height);
        }

        /// <summary>
        /// Закончить сцену: вернуться к экрану и привязать текстуры сцены.
        /// Шейдер постобработки и его uniform-ы привязывает вызывающий, между EndScene и DrawScreen.
        /// В шейдере: u_color = UnitColor, u_depth = UnitDepth
        /// </summary>
        public void EndScene()
        {
            gl.BindFramebuffer(GL.GL_FRAMEBUFFER, 0);
            gl.Viewport(0, 0, _width, _height);

            gl.ActiveTexture(GL.GL_TEXTURE0 + (uint)Gi.ActiveTexturePostDepth);
            gl.BindTexture(GL.GL_TEXTURE_2D, _textureDepth);
            gl.ActiveTexture(GL.GL_TEXTURE0 + (uint)Gi.ActiveTexturePostColor);
            gl.BindTexture(GL.GL_TEXTURE_2D, _textureColor);
            // Активным оставляем блок 0, как ожидает остальной рендер
            gl.ActiveTexture(GL.GL_TEXTURE0);
        }

        /// <summary>
        /// Нарисовать полноэкранный треугольник (шейдер уже привязан).
        /// Состояние GL после вызова такое же, как до него
        /// </summary>
        public void DrawScreen()
        {
            // Запоминаем, что было включено
            bool depthTest = gl.IsEnabled(GL.GL_DEPTH_TEST);
            bool blend = gl.IsEnabled(GL.GL_BLEND);
            bool cullFace = gl.IsEnabled(GL.GL_CULL_FACE);

            if (depthTest) gl.Disable(GL.GL_DEPTH_TEST);
            if (blend) gl.Disable(GL.GL_BLEND);
            if (cullFace) gl.Disable(GL.GL_CULL_FACE);

            gl.BindVertexArray(_vaoEmpty);
            gl.DrawArrays(GL.GL_TRIANGLES, 0, 3);
            gl.BindVertexArray(0);

            // Возвращаем как было
            if (depthTest) gl.Enable(GL.GL_DEPTH_TEST);
            if (blend) gl.Enable(GL.GL_BLEND);
            if (cullFace) gl.Enable(GL.GL_CULL_FACE);

            gl.Clear(GL.GL_DEPTH_BUFFER_BIT);
        }

        private void _SetupTexture(uint filter)
        {
            gl.TexParameter(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MIN_FILTER, filter);
            gl.TexParameter(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MAG_FILTER, filter);
            gl.TexParameter(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_WRAP_S, GL.GL_CLAMP_TO_EDGE);
            gl.TexParameter(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_WRAP_T, GL.GL_CLAMP_TO_EDGE);
        }

        private void _DeleteBuffers()
        {
            if (_fbo != 0) gl.DeleteFramebuffersEXT(1, new uint[] { _fbo });
            if (_textureColor != 0) gl.DeleteTextures(1, new uint[] { _textureColor });
            if (_textureDepth != 0) gl.DeleteTextures(1, new uint[] { _textureDepth });
            _fbo = 0;
            _textureColor = 0;
            _textureDepth = 0;
        }

        public void Dispose()
        {
            _DeleteBuffers();
            gl.DeleteVertexArrays(1, new uint[] { _vaoEmpty });
        }
    }
}
