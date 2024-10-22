using System;
using Vge.Games;
using Vge.Util;

namespace Vge.Renderer.World
{
    /// <summary>
    /// Объект рендера мира
    /// </summary>
    public class WorldRenderer : WarpRenderer
    {
        private MeshVoxel _meshTest;
        /// <summary>
        /// Буфер для склейки рендера. покуда тест в будущем для чанка
        /// </summary>
        private readonly BufferFast _buffer = new BufferFast(1000);

        public WorldRenderer(GameBase game) : base(game)
        {
            _meshTest = new MeshVoxel(game.GetOpenGL());
        }


        bool b2 = true;
        byte b = 0;
        /// <summary>
        /// Метод для прорисовки кадра
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public override void Draw(float timeIndex)
        {
            if (b2)
            {
                b++;
                if (b == 255) b2 = false;
            }
            else
            {
                b--;
                if (b == 0) b2 = true;
            }
            _buffer.Clear();

            AddVertex(-100, 0, -100, 0, 0, b, 255, 255, 255);
            AddVertex(100, 0, -100, .1f, 0, 255, b, 255, 255);
            AddVertex(-100, 0, 100, 0, .1f, 255, 255, b, 255);
            AddVertex(100, 0, 100, .1f, .1f, b, b, b, 255);
        
            _meshTest.Reload(_buffer.ToBuffer(), _buffer.Count);

            _game.Render.TestRun();

            _game.Render.ShaderBindVoxels(_game.Player.View, 256, 1, 1, 1, 15);
            _meshTest.Draw();
            
        }

        /// <summary>
        /// Добавить вершину
        /// </summary>
        private void AddVertex(float x, float y, float z, float u, float v, byte r, byte g, byte b, byte light, byte height = 0)
        {
            _buffer.AddFloat(BitConverter.GetBytes(x));
            _buffer.AddFloat(BitConverter.GetBytes(y));
            _buffer.AddFloat(BitConverter.GetBytes(z));
            _buffer.AddFloat(BitConverter.GetBytes(u));
            _buffer.AddFloat(BitConverter.GetBytes(v));
            _buffer.Buffer[_buffer.Count++] = r;
            _buffer.Buffer[_buffer.Count++] = g;
            _buffer.Buffer[_buffer.Count++] = b;
            _buffer.Buffer[_buffer.Count++] = light;
            _buffer.Buffer[_buffer.Count++] = 0; // animationFrame;
            _buffer.Buffer[_buffer.Count++] = 0;// animationPause;
            _buffer.Buffer[_buffer.Count++] = height;
            _buffer.Count++;
        }

        public override void Dispose()
        {
            _meshTest.Dispose();
            _buffer.Dispose();
        }
    }
}
