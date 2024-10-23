using Vge.Games;
using Vge.Util;
using WinGL.Util;

namespace Vge.Renderer.World
{
    /// <summary>
    /// Объект рендера мира
    /// </summary>
    public class WorldRenderer : WarpRenderer
    {
        /// <summary>
        /// Буфер для склейки рендера. покуда тест в будущем для чанка
        /// </summary>
        public readonly BufferFastFloat BufferMeshFloat = new BufferFastFloat(10000);
        /// <summary>
        /// Буфер для склейки рендера байтовых данных. покуда тест в будущем для чанка
        /// </summary>
        public readonly BufferFast BufferMesh = new BufferFast(10000);

        /// <summary>
        /// Обзор в виде круга
        /// </summary>
        private Vector2i[] _overviewCircles;

        public WorldRenderer(GameBase game) : base(game)
        {
            SetOverviewChunk(_game.Player.OverviewChunk);
        }

        /// <summary>
        /// Задать обзор
        /// </summary>
        public void SetOverviewChunk(byte overviewChunk)
            => _overviewCircles = Ce.GenOverviewCircles(overviewChunk);

        /// <summary>
        /// Запускается мир, возможно смена миров
        /// </summary>
        public void Starting()
        {

        }

        /// <summary>
        /// Останавливаем мир, возможно смена миров
        /// </summary>
        public void Stoping() 
        {

        }

        /// <summary>
        /// Метод для прорисовки кадра
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public override void Draw(float timeIndex)
        {
            _game.Render.TestRun();

            _game.Render.ShaderBindVoxels(_game.Player.View, 256, 1, 1, 1, 15);

            int count = _overviewCircles.Length;
            int px = _game.Player.chPos.X;
            int py = _game.Player.chPos.Y;
            int bx = px << 4;
            int by = py << 4;
            ChunkRender chunkRender;
            int x, y; 
            for (int i = 0; i < count; i++)
            {
                chunkRender = _game.World.ChunkPrClient.GetChunk(_overviewCircles[i].X + px, _overviewCircles[i].Y + py) as ChunkRender;
                if (chunkRender != null)
                {
                    x = _overviewCircles[i].X << 4;
                    y = _overviewCircles[i].Y << 4;
                    _game.Render.ShVoxel.SetUniform3(_game.GetOpenGL(), "pos",
                        x, 0, y);
                    chunkRender.DrawDense();
                }
            }

        }

        public override void Dispose()
        {
            BufferMeshFloat.Dispose();
            BufferMesh.Dispose();
        }
    }
}
