using Mvk2.World.BlockEntity.List;
using System;
using Vge.World.BlockEntity;

namespace Mvk2.World.BlockEntity
{
    /// <summary>
    /// Регистрация блоков сущностей для Малювеки 2
    /// </summary>
    public sealed class BlocksEntityRegMvk
    {
        /// <summary>
        /// Id Блок сущности дерева
        /// </summary>
        public static ushort IdTree { get; private set; }

        /// <summary>
        /// Инициализация Id
        /// </summary>
        public static void InitId()
        {
            IdTree = _GetId("Tree");
        }

        public static void Initialization()
        {
            BlocksEntityReg.RegisterBlockEntityClass("Tree", typeof(BlockEntityTree));
        }

        /// <summary>
        /// Получить Id предмета
        /// </summary>
        private static ushort _GetId(string key)
        {
            int count = Ce.BlocksEntity.BlockEntityAlias.Length;
            for (ushort i = 0; i < count; i++)
            {
                if (key == Ce.BlocksEntity.BlockEntityAlias[i])
                {
                    return i;
                }
            }
            throw new Exception("BlocksEntity [" + key + "] null");
        }
    }
}
