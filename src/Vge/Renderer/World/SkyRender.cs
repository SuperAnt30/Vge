using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Vge.Entity.Player;
using Vge.Renderer.Mesh;
using Vge.Renderer.Shaders;
using WinGL.OpenGL;
using WinGL.Util;

namespace Vge.Renderer.World
{
    /// <summary>
    /// Объект отвечает за прорисовку неба
    /// </summary>
    public class SkyRender : IDisposable
    {
        /// <summary>
        /// Объект OpenGL для элемента управления
        /// </summary>
        protected readonly GL gl;

        /// <summary>
        /// Объект рендера мира
        /// </summary>
        protected readonly WorldRenderer _worldRenderer;
        /// <summary>
        /// Объект игрока на клиенте
        /// </summary>
        protected readonly PlayerClientOwner _player;

        /// <summary>
        /// Шейдор неба
        /// </summary>
        protected readonly ShaderSky _shSky;
        /// <summary>
        /// Сетка неба
        /// </summary>
        private readonly MeshSky _mesh;
        /// <summary>
        /// Количество сегментов окружности
        /// </summary>
        protected int _segments = 16;
        /// <summary>
        /// Верхняя высота
        /// </summary>
        protected int _heightUp = 32;
        /// <summary>
        /// Средняя высота, примерно для глаз
        /// </summary>
        protected int _heightCenter = 8;
        /// <summary>
        /// Нижняя высота
        /// </summary>
        protected int _heightDown = -16;
        /// <summary>
        /// Обзор в блоках
        /// </summary>
        protected int _overviewBlock;

        private Vector3 _colorSky;
        private Vector3 _colorFog;
        protected Vector3 _colorDown;

        public SkyRender(PlayerClientOwner player, WorldRenderer worldRenderer)
        {
            _worldRenderer = worldRenderer;
            _player = player;
            _overviewBlock = _player.OverviewChunk * 16;
            gl = worldRenderer.GetOpenGL();
            _mesh = new MeshSky(gl, GL.GL_STATIC_DRAW);
            _mesh.Reload(_GenBuffer());
            _shSky = new ShaderSky(gl);
        }

        private float[] _GenBuffer()
        {
            List<float> list = new List<float>();
            float fob = _overviewBlock + 32;
            float angleStep = Glm.Pi360 / _segments;
            float x, z, x0, z0, currentAngle;
            x0 = z0 = 0;

            // Генерация вершин по окружности
            for (int i = 0; i <= _segments; i++)
            {
                currentAngle = angleStep * i;
                x = fob * Glm.Cos(currentAngle);
                z = fob * Glm.Sin(currentAngle);

                if (i > 0)
                {
                    list.AddRange(new float[] {
                        // Up
                        x, _heightUp, z, 0,
                        0, _heightUp, 0, 0,
                        x0, _heightUp, z0, 0,

                        // Up side
                        x, _heightCenter, z, 2,
                        x, _heightUp, z, 0,
                        x0, _heightUp, z0, 0,

                        x0, _heightCenter, z0, 2,
                        x, _heightCenter, z, 2,
                        x0, _heightUp, z0, 0,

                        // Down side
                        x, _heightDown * 4, z, 1,
                        x, _heightCenter, z, 2,
                        x0, _heightCenter, z0, 2,

                        x0, _heightDown * 4, z0, 1,
                        x, _heightDown * 4, z, 1,
                        x0, _heightCenter, z0, 2,

                        // Down
                        x0, _heightDown * 4, z0, 1,
                        0, _heightDown * 4, 0, 1,
                        x, _heightDown * 4, z, 1,
                    });
                }

                x0 = x;
                z0 = z;
            }

            return list.ToArray();
        }

        /// <summary>
        /// Изменён обзор чанков
        /// </summary>
        public virtual void ModifyOverviewChunk(int overviewBlock)
        {
            _overviewBlock = overviewBlock;
            _mesh.Reload(_GenBuffer());
        }

        /// <summary>
        /// Игровой такт
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void Update()
        {
            _colorSky = _worldRenderer.ColorSky;
            _colorFog = _worldRenderer.ColorFog;
            _colorDown = _colorFog;
        }

        /// <summary>
        /// Прорисовка неба
        /// </summary>
        public virtual void DrawSky(float timeIndex)
        {
            if (Debug.IsDrawVoxelLine)
            {
                gl.PolygonMode(GL.GL_FRONT_AND_BACK, GL.GL_LINE);
                gl.Disable(GL.GL_CULL_FACE);
            }
            else
            {
                _worldRenderer.Render.DepthOff();
            }

            _shSky.Bind();
            _shSky.SetUniformMatrix4("view", Gi.MatrixView);
            _shSky.SetUniform4("color", _colorSky.X, _colorSky.Y, _colorSky.Z, 1f);
            _shSky.SetUniform4("colorDown", _colorDown.X, _colorDown.Y, _colorDown.Z, 1f);
            _shSky.SetUniform4("colorfog", _colorFog.X, _colorFog.Y, _colorFog.Z, 1f);
            _mesh.Draw();

            _DrawAddElementSky(timeIndex);

            if (Debug.IsDrawVoxelLine)
            {
                gl.Enable(GL.GL_CULL_FACE);
                gl.PolygonMode(GL.GL_FRONT_AND_BACK, GL.GL_FILL);
            }
            else
            {
                _worldRenderer.Render.DepthOn();
            }
        }

        /// <summary>
        /// Прорисовка облака неба
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void DrawClouds(float timeIndex) { }

        /// <summary>
        /// Дополнительные элементы неба
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void _DrawAddElementSky(float timeIndex) { }


        public virtual void Dispose()
        {
            _mesh.Dispose();
            _shSky.Delete();
        }
    }
}
