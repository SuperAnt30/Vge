using System;
using Vge.Entity;

namespace Mvk2.Entity.Damage
{
    /// <summary>
    /// Класс урона для живых сущностей, мобов и игроков
    /// </summary>
    public class DamageLiving : DamageBase
    {
        /// <summary>
        /// Сущность к которой прекреплена физика
        /// </summary>
        protected readonly EntityLiving _entity;

        public DamageLiving(EntityLiving entity)
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
            if (_entity is EntityMob entityMob) entityMob.EntityAge = 0;
            if (_entity.GetHealth() <= 0) return false;

            return base.OnAttack(source, hitY, damage, entityAttack);
        }

        /// <summary>
        /// Атака уже после проверок имунитетов
        /// </summary>
        protected override void _OnAttack(byte source, float damage, EntityLiving entityAttack)
        {
            float health = _entity.GetHealth() - damage;
            _entity.SetHealth(health);

            Console.WriteLine("GetHealth" + _entity.GetHealth());
            // Анимация урона
            //worldServer.Tracker.SendToAllTrackingEntityCurrent(this, new PacketS0BAnimation(Id,
            //        PacketS0BAnimation.EnumAnimation.Hurt, (byte)enumBody));

            if (health <= 0)
            {
                //_entity.SetDead();
                // Звук смерти
                //World.PlaySound(GetDeathSound(), Position, 1, (rand.NextFloat() - rand.NextFloat()) * .2f + 1f);
                //_entity.OnDeath(source);

                //entityPlayerServer.OnDeathPlayerServer(worldServer, source, entityAttacks);
            }
            else
            {
                // Звук урона
                //if (source != EnumDamageSource.OnFire && source != EnumDamageSource.Drown
                //    && source != EnumDamageSource.Fall)
                //{
                //    World.PlaySound(SampleHurt(), Position, 1, (rand.NextFloat() - rand.NextFloat()) * .2f + 1f);
                //}
            }
        }

        /// <summary>
        /// Получить урон от расположения по телу.
        /// 1. Коэффицент в голову или другую чась тиела
        /// 2. Защита от брони
        /// 3. Защита от магии
        /// </summary>
        /// <param name="hitY">если -1 = удар по всему телу, если -2 = удар повсему телу без урона по одежде</param>
        protected override float _GetDamageFromBody(byte source, float hitY, float damage)
        {
            // Определяем куда попали
            EnumBodyDamage enumBody;

            if (hitY == -1) enumBody = EnumBodyDamage.All;
            else if (hitY == -2) enumBody = EnumBodyDamage.Null;
            else
            {
                float height = _entity.Size.GetHeight();

                if (hitY < .1f) enumBody = EnumBodyDamage.Feet; // Стопы
                else if (hitY >= height * .75f)
                {
                    enumBody = EnumBodyDamage.Head; // Голова
                    damage *= 1.25f; // в голову урон на 25% больше
                }
                else if (hitY < height * .375f)
                {
                    enumBody = EnumBodyDamage.Legs; // Ноги
                    damage *= .85f; // в ноги на 15% меньше
                }
                else enumBody = EnumBodyDamage.Body; // Тело
            }

            Console.Write(" " + enumBody);

            return damage;
        }
    }
}
