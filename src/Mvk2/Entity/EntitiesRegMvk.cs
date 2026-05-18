using Vge.Entity;
using Vge.Entity.List;

namespace Mvk2.Entity
{
    /// <summary>
    /// Регистрация моделей сущностей для Малювеки 2
    /// </summary>
    public sealed class EntitiesRegMvk
    {
        /// <summary>
        /// Индекс курочки
        /// </summary>
        public static ushort ChickenId { get; private set; }

        /// <summary>
        /// Инициализация Id
        /// </summary>
        public static void InitId()
        {
            string alias;
            for (ushort i = 0; i < Ce.Entities.Count; i++)
            {
                alias = Ce.Entities.EntitiesAlias[i];
                if (alias == "Chicken") ChickenId = i;
                //else if (alias == "Droplet") DropletId = i;
            }
        }

        public static void Initialization()
        {
            //ModelEntitiesReg.RegisterModelEntityClass("Chick");
            //EntitiesReg.RegisterEntityClass("Robinson", typeof(EntityThrowable));
            EntitiesReg.RegisterEntityClass("Chicken", typeof(EntityThrowableSmall));
            //ModelEntitiesReg.RegisterModelEntityClass("Robinson");
            //ModelEntitiesReg.RegisterModelEntityClass("Skeleton");
            //ModelEntitiesReg.RegisterModelEntityClass("Robinson2");
            //ModelEntitiesReg.RegisterModelEntityClass("Robinson3");
            //ModelEntitiesReg.RegisterModelEntityClass("Robinson4");
            //ModelEntitiesReg.RegisterModelEntityClass("Chicken3");
            //ModelEntitiesReg.RegisterModelEntityClass("Chicken4");
            //ModelEntitiesReg.RegisterModelEntityClass("Robinson5");
        }
    }
}
