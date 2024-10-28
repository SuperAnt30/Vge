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

        public WorldRenderer(GameBase game) : base(game)
        {

        }

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

            int count = _game.Player.FrustumCulling.Count;
            int px = _game.Player.Position.ChunkPositionX;
            int pz = _game.Player.Position.ChunkPositionZ;
            int bx = px << 4;
            int bz = pz << 4;

            float fx = _game.Player.Position.X - bx;
            float fz = _game.Player.Position.Z - bz;
            ChunkRender chunkRender;
            Vector2i vec;
            int x, y; 
            for (int i = 0; i < count; i++)
            {
                vec = _game.Player.FrustumCulling[i];
                chunkRender = _game.World.ChunkPrClient.GetChunk(vec.X + px, vec.Y + pz) as ChunkRender;
                if (chunkRender != null)
                {
                    x = vec.X << 4;
                    y = vec.Y << 4;
                    _game.Render.ShVoxel.SetUniform3(_game.GetOpenGL(), "pos",
                    //x, -_game.Player.Position.Y, y);
                    x - fx, -_game.Player.Position.Y, y - fz);

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
