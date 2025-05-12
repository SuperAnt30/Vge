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

        private EntityRender _entityRender;

        public EntitiesRenderer(GameBase game, ArrayFast<ChunkRender> arrayChunkRender) : base(game)
        {
            gl = GetOpenGL();
            _hitbox = new HitboxEntityRender(gl);
            //_entityRender = new EntityRender(gl);
            _arrayChunkRender = arrayChunkRender;
        }

        /// <summary>
        /// Метод для прорисовки кадра
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public override void Draw(float timeIndex)
        {
            int count = _arrayChunkRender.Count;
            int countEntity;
            ChunkRender chunkRender;
            EntityBase entity;
            int playerId = _game.Player.Id;
            float x = _game.Player.PosFrameX;
            float y = _game.Player.PosFrameY;
            float z = _game.Player.PosFrameZ;

            if (_entityRender == null)
            {
                // TODO::2025-04-25 Продумать, подготовку рендера для сети и не только до создания сущностей
                // Надо сделать в Ce flag на готовность сущностей
                if (Ce.ModelEntities == null) return;
                _entityRender = new EntityRender(gl, Render, _game.Player);
            }

            CountEntitiesFC = 0;
            for (int i = 0; i < count; i++)
            {
                chunkRender = _arrayChunkRender[i];
                countEntity = chunkRender.ListEntities.Count;
                if (countEntity > 0)
                {
                    for (int j = 0; j < countEntity; j++)
                    {
                        entity = chunkRender.ListEntities.GetAt(j);
                        //if (entity.Id != playerId)// && _game.Player.IsBoxInFrustum(entity.GetBoundingBoxOffset(-x, -y, -z)))
                        {
                            // HitBox
                            Render.ShaderBindLine(_game.Player.View,
                                entity.GetPosFrameX(timeIndex) - x,
                                entity.GetPosFrameY(timeIndex) - y,
                                entity.GetPosFrameZ(timeIndex) - z);
                            _hitbox.Draw(timeIndex, entity);
                            // Model
                            entity.Render.Draw(timeIndex, _game.DeltaTimeFrame);
                            //_entityRender.Draw(timeIndex, entity);
                            CountEntitiesFC++;
                        }
                    }
                }
            }
        }

        public EntityRender GetEntityRender() => _entityRender;

        /// <summary>
        /// Метод для прорисовки основного игрока
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public void DrawOwner(float timeIndex)
        {
            //_game.WorldRender.Render.ShaderBindLine(_game.Player.View, 0, 0, 0);
            //_hitbox.Draw(timeIndex, _game.Player);
        }

        public override void Dispose()
        {
            _hitbox.Dispose();
        }

    }
}
