using System;
using System.Runtime.CompilerServices;
using Vge.Entity.Player;
using Vge.Item;
using Vge.Json;
using Vge.Realms;
using Vge.Util;

namespace Mvk2.Item.List.Tools
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
        /// Пауза между ударами в тактах
        /// </summary>
        public int Pause { get; protected set; }
        /// <summary>
        /// Ускорение разрушении блока, по умолчанию 1, при 0 ломает за один удар.
        /// Как множетель %
        /// block.Hardness * Acceleration
        /// </summary>
        public float Acceleration { get; protected set; }

        /// <summary>
        /// ID для графики стола, значение инструмента от 1 до 10
        /// </summary>
        //public ushort TableId { get; private set; } = 0;
        /// <summary>
        /// Сила урона для атаки
        /// </summary>
        private float _damage = 1;

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
                        else if (json.IsKey(Cti.Pause)) Pause = json.GetInt();
                        else if (json.IsKey(Cti.Acceleration)) Acceleration = json.GetFloat();
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
        public override string GetToolTip(ItemStack stack)
            => string.Format(_toolTip, 
                Level, _damage,
                stack.ToStringDamaged(),
                Pause, Acceleration);

        /// <summary>
        /// Получить урон для атаки предметом который в руке
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override float GetDamageToAttack() => _damage;

        /// <summary>
        /// Действие предмета ЛКМ.
        /// Для клиента.
        /// </summary>
        /// <param name="preview">Предпросмотр установки, true - без установки блока(ов)</param>
        /// <param name="begin">Первый клик, без паузы</param>
        public override ResultHandAction OnAction(bool preview, bool begin, ItemStack stack, PlayerClientOwner player)
        {
            MovingObjectPosition moving = player.MovingObject;
            if (moving.IsEntity())
            {
                // Урон для сущности
                return new ResultHandAction(ResultHandAction.ActionType.AttackEntity, HitDelay, Pause);
            }
            return base.OnAction(preview, begin, stack, player);
        }
    }
}
