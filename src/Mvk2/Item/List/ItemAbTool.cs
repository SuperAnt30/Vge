using System;
using System.Runtime.CompilerServices;
using Vge.Item;
using Vge.Json;
using Vge.Realms;

namespace Mvk2.Item.List
{
    /// <summary>
    /// Абстрактный класс предмета инструмент
    /// </summary>
    public abstract class ItemAbTool : ItemBase
    {
        /// <summary>
        /// уровень инструмента, Качество дропа, 0-минимум, 1-левел инструмента, 2-левел, 3-левел
        /// </summary>
        public int Level { get; private set; }
        /// <summary>
        /// ID для графики стола, значение инструмента от 1 до 10
        /// </summary>
        //public ushort TableId { get; private set; } = 0;
        /// <summary>
        /// Сила урона для атаки
        /// </summary>
        public float Damage { get; protected set; } = 1;


        #region Методы для импорта данных с json

        /// <summary>
        /// Прочесть состояние блока из Json формы
        /// </summary>
        protected override void _ReadStateFromJson(JsonCompound state)
        {
            base._ReadStateFromJson(state);
            if (state.Items != null)
            {
                try
                {
                    // Статы
                    foreach (JsonKeyValue json in state.Items)
                    {
                        if (json.IsKey(Cti.Damage)) Damage = json.GetFloat();
                        else if (json.IsKey(Cti.Level)) Level = json.GetInt();
                    }
                }
                catch
                {
                    throw new Exception(Sr.GetString(Sr.ErrorReadJsonItemStat, Alias));
                }
            }
        }

        #endregion

        /// <summary>
        /// Задать подсказку
        /// </summary>
        public override void SetToolTipLang(string toolTip)
        {
            base.SetToolTipLang(toolTip);
            _toolTip = _toolTip + ChatStyle.Br + "-" + ChatStyle.Br
                + "Level {0}" + ChatStyle.Br
                + "Damage {1}" + ChatStyle.Br
                + "{2}";
        }

        /// <summary>
        /// Текст в подсказке для GUI
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string GetToolTip(ItemStack stack)
            => string.Format(_toolTip, 
                Level, Damage,
                stack.ToStringDamaged());
    }
}
