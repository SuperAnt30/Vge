using Mvk2.Entity.List;
using System;
using Vge.Entity;
using Vge.Entity.Particle;
using WinGL.Util;

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
            int count;

            if (_health <= 0)
            {
                _entity.SetDead();
                count = 25;
            }
            else
            {
                count = 5;
            }

            // Анимация урона
            _entity.GetWorldServer().SpawnParticle(EntitiesFXReg.PartColorId, count,
                _entity.GetPositionCenterVec(), new Vector3(.5f), 1, 0x1000000);
        }
    }
}
