using Vge.Entity;
using Vge.Games;
using Vge.Util;
using WinGL.OpenGL;

namespace Vge.Renderer.World.Entity
{
    /// <summary>
    /// Объект рендера всех сущностей
    /// </summary>
    public class EntitiesRenderer : WarpRenderer
    {
        /// <summary>
        /// Количество сущностей которые попали в FrustumCulling чанка, т.е. видимых для прорисовки
        /// </summary>
        public int CountEntitiesFC { get; private set; }
        /// <summary>
        /// Объект OpenGL для элемента управления
        /// </summary>
        private readonly GL gl;
        /// <summary>
        /// Метод чанков для прорисовки
        /// </summary>
        private readonly ArrayFast<ChunkRender> _arrayChunkRender;

        private HitboxEntityRender _hitbox;

        public EntitiesRenderer(GameBase game, ArrayFast<ChunkRender> arrayChunkRender) : base(game)
        {
            gl = GetOpenGL();
            _hitbox = new HitboxEntityRender(gl);
            _arrayChunkRender = arrayChunkRender;
        }

        /// <summary>
        /// Метод для прорисовки кадра
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public override void Draw(float timeIndex)
        {
            int count = _arrayChunkRender.Count;
            int countSector, countEntity;
            ChunkRender chunkRender;
            EntityBase entity;
            int playerId = _game.Player.Id;
            float x = _game.Player.PosFrameX;
            float y = _game.Player.PosFrameY;
            float z = _game.Player.PosFrameZ;

            CountEntitiesFC = 0;
            for (int i = 0; i < count; i++)
            {
                chunkRender = _arrayChunkRender[i];
                if (chunkRender.CountEntities > 0)
                {
                    countSector = chunkRender.ListEntities.Length;
                    for (int j = 0; j < countSector; j++)
                    {
                        countEntity = chunkRender.ListEntities[j].Count;
                        for (int k = 0; k < countEntity; k++)
                        {
                            entity = chunkRender.ListEntities[j].GetAt(k);
                            if (entity.Id != playerId && _game.Player.IsBoxInFrustum(entity.GetBoundingBoxOffset(-x, -y, -z)))
                            {
                                _game.WorldRender.Render.ShaderBindLine(_game.Player.View,
                                    entity.GetPosFrameX(timeIndex) - x,
                                    entity.GetPosFrameY(timeIndex) - y,
                                    entity.GetPosFrameZ(timeIndex) - z);
                                _hitbox.Draw(timeIndex, entity);
                                CountEntitiesFC++;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Метод для прорисовки основного игрока
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public void DrawOwner(float timeIndex)
        {
            _game.WorldRender.Render.ShaderBindLine(_game.Player.View, 0, 0, 0);
            _hitbox.Draw(timeIndex, _game.Player);
        }

        public override void Dispose()
        {
            _hitbox.Dispose();
        }

        //private void _RenderEntity(EntityBase entity, float timeIndex)
        //{
        //    _game.WorldRender.Render.ShaderBindLine(_game.Player.View,
        //        _game.Player.PosFrameX,
        //        _game.Player.PosFrameY,
        //        _game.Player.PosFrameZ);
        //   // _owner.Draw();
        //}

    }
}
