using System;
using System.Runtime.CompilerServices;

namespace Vge.Entity
{
    /// <summary>
    /// Абстрактный класс урона сущности
    /// </summary>
    public abstract class DamageBase
    {
        ///// <summary>
        ///// Сущность к которой прекреплена физика
        ///// </summary>
        //private readonly EntityBase _entity;

        //public DamageBase(EntityBase entity)
        //{
        //    _entity = entity;
        //}

        /// <summary>
        /// Имеется ли у сущности иммунитет от горения в огне и лаве
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual bool _IsImmuneToFire() => false;
        /// <summary>
        /// Имеется ли у сущности иммунитет к отсутствии воздуха
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual bool _IsImmuneToLackOfAir() => false;
        /// <summary>
        /// Имеется ли у сущности иммунитет к падению
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual bool _IsImmuneToFall() => false;
        /// <summary>
        /// Имеется ли у сущности иммунитет на всё
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual bool _IsImmuneToAll() => false;

        /// <summary>
        /// Сущность получает урон, только на сервере.
        /// hitY = -1 удар по всему телу, 
        /// hitY = -2 = удар повсему телу без урона по одежде
        /// </summary>
        /// <returns>true - урон был нанесён</returns>
        public virtual bool OnAttack(byte source, float hitY, float damage,
            EntityLiving entityAttack = null)
        {
            Console.Write("OnAttack " + damage);
            if (_IsImmuneToAll() && source != (byte)EnumDamageSource.OutOfWorld) return false;
            if (_IsImmuneToFall() && source == (byte)EnumDamageSource.Fall) return false;
            if (_IsImmuneToFire() && (source == (byte)EnumDamageSource.InFire
                || source == (byte)EnumDamageSource.OnFire
                || source == (byte)EnumDamageSource.Lava)) return false;
            if (_IsImmuneToLackOfAir() && source == (byte)EnumDamageSource.Drown) return false;
            damage = _GetDamageFromBody(source, hitY, damage);
            Console.WriteLine(" => " + damage);
            if (damage > 0)
            {
                _OnAttack(source, damage, entityAttack);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Атака уже после проверок имунитетов
        /// </summary>
        /// <returns>true - урон был нанесён</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void _OnAttack(byte source, float damage, EntityLiving entityAttack) { }

        /// <summary>
        /// Получить урон от расположения по телу.
        /// 1. Коэффицент в голову или другую чась тиела
        /// 2. Защита от брони
        /// 3. Защита от магии
        /// </summary>
        /// <param name="hitY">если -1 = удар по всему телу, если -2 = удар повсему телу без урона по одежде</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual float _GetDamageFromBody(byte source, float hitY, float damage) => damage;
    }
}
