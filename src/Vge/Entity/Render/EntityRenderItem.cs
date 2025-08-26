using System.Runtime.CompilerServices;
using Vge.Entity.List;
using Vge.Item;
using Vge.Renderer.World.Entity;

namespace Vge.Entity.Render
{
    /// <summary>
    /// Объект рендера сущности, хранящий данные рендера, без анимации. Только для клиента
    /// </summary>
    public class EntityRenderItem : EntityRenderAbstract
    {
        public EntityRenderItem(EntityBase entity, EntitiesRenderer entities)
            : base(entity, entities) { }

        /// <summary>
        /// Обновить рассчитать матрицы для кадра
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        /// <param name="deltaTime">Дельта последнего кадра в mc</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void UpdateMatrix(float timeIndex, float deltaTime)
        {
            if (_entityRender == null && Entity is EntityItem entityItem)
            {
                ItemStack itemStack = entityItem.GetEntityItemStack();
                if (itemStack != null)
                {
                    _entityRender = Entities.GetItemRender(itemStack.Item.IndexItem);
                }
            }
        }

        /// <summary>
        /// Метод для прорисовки
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        /// <param name="deltaTime">Дельта последнего кадра в mc</param>
        public override void Draw(float timeIndex, float deltaTime)
        {
            if (_entityRender != null) // Может быть случай, когда пакет данных стака ещё не пришёл
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
                    0, 0, 0);

                // Рисуем основную сетку сущности
                _entityRender.MeshDraw();
            }
        }
    }
}
