using System;
using Vge.Util;

namespace Vge.Entity.Particle
{
    // <summary>
    /// Регистрация сущностей частичек
    /// </summary>
    public sealed class EntitiesFXReg
    {
        /// <summary>
        /// Индекс частички блока
        /// </summary>
        public static ushort PartId { get; private set; }

        /// <summary>
        /// Таблица предметов для регистрации
        /// </summary>
        public static readonly EntitiesRegTable Table = new EntitiesRegTable();

        /// <summary>
        /// Инициализация блоков, если window не указывать, прорисовки о статусе не будет (для сервера)
        /// </summary>
        public static void Initialization(WindowMain window = null)
        {
            if (window != null)
            {
                window.LScreen.Process(L.T("CreateItems"));
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

            // Регистрация обязательных предметов
            RegisterEntityFXClass("Part", typeof(EntityPartFX));
        }

        /// <summary>
        /// Зарегистрировать Частичку
        /// </summary>
        public static void RegisterEntityFXClass(string alias, Type entityFxType)
            => Table.Add(alias, new ResourcesEntityBase(alias, entityFxType));

        /// <summary>
        /// Корректировка блоков после загрузки, если загрузки нет,
        /// всё равно надо, для активации
        /// </summary>
        public static void Correct(CorrectTable correct)
        {
            correct.CorrectRegLoad(Table);
            Ce.EntitiesFX = new EntityFXArrays();
            // Очистить таблицы и вспомогательные данные json
            _Clear();
        }

        /// <summary>
        /// Очистить таблицы и вспомогательные данные json
        /// </summary>
        private static void _Clear() => Table.Clear();

        /// <summary>
        /// Инициализация Id
        /// </summary>
        public static void InitId()
        {
            string alias;
            for (ushort i = 0; i < Ce.EntitiesFX.Count; i++)
            {
                alias = Ce.EntitiesFX.EntitiesFXAlias[i];
                if (alias == "Part") PartId = i;
            }
        }
    }
}
