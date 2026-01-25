using System;
using Vge.Entity;
using Vge.Util;

namespace Vge.World.BlockEntity
{
    /// <summary>
    /// Регистрация блоков сущностей
    /// </summary>
    public sealed class BlocksEntityReg
    {
        /// <summary>
        /// Таблица блоков сущностей для регистрации
        /// </summary>
        public static readonly EntitiesRegTable Table = new EntitiesRegTable();

        /// <summary>
        /// Инициализация блоков, если window не указывать, прорисовки о статусе не будет (для сервера)
        /// </summary>
        public static void Initialization(WindowMain window = null)
        {
            if (window != null)
            {
                window.LScreen.Process(L.T("CreateBlocksEntity"));
                window.DrawFrame();
            }

            _InitializationBegin();
        }

        /// <summary>
        /// Перед инициализацией
        /// </summary>
        private static void _InitializationBegin()
        {
            // Очистить таблицы и вспомогательные данные json
            _Clear();

            // Регистрация обязательных блоков
            // Воздух
            //Air = new BlockAir();
            //Air.InitAliasAndJoinN1(AliasAir, new JsonCompound(), new JsonCompound(new JsonKeyValue[] { }));
            //Table.Add(AliasAir, Air);
        }

        /// <summary>
        /// Корректировка блоков после загрузки, если загрузки нет,
        /// всё равно надо, для активации
        /// </summary>
        public static void Correct(CorrectTable correct)
        {
            correct.CorrectRegLoad(Table);
            Ce.BlocksEntity = new BlockEntityArrays();

            // Очистить таблицы и вспомогательные данные json
            _Clear();
        }

        /// <summary>
        /// Очистить таблицы и вспомогательные данные json
        /// </summary>
        private static void _Clear()
        {
            // Очистить массивы регистрации
            Table.Clear();
        }

        /// <summary>
        /// Зарегистрировать блок сущности
        /// </summary>
        public static void RegisterBlockEntityClass(string alias, Type entityType)
        {
            Table.Add(alias, new ResourcesEntityBase(alias, entityType));
        }
    }
}
