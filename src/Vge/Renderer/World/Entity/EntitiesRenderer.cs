using System.Runtime.CompilerServices;
using Vge.Entity;
using Vge.Entity.Player;
using Vge.Entity.Render;
using Vge.Games;
using Vge.Renderer.Shaders;
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
        /// Видем ли мы хитбокс сущности
        /// </summary>
        public bool IsHitBox = false;

        /// <summary>
        /// Объект буфера
        /// </summary>
        public readonly VertexLayersBuffer Buffer = new VertexLayersBuffer();

        /// <summary>
        /// Объект OpenGL для элемента управления
        /// </summary>
        private readonly GL gl;
        /// <summary>
        /// Метод чанков для прорисовки
        /// </summary>
        private readonly ArrayFast<ChunkRender> _arrayChunkRender;
        /// <summary>
        /// Объект для отладки хитбокса сущности
        /// </summary>
        private readonly HitboxEntityRender _hitbox;
        
        /// <summary>
        /// Массив всех типов сущностей
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

        /// <summary>
        /// Шейдора для сущностей
        /// </summary>
        public readonly ShadersEntity ShsEntity;
        

        public EntitiesRenderer(GameBase game, ArrayFast<ChunkRender> arrayChunkRender) : base(game)
        {
            gl = GetOpenGL();
            ShsEntity = game.Render.ShsEntity;
            _hitbox = new HitboxEntityRender(gl);
            _arrayChunkRender = arrayChunkRender;
        }

        /// <summary>
        /// Объект игрока для клиента
        /// </summary>
        public PlayerClientOwner Player
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _game.Player;
        }

        /// <summary>
        /// Игра запущена, всё загружено из библиотек
        /// </summary>
        public void GameStarting()
        {
            _idTextureSmall = Render.Texture.CreateTexture2dArray(
                EntitiesReg.TextureManager.WidthSmall,
                EntitiesReg.TextureManager.HeightSmall,
                EntitiesReg.TextureManager.DepthSmall, 
                (uint)Gi.ActiveTextureSamplerSmall);

            _idTextureBig = Render.Texture.CreateTexture2dArray(
                EntitiesReg.TextureManager.WidthBig,
                EntitiesReg.TextureManager.HeightBig,
                EntitiesReg.TextureManager.DepthBig, 
                (uint)Gi.ActiveTextureSamplerBig);

            // Заносим все текстуры сущностей
            EntitiesReg.SetImageTexture2dArray(Render.Texture, _idTextureSmall, _idTextureBig);

            // Создаём объекты рендора всех типов сущностей
            int count = Ce.Entities.Count;
            _entityRender = new EntityRender[count];
            ResourcesEntity resourcesEntity;
            for (ushort id = 0; id < count; id++)
            {
                resourcesEntity = Ce.Entities.GetModelEntity(id);
                _entityRender[id] = new EntityRender(gl, resourcesEntity);
            }
        }

        /// <summary>
        /// Метод для прорисовки кадра
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public override void Draw(float timeIndex)
        {
            CountEntitiesFC = 0;
            int count = _arrayChunkRender.Count;
            int countEntity;
            ChunkRender chunkRender;
            EntityBase entity;
            int playerId = _game.Player.Id;
            float x = _game.Player.PosFrameX;
            float y = _game.Player.PosFrameY;
            float z = _game.Player.PosFrameZ;

            // Параметрв шейдоров
            ShsEntity.BindUniformBigin();

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
                            // Model
                            entity.Render.Draw(timeIndex, _game.DeltaTimeFrame);
                            CountEntitiesFC++;
                        }
                    }
                }
            }

            if (IsHitBox)
            {
                // Есть хит бокс сущности
                // Пробегаемся заного по всем сущностям, это будет быстрее, чем биндить шейдера
                Render.ShLine.Bind();
                Render.ShLine.SetUniformMatrix4("view", Gi.MatrixView);
                for (int i = 0; i < count; i++)
                {
                    chunkRender = _arrayChunkRender[i];
                    countEntity = chunkRender.ListEntities.Count;
                    if (countEntity > 0)
                    {
                        for (int j = 0; j < countEntity; j++)
                        {
                            entity = chunkRender.ListEntities.GetAt(j);
                            if (entity.Id != playerId)
                            {
                                Render.ShLine.SetUniform3("pos",
                                    entity.GetPosFrameX(timeIndex) - x,
                                    entity.GetPosFrameY(timeIndex) - y,
                                    entity.GetPosFrameZ(timeIndex) - z);
                                _hitbox.Draw(timeIndex, entity);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Метод для прорисовки кадра теней
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public void DrawDepthMap(float timeIndex)
        {
            int count = _arrayChunkRender.Count;
            if (count > ShadowMapping.CountChunkShadowMap) count = ShadowMapping.CountChunkShadowMap;
            int countEntity;
            ChunkRender chunkRender;

            // Параметрв шейдоров
            ShsEntity.BindUniformBiginDepthMap();

            for (int i = 0; i < count; i++)
            {
                chunkRender = _arrayChunkRender[i];
                countEntity = chunkRender.ListEntities.Count;
                if (countEntity > 0)
                {
                    for (int j = 0; j < countEntity; j++)
                    {
                        chunkRender.ListEntities.GetAt(j).Render.Draw(timeIndex, _game.DeltaTimeFrame);
                    }
                }
            }
        }

        /// <summary>
        /// Метод для прорисовки кадра теней
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public void UpdateMatrix(float timeIndex)
        {
            int count = _arrayChunkRender.Count;
            int countEntity;
            ChunkRender chunkRender;
            for (int i = 0; i < count; i++)
            {
                chunkRender = _arrayChunkRender[i];
                countEntity = chunkRender.ListEntities.Count;
                if (countEntity > 0)
                {
                    for (int j = 0; j < countEntity; j++)
                    {
                        chunkRender.ListEntities.GetAt(j).Render.UpdateMatrix(timeIndex, _game.DeltaTimeFrame);
                    }
                }
            }
        }

        /// <summary>
        /// Получить объект рендера сущности по индексу
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EntityRender GetEntityRender(int indexEntity) => _entityRender[indexEntity];

        /// <summary>
        /// Метод для прорисовки основного игрока
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public void DrawOwner(float timeIndex)
        {
            // Параметрв шейдоров
            if (_game.Player.ViewCamera != EnumViewCamera.Eye)
            {
                ShsEntity.BindUniformBigin();
                _game.Player.Render.Draw(timeIndex, _game.DeltaTimeFrame);
            }

            if (IsHitBox)
            {
                _game.WorldRender.Render.ShaderBindLine(Gi.MatrixView, 0, 0, 0);
                _hitbox.Draw(timeIndex, _game.Player);
            }
        }

        public override void Dispose()
        {
            _hitbox.Dispose();
            // Удаление текстур для сущностей
            if (_idTextureSmall != 0)
            {
                _game.WorldRender.Render.Texture.DeleteTexture(_idTextureSmall);
                _idTextureSmall = 0;
            }
            if (_idTextureBig != 0)
            {
                _game.WorldRender.Render.Texture.DeleteTexture(_idTextureBig);
                _idTextureBig = 0;
            }
            if (_entityRender != null)
            {
                for (int i = 0; i < _entityRender.Length; i++)
                {
                    _entityRender[i]?.Dispose();
                }
            }
        }

    }
}
