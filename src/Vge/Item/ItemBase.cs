using System;
using System.Runtime.CompilerServices;
using Vge.Json;
using Vge.World.Block;

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
        /// Массив сторон прямоугольных форм
        /// </summary>
        private QuadSide[] _quads;
        /// <summary>
        /// Массив сторон прямоугольных форм
        /// </summary>
     //   private QuadSide[] _quads;

        #endregion

        #region Init

        /// <summary>
        /// Инициализация предметов данные с json
        /// </summary>
        public virtual void InitAndJoinN2(JsonCompound state, JsonCompound shape)
        {
            if (state.Items != null)
            {
                _ReadStateFromJson(state);
                // Модель
                _ShapeDefinition(state, shape);
            }
            //_InitBlockRender();
        }

        /// <summary>
        /// Инициализация предметов данные с json
        /// </summary>
        public virtual void InitAndJoinN2(JsonCompound state)
        {
            if (state.Items != null)
            {
                _ReadStateFromJson(state);
                // Модель
                // Cti.Sprite
                //_ShapeDefinition(state, shape);
            }
            //_InitBlockRender();
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

        /// <summary>
        /// Получить модель
        /// </summary>
        protected virtual void _ShapeDefinition(JsonCompound state, JsonCompound shape)
        {
            ItemShapeDefinition shapeDefinition = new ItemShapeDefinition(Alias);
            _quads = shapeDefinition.RunShapeItemFromJson(state.GetObject(Cti.View), shape);
        }

        #endregion

        #region Render

        /// <summary>
        /// Массив сторон прямоугольных форм для рендера
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual QuadSide[] GetQuads() => _quads;

        #endregion

        public override string ToString() => IndexItem.ToString() + " " + Alias;
    }
}
