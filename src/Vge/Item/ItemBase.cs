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

        #region Для Render

        /// <summary>
        /// Буфер сетки для рендера
        /// </summary>
        private VertexEntityBuffer _buffer;
        /// <summary>
        /// Буфер сетки для рендера для Gui
        /// </summary>
        private VertexEntityBuffer _bufferGui;

        #endregion

        #region Init

        /// <summary>
        /// Инициализация предметов данные с json, 
        /// это не спрайти, а модель предмета или модель с блока
        /// </summary>
        public virtual void InitAndJoinN2(JsonCompound state, JsonCompound shape, bool isItem)
        {
            if (state.Items != null)
            {
                _ReadStateFromJson(state);
                ItemShapeDefinition shapeDefinition = new ItemShapeDefinition(Alias);
                _buffer = ItemShapeSprite.Convert(
                    shapeDefinition.RunShapeItemFromJson(state.GetObject(Cti.View), shape)
                );
            }
        }

        /// <summary>
        /// Инициализация предметов данные с json, 
        /// это спрайт
        /// </summary>
        public virtual void InitAndJoinN2(JsonCompound state)
        {
            if (state.Items != null)
            {
                _ReadStateFromJson(state);
                // Форма из спрайта
                ItemShapeSprite shapeSprite = new ItemShapeSprite(Alias, state.GetString(Cti.Sprite));
                _buffer = shapeSprite.GenBuffer();
            }
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
                    }
                }
                catch
                {
                    throw new Exception(Sr.GetString(Sr.ErrorReadJsonItemStat, Alias));
                }
            }
        }

        #endregion

        #region Render

        /// <summary>
        /// Получить буфер сетки предмета для рендера
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual VertexEntityBuffer GetBuffer() => _buffer;

        #endregion

        public override string ToString() => IndexItem.ToString() + " " + Alias;
    }
}
