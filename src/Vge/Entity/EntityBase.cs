using System.Runtime.CompilerServices;
using Vge.Util;
using WinGL.Util;

namespace Vge.Entity
{
    /// <summary>
    /// Базовый объект сущности
    /// </summary>
    public abstract class EntityBase
    {
        /// <summary>
        /// Уникальный порядковый номер игрока
        /// </summary>
        public int Id { get; protected set; }

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
        /// Вращение этой сущности по оси Y
        /// </summary>
        public float RotationYaw;
        /// <summary>
        /// Вращение этой сущности вверх вниз
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
        public bool OnGround = true;

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
        /// Получить угол Yaw для кадра
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetRotationFrameYaw(float timeIndex)
        {
            if (timeIndex >= 1.0f || RotationPrevYaw == RotationYaw) return RotationYaw;
            return RotationPrevYaw + (RotationYaw - RotationPrevYaw) * timeIndex;
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
    }
}
