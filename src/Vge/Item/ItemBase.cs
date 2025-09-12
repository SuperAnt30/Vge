using System;
using System.Runtime.CompilerServices;
using Vge.Entity.Render;
using Vge.Json;

namespace Vge.Item
{
    /// <summary>
    /// Базовый объект Предмета
    /// </summary>
    public class ItemBase
    {
        /// <summary>
        /// Индекс предмета из таблицы
        /// </summary>
        public ushort IndexItem { get; private set; }
        /// <summary>
        /// Псевдоним предмета из таблицы
        /// </summary>
        public string Alias { get; private set; }

        /// <summary>
        /// Название предмета на текущем языке
        /// </summary>
        public string CurentName { get; protected set; } = "";
        /// <summary>
        /// Описание предмета на текущем языке
        /// </summary>
        public string Description { get; protected set; } = "";

        /// <summary>
        /// Максимальное количество однотипный вещей в одной ячейке
        /// </summary>
        public int MaxStackSize { get; protected set; }
        /// <summary>
        /// Максимальное количество урона, при 0, нет учёта урона
        /// </summary>
        public int MaxDamage { get; protected set; }

        /// <summary>
        /// Пол ширина предмета
        /// </summary>
        public float Width { get; protected set; } = .4375f;
        /// <summary>
        /// Высота предмета
        /// </summary>
        public float Height { get; protected set; } = .125f;
        /// <summary>
        /// Вес предмета. В килограммах.
        /// </summary>
        public int Weight { get; protected set; } = 2;
        /// <summary>
        /// Коэффициент рикошета, 0 нет отскока, 1 максимальный
        /// </summary>
        public float Rebound { get; protected set; } = .5f;

        /// <summary>
        /// Объект буферов для рендера
        /// </summary>
        public ItemRenderBuffer Buffer { get; protected set; }

        #region Init

        /// <summary>
        /// Инициализация предметов данные с json, 
        /// </summary>
        public virtual void Init(JsonCompound state)
        {
            _ReadStateFromJson(state);
            // Создание буфера для рендера
            Buffer = new ItemRenderBuffer();
        }
        /// <summary>
        /// Удалить объект данных буфера, он уже должен быть в буфере GPU
        /// </summary>
        public void BufferClear()
        {
            Buffer.Clear();
            //Buffer = null;
        }

        /// <summary>
        /// Задать индекс, из таблицы
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetIndex(ushort id) => IndexItem = id;

        /// <summary>
        /// Задать псевдоним
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetAlias(string alias) => Alias = alias;

        #endregion

        #region Методы для импорта данных с json

        /// <summary>
        /// Прочесть состояние блока из Json формы
        /// </summary>
        private void _ReadStateFromJson(JsonCompound state)
        {
            if (state.Items != null)
            {
                try
                {
                    // Статы
                    foreach (JsonKeyValue json in state.Items)
                    {
                        if (json.IsKey(Cti.MaxStackSize)) MaxStackSize = json.GetInt();
                        if (json.IsKey(Cti.MaxDamage)) MaxDamage = json.GetInt();
                        if (json.IsKey(Cti.Width)) Width = json.GetFloat();
                        if (json.IsKey(Cti.Height)) Height = json.GetFloat();
                        if (json.IsKey(Cti.Weight)) Weight = json.GetInt();
                        if (json.IsKey(Cti.Rebound)) Rebound = json.GetFloat();
                    }
                }
                catch
                {
                    throw new Exception(Sr.GetString(Sr.ErrorReadJsonItemStat, Alias));
                }
            }
        }

        #endregion

        

        public override string ToString() => IndexItem.ToString() + " " + Alias;
    }
}
