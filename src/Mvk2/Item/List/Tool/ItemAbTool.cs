using System;
using System.Runtime.CompilerServices;
using Vge.Item;
using Vge.Json;
using Vge.Realms;

namespace Mvk2.Item.List.Tool
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
        private float _damage = 1;
        /// <summary>
        /// Пауза между ударами в тактах
        /// </summary>
        protected int _pause;
        /// <summary>
        /// Ускорение разрушении блока, по умолчанию 1, при 0 ломает за один удар.
        /// </summary>
        protected float _acceleration;

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
                        if (json.IsKey(Cti.Damage)) _damage = json.GetFloat();
                        else if (json.IsKey(Cti.Level)) Level = json.GetInt();
                        else if (json.IsKey(Cti.Pause)) _pause = json.GetInt();
                        else if(json.IsKey(Cti.Acceleration)) _acceleration = json.GetFloat();
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
                + "{2} [{3} {4}]";
        }

        /// <summary>
        /// Текст в подсказке для GUI
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string GetToolTip(ItemStack stack)
            => string.Format(_toolTip, 
                Level, _damage,
                stack.ToStringDamaged(),
                _pause, _acceleration);

        /// <summary>
        /// Получить урон для атаки предметом который в руке
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override float GetDamageToAttack() => _damage;
    }
}
