using System.Runtime.CompilerServices;
using Vge.Util;
using WinGL.Util;

namespace Vge.Entity
{
    /**
     * 23.01.2025
     * float на позициях. Ограничивает перемещение в -25000 .. 25000
     * если шире, могут быть различные артефакты. Первый из них проходы по колизиям,
     * и падения с блока при шифте
     */
    /// <summary>
    /// Базовый объект сущности
    /// </summary>
    public abstract class EntityBase
    {
        /// <summary>
        /// Уникальный порядковый номер игрока
        /// </summary>
        public int Id { get; protected set; }
        /// <summary>
        /// Сущность мертва, не активна
        /// </summary>
        public bool IsDead { get; protected set; } = false;

        #region Переменные для Position и Rotation

        #region Position 

        /// <summary>
        /// Позиция этой сущности по оси X
        /// </summary>
        public float PosX;
        /// <summary>
        /// Позиция этой сущности по оси Y
        /// </summary>
        public float PosY;
        /// <summary>
        /// Позиция этой сущности по оси Z
        /// </summary>
        public float PosZ;

        /// <summary>
        /// Вращение этой сущности по оси Y в радианах
        /// </summary>
        public float RotationYaw;
        /// <summary>
        /// Вращение этой сущности вверх вниз в радианах
        /// </summary>
        public float RotationPitch;

        #endregion

        #region PositionPrev

        /// <summary>
        /// Позиция этой сущности по оси X в прошлом такте
        /// </summary>
        public float PosPrevX;
        /// <summary>
        /// Позиция этой сущности по оси Y в прошлом такте
        /// </summary>
        public float PosPrevY;
        /// <summary>
        /// Позиция этой сущности по оси Z в прошлом такте
        /// </summary>
        public float PosPrevZ;

        /// <summary>
        /// Вращение этой сущности по оси Y в прошлом такте
        /// </summary>
        public float RotationPrevYaw;
        /// <summary>
        /// Вращение этой сущности вверх вниз в прошлом такте
        /// </summary>
        public float RotationPrevPitch;

        #endregion

        /// <summary>
        /// Координату X в каком чанке находится
        /// </summary>
        public int ChunkPositionX => Mth.Floor(PosX) >> 4;
        /// <summary>
        /// Координата Z в каком чанке находится
        /// </summary>
        public int ChunkPositionZ => Mth.Floor(PosZ) >> 4;
        /// <summary>
        /// Координата Y
        /// </summary>
        public int PositionY => Mth.Floor(PosY);

        #endregion

        /// <summary>
        /// Объект физики
        /// </summary>
        public PhysicsBase Physics { get; protected set; }
        
        /// <summary>
        /// Уровень перемещение. Для сервера 1 и 0 чтоб передвавать клиентам перемещение.
        /// Для клиента 2 - 0, чтоб минимизировать запросы.
        /// </summary>
        public byte LevelMotionChange;

        /// <summary>
        /// Пол ширина сущности
        /// </summary>
        public float Width { get; protected set; } = .3f;// .6f;
        /// <summary>
        /// Высота сущности
        /// </summary>
        public float Height { get; protected set; } = 1.8f;//3.6f;
        /// <summary>
        /// Высота глаз
        /// </summary>
        public float Eye { get; protected set; }

        /// <summary>
        /// На земле
        /// </summary>
        public bool OnGround = false;

        /// <summary>
        /// Задать индекс
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetEntityId(int id) => Id = id;

        #region Методы для Position и Rotation

        /// <summary>
        /// Получить вектор позиции
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 GetPositionVec() => new Vector3(PosX, PosY, PosZ);

        /// <summary>
        /// Получить координаты чанка
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2i GetChunkPosition() => new Vector2i(ChunkPositionX, ChunkPositionZ);

        /// <summary>
        /// Изменена ли позиция
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsPositionChange()
            => PosX != PosPrevX || PosY != PosPrevY || PosZ != PosPrevZ;

        /// <summary>
        /// Изменено ли вращение
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsRotationChange()
            => RotationYaw != RotationPrevYaw || RotationPitch != RotationPrevPitch;

        /// <summary>
        /// Вернуть строку расположения
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToStringPosition()
            => string.Format("{0:0.00}; {1:0.00}; {2:0.00}", PosX, PosY, PosZ);

