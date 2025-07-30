using System.Runtime.CompilerServices;
using Vge.Renderer.World.Entity;
using Vge.World;
using Vge.World.Block;
using Vge.World.Chunk;

namespace Vge.Entity.Render
{
    /// <summary>
    /// Объект рендера сущности, хранящий данные рендера, без анимации. Только для клиента
    /// </summary>
    public class EntityRenderClient : EntityRenderBase
    {
        /// <summary>
        /// Объект рендера всех сущностей
        /// </summary>
        public readonly EntitiesRenderer Entities;

        /// <summary>
        /// Статичный объект сетки типа сущности, не меняется
        /// </summary>
        protected EntityRender _entityRender;

        /// <summary>
        /// Объект для рендера слоёв (предметов или одежды) на сущности
        /// </summary>
        protected readonly EntityLayerRender _entityLayerRender;

        /// <summary>
        /// Ресурсы сущности, нужны везде, но форма только на клиенте
        /// </summary>
        protected readonly ResourcesEntity _resourcesEntity;

        /// <summary>
        /// Освещёность блочного света
        /// </summary>
        protected float _lightBlock;
        /// <summary>
        /// Освещёность небесного света
        /// </summary>
        protected float _lightSky;

        public EntityRenderClient(EntityBase entity, EntitiesRenderer entities, ResourcesEntity resourcesEntity) : base(entity)
        {
            Entities = entities;
            _resourcesEntity = resourcesEntity;

            // Если имеется инвентарь, то создаём объект слоёв
            if (Entity is EntityLiving || _resourcesEntity.IsAnimation) // Временно реагируем на живую сущность
            {
                _entityLayerRender = new EntityLayerRender(entities);
            }
        }

        /// <summary>
        /// Обновить рассчитать матрицы для кадра
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        /// <param name="deltaTime">Дельта последнего кадра в mc</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void UpdateMatrix(float timeIndex, float deltaTime)
        {
            if (_entityRender == null)
            {
                _entityRender = Entities.GetEntityRender(Entity.IndexEntity);
            }
        }

        /// <summary>
        /// Игровой такт на клиенте
        /// </summary>
        /// <param name="deltaTime">Дельта последнего тика в mc</param>
        public override void UpdateClient(WorldClient world, float deltaTime)
        {
            // Проверяем освещение
            _BrightnessForRender(world);
        }

        /// <summary>
        /// Метод для прорисовки
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        /// <param name="deltaTime">Дельта последнего кадра в mc</param>
        public override void Draw(float timeIndex, float deltaTime)
        {
            float ppfx = Entities.Player.PosFrameX;
            float ppfy = Entities.Player.PosFrameY;
            float ppfz = Entities.Player.PosFrameZ;

            // Заносим в шейдор
            Entities.ShsEntity.UniformData(
                Entity.GetPosFrameX(timeIndex) - ppfx,
                Entity.GetPosFrameY(timeIndex) - ppfy,
                Entity.GetPosFrameZ(timeIndex) - ppfz,
                _lightBlock, _lightSky,
                _resourcesEntity.GetDepthTextureAndSmall(),
                _resourcesEntity.GetIsAnimation(), 0
                );

            // Рисуем основную сетку сущности
            _entityRender.MeshDraw();

            // Если имеются слои, рисуем сетку слоёв
            _entityLayerRender?.MeshDraw();
        }

        /// <summary>
        /// Получить яркость для рендера 0.0 - 1.0
        /// </summary>
        private void _BrightnessForRender(WorldClient world)
        {
            BlockPos blockPos = new BlockPos(Entity.PosX, Entity.PosY + Entity.Size.GetHeight() * .85f, Entity.PosZ);
            if (blockPos.IsValid(world.ChunkPr.Settings))
            {
                ChunkBase chunk = world.GetChunk(blockPos.GetPositionChunk());
                if (chunk != null)
                {
                    ChunkStorage chunkStorage = chunk.StorageArrays[blockPos.Y >> 4];
                    int index = (blockPos.Y & 15) << 8 | (blockPos.Z & 15) << 4 | (blockPos.X & 15);
                    _lightBlock = (chunkStorage.Light[index] >> 4) / 16f + .03125f;
                    _lightSky = (chunkStorage.Light[index] & 15) / 16f + .03125f;
                }
                else
                {
                    // Если блок не определён
                    _lightBlock = 0;
                    _lightSky = 1;
                }
            }
            else
            {
                // Если блок не определён
                _lightBlock = 0;
                _lightSky = 1;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Dispose() => _entityLayerRender?.Dispose();
    }
}
