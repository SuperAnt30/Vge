using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Vge.Entity.Player;
using Vge.Renderer.Mesh;
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
        /// Сетка неба
        /// </summary>
        private readonly MeshSky _mesh;

        public SkyRender(PlayerClientOwner player, WorldRenderer worldRenderer)
        {
            _worldRenderer = worldRenderer;
            _player = player;
            gl = worldRenderer.GetOpenGL();
            _mesh = new MeshSky(gl);
            _mesh.Reload(_GenBuffer(_player.OverviewChunk * 16));
        }

        private float[] _GenBuffer(int overviewBlock)
        {
            float fob = overviewBlock + 32;
            float fy = 32;// 24;
            float fyd = 8;

            List<float> list = new List<float>();

            int segments = 32;

            float angleStep = Glm.Pi360 / segments;

            float x, z, x0, z0;
            x0 = z0 = 0;

            // Генерация вершин по окружности
            for (int i = 0; i <= segments; i++)
            {
                float currentAngle = angleStep * i;
                x = fob * Glm.Cos(currentAngle);
                z = fob * Glm.Sin(currentAngle);

                if (i > 0)
                {
                    list.AddRange(new float[] {
                        x, fy, z, 0,
                        0, fy, 0, 0,
                        x0, fy, z0, 0,

                        x, fyd, z, 1,
                        x, fy, z, 0,
                        x0, fy, z0, 0,

                        x0, fyd, z0, 1,
                        x, fyd, z, 1,
                        x0, fy, z0, 0
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
        public void ModifyOverviewChunk(int overviewBlock)
        {
            _mesh.Reload(_GenBuffer(overviewBlock));
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

            _worldRenderer.Render.ShaderBindSky(Gi.MatrixView, _worldRenderer.ColorSky,
                _worldRenderer.ColorFog);
            _mesh.Draw();

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

        public virtual void Dispose()
        {
            _mesh.Dispose();
        }
    }
}