        /// <summary>
        /// Вернуть строку расположения и вращения
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToStringPositionRotation()
            => string.Format("{0:0.00}; {1:0.00}; {2:0.00} Y:{3:0.00} P:{4:0.00}",
                PosX, PosY, PosZ, Glm.Degrees(RotationYaw), Glm.Degrees(RotationPitch));

        /// <summary>
        /// Получить позицию X для кадра
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetPosFrameX(float timeIndex)
        {
            if (timeIndex >= 1.0f || PosPrevX == PosX) return PosX;
            return PosPrevX + (PosX - PosPrevX) * timeIndex;
        }

        /// <summary>
        /// Получить позицию Y для кадра
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetPosFrameY(float timeIndex)
        {
            if (timeIndex >= 1.0f || PosPrevY == PosY) return PosY;
            return PosPrevY + (PosY - PosPrevY) * timeIndex;
        }

        /// <summary>
        /// Получить позицию Z для кадра
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetPosFrameZ(float timeIndex)
        {
            if (timeIndex >= 1.0f || PosPrevZ == PosZ) return PosZ;
            return PosPrevZ + (PosZ - PosPrevZ) * timeIndex;
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
        /// Получить растояние до сущности
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Distance(EntityBase entity)
        {
            float x = PosX - entity.PosX;
            float y = PosY - entity.PosY;
            float z = PosZ - entity.PosZ;
            return Mth.Sqrt(x * x + y * y + z * z);
        }

        /// <summary>
        /// Получить растояние до сущности
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Distance(Vector3 vec)
        {
            float x = PosX - vec.X;
            float y = PosY - vec.Y;
            float z = PosZ - vec.Z;
            return Mth.Sqrt(x * x + y * y + z * z);
        }

        /// <summary>
        /// Проверка расстояния в кубаче, без Sqrt
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsDistanceCube(float posX, float posY, float posZ, float length)
        {
            float x = PosX - posX;
            float y = PosY - posY;
            float z = PosZ - posZ;
            return x > length ||x < -length || y > length || y < -length || z > length || z < -length;
        }

        /// <summary>
        /// Обновить значения Prev
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void UpdatePrev()
        {
            PosPrevX = PosX;
            PosPrevY = PosY;
            PosPrevZ = PosZ;
            RotationPrevYaw = RotationYaw;
            RotationPrevPitch = RotationPitch;
        }

        #endregion

        /// <summary>
        /// Получить ограничительную рамку сущности
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AxisAlignedBB GetBoundingBox()
            => new AxisAlignedBB(PosX - Width, PosY, PosZ - Width, PosX + Width, PosY + Height, PosZ + Width);

        /// <summary>
        /// Получить ограничительную рамку на выбранной позиции
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AxisAlignedBB GetBoundingBox(float x, float y, float z) 
            => new AxisAlignedBB(x - Width, y, z - Width, x + Width, y + Height, z + Width);

        #region Tracker

        /// <summary>
        /// Была ли смена обзора чанков, для трекера
        /// </summary>
        public virtual bool IsOverviewChunkChanged() => false;

        /// <summary>
        /// Сделана смена обзора чанков, для трекера
        /// </summary>
        public virtual void MadeOverviewChunkChanged() { }

        #endregion

        /// <summary>
        /// Будет уничтожен следующим тиком
        /// </summary>
        public virtual void SetDead() => IsDead = true;
        /// <summary>
        /// Получить название для рендеринга
        /// </summary>
        public virtual string GetName() => "";

        /// <summary>
        /// Игровой такт
        /// </summary>
        public virtual void Update() { }

        /// <summary>
        /// Получить массив XZ с вращением
        /// Не исползуется, для примера будущей детальной коллизии для урона
        /// </summary>
        public Vector3[] ToVector3Array(float motionX, float motionY, float motionZ)
        {
            AxisAlignedBB axis = new AxisAlignedBB(-Width, 0, -Width, Width, Height, Width);
            Vector3[] vectors = axis.ToVector3Array();
            motionX += PosX;
            motionY += PosY;
            motionZ += PosZ;
            Vector3 vec;
            for (int i = 0; i < 8; i++)
            {
                vec = Glm.Rotate(vectors[i], RotationYaw, new Vector3(0, -1, 0));
                vectors[i].X = vec.X + motionX;
                vectors[i].Y = vec.Y + motionY;
                vectors[i].Z = vec.Z + motionZ;
            }
            return vectors;
        }
    }
}
