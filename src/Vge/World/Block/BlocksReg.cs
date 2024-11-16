using System;
using Vge.World.Block.List;

namespace Vge.World.Block
{
    /// <summary>
    /// Регистрация блоков
    /// </summary>
    public sealed class BlocksReg
    {

        public static void Initialization()
        {
            // Очистить массивы регистрации перед началом заполнения
            Table.Clear();

            // Первые обязательные блоки
            RegisterBlockClass("Air", new BlockAir());
            RegisterBlockClass("Debug", new BlockBase());
        }

        /// <summary>
        /// Корректировка блоков после загрузки, если загрузки нет,
        /// всё равно надо, для активации
        /// </summary>
        public static void Correct(CorrectTable correct)
        {
            correct.CorrectRegLoad(Table);
            try
            {
                int c = Blocks.Count;
            }
            catch (Exception ex)
            {
                throw ex.InnerException;
            }
            // Очистить массивы регистрации
            Table.Clear();
        }

        /// <summary>
        /// Таблица блоков для регистрации
        /// </summary>
        public static readonly BlockRegTable Table = new BlockRegTable();

        /// <summary>
        /// Зврегистрировать блок
        /// </summary>
        public static void RegisterBlockClass(string alias, BlockBase blockObject)
        {
            Table.Add(alias, blockObject);
        }
    }
}
