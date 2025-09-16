using System.Runtime.CompilerServices;
using Vge.Entity.Inventory;
using Vge.Entity.MetaData;
using Vge.Entity.Sizes;
using WinGL.Util;

namespace Vge.Entity
{
    /// <summary>
    /// Объект живой сущности, которая может сама перемещаться и вращаться.
    /// Игроки, мобы
    /// </summary>
    public abstract class EntityLiving : EntityBase//<SizeEntityLiving>
    {
        /// <summary>
        /// Цельная голова с телом
        /// </summary>
        public bool SolidHeadWithBody { get; protected set; } = true;

        #region Rotation 

        /// <summary>
        /// Вращение этой сущности по оси Y в радианах
        /// </summary>
        public float RotationYaw;
        /// <summary>
        /// Вращение этой сущности вверх вниз в радианах
        /// </summary>
        public float RotationPitch;

        /// <summary>
        /// Вращение этой сущности по оси Y в прошлом такте
        /// </summary>
        public float RotationPrevYaw;
        /// <summary>
        /// Вращение этой сущности вверх вниз в прошлом такте
        /// </summary>
        public float RotationPrevPitch;

        /// <summary>
        /// Вращение этой сущности по оси Y с сервера, только для клиента
        /// </summary>
        public float RotationServerYaw;
        /// <summary>
        /// Вращение этой сущности вверх вниз с сервера, только для клиента
        /// </summary>
        public float RotationServerPitch;

        /// <summary>
        /// Вращение тела этой сущности по оси Y в радианах
        /// </summary>
        private float _rotationYawBody;
        /// <summary>
        /// Вращение тела этой сущности по оси Y в прошлом такте
        /// </summary>
        private float _rotationPrevYawBody;

        #endregion

        /// <summary>
        /// Объект Размер, вес и прочее сущностей которая работает с физикой
        /// </summary>
        public SizeEntityLiving SizeLiving { get; protected set; }

        /// <summary>
        /// Объект инвенторя
        /// </summary>
        public InventoryBase Inventory { get; protected set; }

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
            => string.Format("{0:0.000}; {1:0.000}; {2:0.000} Y:{3:0.0} Yb:{4:0.0} P:{5:0.0}",
                PosX, PosY, PosZ, Glm.Degrees(RotationYaw), 
                Glm.Degrees(_rotationYawBody), Glm.Degrees(RotationPitch));

        /// <summary>
        /// Получить угол Yaw тела для кадра
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetRotationFrameYawBody(float timeIndex)
        {
            if (timeIndex >= 1.0f || _rotationPrevYawBody == _rotationYawBody) return _rotationYawBody;
            float biasYaw = _rotationYawBody - _rotationPrevYawBody;
            if (biasYaw > Glm.Pi)
            {
                return _rotationPrevYawBody + (_rotationYawBody - Glm.Pi360 - _rotationPrevYawBody) * timeIndex;
            }
            if (biasYaw < -Glm.Pi)
            {
                return _rotationPrevYawBody + (_rotationYawBody + Glm.Pi360 - _rotationPrevYawBody) * timeIndex;
            }
            return _rotationPrevYawBody + biasYaw * timeIndex;
        }

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
            RotationPitch = RotationServerPitch;
        }

        /// <summary>
        /// Поворот тела от поворота головы или движения.
        /// Для сущностей где голова вращается отдельно от тела.
        /// Запускается ТОЛЬКО на клиенте, где есть Render
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void _RotationBody(bool notCheck = false)
        {
            if (SolidHeadWithBody) _RotationBodyEqualHead();
            else _RotationBodyFromHead(notCheck);
        }

        /// <summary>
        /// Поворот тела равен повороту головы, для сущностей где тело и голова цельная.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _RotationBodyEqualHead()
        {
            if (RotationYaw != _rotationYawBody) _rotationYawBody = RotationYaw;
            if (RotationPrevYaw != _rotationPrevYawBody) _rotationPrevYawBody = RotationPrevYaw;
        }

