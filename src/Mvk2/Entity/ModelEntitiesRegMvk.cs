using Vge.Entity;
using Vge.Entity.List;

namespace Mvk2.Entity
{
    /// <summary>
    /// Регистрация моделей сущностей для Малювеки 2
    /// </summary>
    public sealed class ModelEntitiesRegMvk
    {
        public static void Initialization()
        {
            //ModelEntitiesReg.RegisterModelEntityClass("Chick");
            ModelEntitiesReg.RegisterModelEntityClass("Chicken", typeof(EntityThrowable));
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
