using Mvk2.World.BlockEntity.List;
using Vge.World.BlockEntity;

namespace Mvk2.World.BlockEntity
{
    /// <summary>
    /// Регистрация блоков сущностей для Малювеки 2
    /// </summary>
    public sealed class BlocksEntityRegMvk
    {
        public static void Initialization()
        {
            BlocksEntityReg.RegisterBlockEntityClass("Tree", typeof(BlockEntityTree));
        }
    }
}
