using System;
using System.Runtime.CompilerServices;
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
        public byte MaxStackSize { get; protected set; }
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

        /// <summary>
        /// Название анимации держать, если не указана, то будет по умолчанию 
        /// </summary>
        public string Hold { get; protected set; } = "";

        /// <summary>
        /// Массив ключей ячеек одежды инвентаря, куда можно устанавливать этот предмет.
        /// Если null, то можно установить только в ячейку с ключём 0
        /// </summary>
        protected byte[] _slotClothIndex;

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

        /// <summary>
        /// Проверить может ли предмет быть в текущем слоте одежды с данным ключём
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual bool CheckSlotClothKey(byte key)
        {
            if (_slotClothIndex == null) return key == 0;
            foreach(byte slot in _slotClothIndex)
            {
                if (slot == key) return true;
            }
            return false;
        }

        /// <summary>
        /// Имеется ли замена предмета при выборе его в StackAir
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual bool CheckToAir() => false;

        /// <summary>
        /// Заменить предмет из-за воздуха, при выборе его в StackAir
        /// TODO::2025-09-22 это надо горящий факел, чтоб зутухал, для горяжего факела, сделать доп индекс хранения
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual ushort IndexReplaceItemDueAir() => 0;

        #region Методы для импорта данных с json

        /// <summary>
        /// Прочесть состояние блока из Json формы
        /// </summary>
        protected virtual void _ReadStateFromJson(JsonCompound state)
        {
            if (state.Items != null)
            {
                try
                {
                    // Статы
                    foreach (JsonKeyValue json in state.Items)
                    {
                        if (json.IsKey(Cti.MaxStackSize))
                        {
                            int size = json.GetInt();
                            if (size > 255) size = 255;
                            if (size < 1) size = 1;
                            MaxStackSize = (byte)size;
                        }
                        else if (json.IsKey(Cti.MaxDamage)) MaxDamage = json.GetInt();
                        else if (json.IsKey(Cti.SlotClothIndex))
                        {
                            if (json.IsValue()) // byte
                            {
                                _slotClothIndex = new byte[] { (byte)json.GetInt() };
                            }
                            else if (json.IsArray()) // Array
                            {
                                _slotClothIndex = json.GetArray().ToArrayByte();
                            }
                        }
                        else if (json.IsKey(Cti.Width)) Width = json.GetFloat();
                        else if (json.IsKey(Cti.Height)) Height = json.GetFloat();
                        else if (json.IsKey(Cti.Weight)) Weight = json.GetInt();
                        else if (json.IsKey(Cti.Rebound)) Rebound = json.GetFloat();
                        else if (json.IsKey(Cti.Hold)) Hold = json.GetString();
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
