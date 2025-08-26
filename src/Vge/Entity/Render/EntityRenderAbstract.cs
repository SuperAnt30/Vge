using Vge.Renderer.World.Entity;
using Vge.World;
using Vge.World.Block;
using Vge.World.Chunk;

namespace Vge.Entity.Render
{
    /// <summary>
    /// Абстрактный объект рендера сущности, хранящий данные рендера, без анимации. Только для клиента
    /// </summary>
    public abstract class EntityRenderAbstract : EntityRenderBase
    {
        /// <summary>
        /// Сущность к которой прекреплена физика
        /// </summary>
        public readonly EntityBase Entity;

        /// <summary>
        /// Объект рендера всех сущностей
        /// </summary>
        public readonly EntitiesRenderer Entities;

        /// <summary>
        /// Освещёность блочного света
        /// </summary>
        protected float _lightBlock;
        /// <summary>
        /// Освещёность небесного света
        /// </summary>
        protected float _lightSky;


        public EntityRenderAbstract(EntityBase entity, EntitiesRenderer entities)
        {
            Entity = entity;
            Entities = entities;
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
    }
}