        /// <summary>
        /// Поворот тела от поворота головы или движения.
        /// Для сущностей где голова вращается отдельно от тела.
        /// </summary>
        private void _RotationBodyFromHead(bool notCheck)
        {
            if (notCheck || PosX != PosPrevX || PosZ != PosPrevZ
                || RotationYaw != RotationPrevYaw || _rotationYawBody != _rotationPrevYawBody)
            {
                _rotationPrevYawBody = _rotationYawBody;
                float yawOffset = _rotationYawBody;

                float xDis = PosX - PosPrevX;
                float zDis = PosZ - PosPrevZ;
                if (xDis * xDis + zDis * zDis > .0025f)
                {
                    // Движение, высчитываем угол направления
                    if (Render.IsMovingStrafe())
                    {
                        // Этот вариант, если тело при твещении стремится к голове, для анимации
                        yawOffset = RotationYaw;
                    }
                    else
                    {
                        // Вариант если надо повернуть тело, как в minecraft
                        yawOffset = Glm.Atan2(zDis, xDis) + Glm.Pi90;
                        // Реверс для бега назад
                        float yawRev = Glm.WrapAngleToPi(yawOffset - _rotationYawBody);
                        if (yawRev < -1.8f) yawOffset += Glm.Pi;
                        else if (yawRev > 1.8f) yawOffset -= Glm.Pi;
                    }
                }

                float yaw = Glm.WrapAngleToPi(yawOffset - _rotationYawBody);
                _rotationYawBody += yaw * .2f;
                yaw = Glm.WrapAngleToPi(RotationYaw - _rotationYawBody);

                if (yaw < -Glm.Pi60) yaw = -Glm.Pi60;
                else if (yaw > Glm.Pi60) yaw = Glm.Pi60;

                _rotationYawBody = RotationYaw - yaw;

                // Смещаем тело если дельта выше 30 градусов, плавным возвращением
                if (yaw > Glm.Pi30 || yaw < -Glm.Pi30) _rotationYawBody += yaw * .03f;

                _rotationYawBody = Glm.WrapAngleToPi(_rotationYawBody);
            }
        }

        /// <summary>
        /// Спавн сущности, первый пакет, вращение
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SpawnRotation(float yaw, float pitch)
        {
            RotationServerYaw = RotationPrevYaw = RotationYaw = yaw;
            RotationServerPitch = RotationPrevPitch = RotationPitch = pitch;
            _RotationBody(true);
        }

        /// <summary>
        /// Задать вращение для сущностей с AI
        /// </summary>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public void SetRotationAI(float yaw, float pitch)
        //{
        //    RotationYaw = yaw;
        //    RotationPitch = pitch;
        //    _RotationBody();
        //}

        #endregion

        /// <summary>
        /// Высота глаз
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual float GetEyeHeight() => SizeLiving.GetEye();

        #region MetaData

        /// <summary>
        /// Инициализация
        /// </summary>
        protected override void _InitMetaData()
        {
            MetaData = new DataWatcher(1);
            // флаги 0-горит; 1-крадется; 2-едет на чем-то; 3-бегает; 4-ест; 5-невидимый
            MetaData.Set(0, (byte)0);
        }

        /// <summary>
        /// Возвращает true, если флаг активен для сущности.Известные флаги:
        /// 0) горит; 1) крадется; 2) едет на чем-то; 3) бегает; 4) ест; 5) наблюдение; 
        /// </summary>
        /// <param name="flag">0) горит; 1) крадется; 2) едет на чем-то; 3) бегает; 4) ест; 5) невидимый</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool GetFlag(int flag) => (MetaData.GetByte(0) & 1 << flag) != 0;

        /// <summary>
        /// Включите или отключите флаг сущности
        /// 0) горит; 1) крадется; 2) едет на чем-то; 3) бегает; 4) ест; 5) наблюдение; 6) спит 7) невидимый по чаре
        /// </summary>
        /// <param name="flag">0) горит; 1) крадется; 2) едет на чем-то; 3) бегает; 4) ест; 5) невидимый</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void SetFlag(int flag, bool set)
        {
            byte var3 = MetaData.GetByte(0);
            if (set) MetaData.UpdateObject(0, (byte)(var3 | 1 << flag));
            else MetaData.UpdateObject(0, (byte)(var3 & ~(1 << flag)));
        }

        /// <summary>
        /// Ускорение
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSprinting() => GetFlag(3);
        /// <summary>
        /// Задать значение ускоряется ли
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetSprinting(bool sprinting) => SetFlag(3, sprinting);

        /// <summary>
        /// Крадется
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSneaking() => GetFlag(1);
        /// <summary>
        /// Задать значение крадется ли
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetSneaking(bool sneaking) => SetFlag(1, sneaking);

        /// <summary>
        /// Горит ли сущность
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool InFire() => GetFlag(0);
        /// <summary>
        /// Задать горит ли сущность
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetInFire(bool fire) => SetFlag(0, fire);

        /// <summary>
        /// Положение стоя
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void Standing() { }

        /// <summary>
        /// Положение сидя
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void Sitting() { }

        #endregion
    }
}
