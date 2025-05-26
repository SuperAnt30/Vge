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
        /// <summary>
        /// Видем ли мы хитбокс сущности
        /// </summary>
        private bool _isHitBox = true;

        /// <summary>
        /// Это временно!!!
        /// </summary>
        private EntityRender[] _entityRender;

        /// <summary>
        /// Id маленьких текстур
        /// </summary>
        private uint _idTextureSmall;
        /// <summary>
        /// Id больших текстур
        /// </summary>
        private uint _idTextureBig;

        public EntitiesRenderer(GameBase game, ArrayFast<ChunkRender> arrayChunkRender) : base(game)
        {
            gl = GetOpenGL();
            _hitbox = new HitboxEntityRender(gl);
            //_entityRender = new EntityRender(gl, Render, game.Player);
            _arrayChunkRender = arrayChunkRender;
        }

        /// <summary>
        /// Игра запущена, всё загружено из библиотек
        /// </summary>
        public void GameStarting()
        {
            _idTextureSmall = Render.Texture.CreateTexture2dArray(
                ModelEntitiesReg.TextureManager.WidthSmall,
                ModelEntitiesReg.TextureManager.HeightSmall,
                ModelEntitiesReg.TextureManager.DepthSmall, 3);

            _idTextureBig = Render.Texture.CreateTexture2dArray(
                ModelEntitiesReg.TextureManager.WidthBig,
                ModelEntitiesReg.TextureManager.HeightBig,
                ModelEntitiesReg.TextureManager.DepthBig, 4);

            // TODO::2025-04-25 Продумать, подготовку рендера для сети и не только до создания сущностей
            // TODO::2025-05-14 Из-за Texture ещё не готов модуль



            int count = Ce.ModelEntities.ModelEntitiesObjects.Length;

            _entityRender = new EntityRender[count];

            ModelEntity modelEntity;
            for (int i = 0; i < count; i++)
            {
                modelEntity = Ce.ModelEntities.ModelEntitiesObjects[i];
                for (int t = 0; t < modelEntity.Textures.Length; t++)
                {
                    Render.Texture.SetImageTexture2dArray(modelEntity.Textures[t], modelEntity.DepthTextures[t],
                        modelEntity.TextureSmall ? _idTextureSmall : _idTextureBig,
                        (uint)(modelEntity.TextureSmall ? 3 : 4));
                }
                _entityRender[i] = new EntityRender(gl, Render, modelEntity, _game.Player);
            }
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
                        if (entity.Id != playerId)// && _game.Player.IsBoxInFrustum(entity.GetBoundingBoxOffset(-x, -y, -z)))
                        {
                            // HitBox
                            if (_isHitBox)
                            {
                                Render.ShaderBindLine(_game.Player.View,
                                    entity.GetPosFrameX(timeIndex) - x,
                                    entity.GetPosFrameY(timeIndex) - y,
                                    entity.GetPosFrameZ(timeIndex) - z);
                                _hitbox.Draw(timeIndex, entity);
                            }
                            // Model
                            entity.Render.Draw(timeIndex, _game.DeltaTimeFrame);
                            CountEntitiesFC++;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Получить объект рендера сущности по индексу
        /// </summary>
        public EntityRender GetEntityRender(int indexEntity) => _entityRender[indexEntity];

        /// <summary>
        /// Метод для прорисовки основного игрока
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public void DrawOwner(float timeIndex)
        {
            _game.WorldRender.Render.ShaderBindLine(_game.Player.View, 0, 0, 0);
            _hitbox.Draw(timeIndex, _game.Player);
        }

        /// <summary>
        /// Игровой такт
        /// </summary>
        /// <param name="deltaTime">Дельта последнего тика в mc</param>
        public override void OnTick(float deltaTime)
        {
        }

        public override void Dispose()
        {
            _hitbox.Dispose();
            if (_entityRender != null)
            {
                for (int i = 0; i < _entityRender.Length; i++)
                    if (_entityRender[i] != null) _entityRender[i].Dispose();
                //_entityRender.Dispose();
            }
        }

    }
}
