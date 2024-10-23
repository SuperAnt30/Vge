using System;
using Vge.Util;
using Vge.World;
using Vge.World.Chunk;

namespace Vge.Renderer.World
{
    /// <summary>
    /// Объект рендера чанка, он же клиентский чанк
    /// </summary>
    public class ChunkRender : ChunkBase, IDisposable
    {
        /// <summary>
        /// Клиентский мир
        /// </summary>
        private readonly WorldClient _worldClient;
        /// <summary>
        /// Сетка чанка сплошных блоков
        /// </summary>
        private readonly MeshVoxel _meshDense;

        /// <summary>
        /// Буфер для склейки рендера
        /// </summary>
        private readonly BufferFastFloat _bufferFloat;
        /// <summary>
        /// Буфер для склейки рендера, байтовых данных
        /// </summary>
        private readonly BufferFast _buffer;

        public ChunkRender(WorldClient worldClient, int chunkPosX, int chunkPosY) 
            : base(worldClient, chunkPosX, chunkPosY)
        {
            _worldClient = worldClient;
            _bufferFloat = _worldClient.WorldRender.BufferMeshFloat;
            _buffer = _worldClient.WorldRender.BufferMesh;
            _meshDense = new MeshVoxel(_worldClient.WorldRender.GetOpenGL());

            _bufferFloat.Clear();
            _buffer.Clear();
            AddVertex(0, 0, 0, 0, 0, 0, (byte)(chunkPosX % 3 == 0 ? 255 : 0), 0, 255);
            AddVertex(16, 0, 0, .1f, 0, 255, 255, 255, 255);
            AddVertex(0, 0, 16, 0, .1f, 0, (byte)(chunkPosY % 3 == 0 ? 255 : 0), (byte)(chunkPosY % 3 == 0 ? 255 : 0), 255);
            AddVertex(16, 0, 16, .1f, .1f, 255, 255, 255, 255);

            _meshDense.Reload(_bufferFloat, _buffer);
        }

        /// <summary>
        /// Прорисовка сплошных блоков псевдо чанка
        /// </summary>
        public void DrawDense() => _meshDense.Draw();

        public void Dispose() => _meshDense.Dispose();

        /// <summary>
        /// Добавить вершину
        /// </summary>
        private void AddVertex(float x, float y, float z, float u, float v, byte r, byte g, byte b, byte light, byte height = 0)
        {
            _bufferFloat.Add(x);
            _bufferFloat.Add(y);
            _bufferFloat.Add(z);
            _bufferFloat.Add(u);
            _bufferFloat.Add(v);

            _buffer.Buffer[_buffer.Count++] = r;
            _buffer.Buffer[_buffer.Count++] = g;
            _buffer.Buffer[_buffer.Count++] = b;
            _buffer.Buffer[_buffer.Count++] = light;
            _buffer.Buffer[_buffer.Count++] = 0; // animationFrame;
            _buffer.Buffer[_buffer.Count++] = 0;// animationPause;
            _buffer.Buffer[_buffer.Count++] = height;
            _buffer.Count++;
        }
    }
}
