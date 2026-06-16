using Mvk2.Entity.List;
using System;
using Vge.Entity;

namespace Mvk2.Entity.Damage
{
    /// <summary>
    /// Класс урона для живых сущностей, мобов и игроков
    /// </summary>
    public class DamageItem : DamageBase
    {
        /// <summary>
        /// Состояние (Например, урон для инструментов) 
        /// </summary>
        private float _health = 5;

        /// <summary>
        /// Сущность к которой прекреплена физика
        /// </summary>
        protected readonly EntityItemMvk _entity;

        public DamageItem(EntityItemMvk entity)
        {
            _entity = entity;
        }

        /// <summary>
        /// Сущность получает урон, только на сервере.
        /// hitY = -1 удар по всему телу, 
        /// hitY = -2 = удар повсему телу без урона по одежде
        /// </summary>
        /// <returns>true - урон был нанесён</returns>
        public override bool OnAttack(byte source, float hitY, float damage,
            EntityLiving entityAttack = null)
        {
            if (_health <= 0) return false;

            return base.OnAttack(source, hitY, damage, entityAttack);
        }

        /// <summary>
        /// Атака уже после проверок имунитетов
        /// </summary>
        protected override void _OnAttack(byte source, float damage, EntityLiving entityAttack)
        {
            _health -= damage;

            if (_health <= 0)
            {
                _entity.SetDead();
            }
        }
    }
}
