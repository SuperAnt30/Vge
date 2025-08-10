using System.Runtime.CompilerServices;
using Vge.Renderer.World.Entity;
using Vge.World;

namespace Vge.Entity.Render
{
    /// <summary>
    /// Объект рендера сущности, хранящий данные рендера, с АНИМАЦИЕЙ и глазами и возможно ртом.
    /// Только для клиента
    /// </summary>
    public class EntityRenderEyeLips : EntityRenderAnimation
    {

        public EntityRenderEyeLips(EntityBase entity, EntitiesRenderer entities, ResourcesEntity resourcesEntity) 
            : base(entity, entities, resourcesEntity)
        {
            
        }

        int timeeye = 0;
        int timelipsSmile = 0;
        int timelips = 0;

        /// <summary>
        /// Игровой такт на клиенте
        /// </summary>
        /// <param name="deltaTime">Дельта последнего тика в mc</param>
        public override void UpdateClient(WorldClient world, float deltaTime)
        {
            base.UpdateClient(world, deltaTime);

            // TEST
            timeeye++;
            if (timeeye > 50) timeeye = 0;
            timelips++;
            if (timelips > 8) timelips = 0;
            timelipsSmile++;
            if (timelipsSmile > 150) timelipsSmile = 0;
          
        }

        /// <summary>
        /// Получить параметр для шейдора, на состояния глаз и рта
        /// Значение 1 это открыты глаза, закрыт рот.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override int GetEyeLips()
        {
            int eye = (timeeye > 5) ? 1 : 0; // глаза
            int lips = 0; // губы
            if (timelipsSmile > 130) lips = 2; // улыбка
            else if (timelipsSmile > 70) lips = (timelips > 4) ? 1 : 0; // балтает
            int eyeLips = lips << 1 | eye;

            return eyeLips;
        }
    }
}
