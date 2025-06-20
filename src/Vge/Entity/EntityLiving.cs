﻿using System.Runtime.CompilerServices;
using WinGL.Util;

namespace Vge.Entity
{
    /// <summary>
    /// Объект живой сущности, которая может сама перемещаться и вращаться.
    /// Игроки, мобы
    /// </summary>
    public abstract class EntityLiving : EntityBase
    {

        /// <summary>
        /// Объект Размер, вес и прочее сущностей которая работает с физикой
        /// </summary>
        public SizeEntityLiving SizeLiving { get; protected set; }


        #region Rotation 

        /// <summary>
        /// Вращение этой сущности по оси Y в радианах
        /// </summary>
        public float RotationYaw;
        /// <summary>
        /// Вращение головы этой сущности по оси Y в радианах (если голова имеется, если нет, игнорируем)
        /// </summary>
        public float RotationYawHead;
        /// <summary>
        /// Вращение этой сущности вверх вниз в радианах
        /// </summary>
        public float RotationPitch;

        /// <summary>
        /// Вращение этой сущности по оси Y в прошлом такте
        /// </summary>
        public float RotationPrevYaw;
        /// <summary>
        /// Вращение этой сущности по оси Y в прошлом такте (если голова имеется, если нет, игнорируем)
        /// </summary>
        public float RotationPrevYawHead;
        /// <summary>
        /// Вращение этой сущности вверх вниз в прошлом такте
        /// </summary>
        public float RotationPrevPitch;

        /// <summary>
        /// Вращение этой сущности по оси Y с сервера, только для клиента
        /// </summary>
        public float RotationServerYaw;
        /// <summary>
        /// Вращение этой сущности по оси Y с сервера, только для клиента (если голова имеется, если нет, игнорируем)
        /// </summary>
        public float RotationServerYawHead;
        /// <summary>
        /// Вращение этой сущности вверх вниз с сервера, только для клиента
        /// </summary>
        public float RotationServerPitch;

        #endregion

        #region Методы для Position и Rotation

        /// <summary>
        /// Изменено ли вращение
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsRotationChange()
            => RotationYaw != RotationPrevYaw || RotationPitch != RotationPrevPitch;

        /// <summary>
        /// Вернуть строку расположения и вращения
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToStringPositionRotation()
            => string.Format("{0:0.00}; {1:0.00}; {2:0.00} Y:{3:0.00} P:{4:0.00}",
                PosX, PosY, PosZ, Glm.Degrees(RotationYaw), Glm.Degrees(RotationPitch));

        /// <summary>
        /// Получить угол Yaw для кадра
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetRotationFrameYaw(float timeIndex)
        {
            if (timeIndex >= 1.0f || RotationPrevYaw == RotationYaw) return RotationYaw;
            float biasYaw = RotationYaw - RotationPrevYaw;
            if (biasYaw > Glm.Pi)
            {
                return RotationPrevYaw + (RotationYaw - Glm.Pi360 - RotationPrevYaw) * timeIndex;
            }
            if (biasYaw < -Glm.Pi)
            {
                return RotationPrevYaw + (RotationYaw + Glm.Pi360 - RotationPrevYaw) * timeIndex;
            }
            return RotationPrevYaw + biasYaw * timeIndex;
        }

        /// <summary>
        /// Получить угол Pitch для кадра
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetRotationFramePitch(float timeIndex)
        {
            if (timeIndex >= 1.0f || RotationPrevPitch == RotationPitch) return RotationPitch;
            return RotationPrevPitch + (RotationPitch - RotationPrevPitch) * timeIndex;
        }

        /// <summary>
        /// Обновить значения Prev
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void UpdatePositionPrev()
        {
            PosPrevX = PosX;
            PosPrevY = PosY;
            PosPrevZ = PosZ;
            RotationPrevYaw = RotationYaw;
            RotationPrevYawHead = RotationYawHead;
            RotationPrevPitch = RotationPitch;
        }

        /// <summary>
        /// Обновить значения позиций с сервера, только для клиента
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void UpdatePositionServer()
        {
            UpdatePositionPrev();
            PosX = PosServerX;
            PosY = PosServerY;
            PosZ = PosServerZ;
            RotationYaw = RotationServerYaw;
            RotationYawHead = RotationServerYawHead;
            RotationPitch = RotationServerPitch;
        }

        #endregion
    }
}
