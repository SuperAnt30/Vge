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
        /// Тип сущности
        /// </summary>
        public EnumEntity Type { get; protected set; }
        /// <summary>
        /// Сущность мертва, не активна
        /// </summary>
        public bool IsDead { get; protected set; } = false;

        #region Переменные для Position

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
        /// Координату X в каком чанке находится перерасчётес с PosX
        /// </summary>
        public int ChunkPositionX => Mth.Floor(PosX) >> 4;
        /// <summary>
        /// Координата Y в каком чанке находится перерасчётес с PosY
        /// </summary>
        public int ChunkPositionY => Mth.Floor(PosY) >> 4;
        /// <summary>
        /// Координата Z в каком чанке находится перерасчётес с PosZ
        /// </summary>
        public int ChunkPositionZ => Mth.Floor(PosZ) >> 4;

        /// <summary>
        /// Координату X в каком чанке находится перерасчётес с PosX
        /// </summary>
        public int ChunkPositionPrevX { get; private set; }
        /// <summary>
        /// Координата Y в каком чанке находится перерасчётес с PosY
        /// </summary>
        public int ChunkPositionPrevY { get; private set; }
        /// <summary>
        /// Координата Z в каком чанке находится перерасчётес с PosZ
        /// </summary>
        public int ChunkPositionPrevZ { get; private set; }

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
        /// Был ли эта сущность добавлена в чанк, в котором он находится? 
        /// </summary>
        public bool AddedToChunk;

        /// <summary>
        /// Пол ширина сущности
        /// </summary>
        public float Width { get; protected set; } = .3f;// .6f;
        /// <summary>
        /// Высота сущности
        /// </summary>
        public float Height { get; protected set; } = 1.8f;//3.6f;
        
        /// <summary>
        /// На земле
        /// </summary>
        public bool OnGround = false;

        /// <summary>
        /// Спит ли физика, для отладки если Physics == null
        /// </summary>
        private bool _physicSleepDebug;

        /// <summary>
        /// Задать индекс
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetEntityId(int id) => Id = id;

        #region Методы для Position Chunk

        /// <summary>
        /// Получить координаты чанка
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2i GetChunkPosition() => new Vector2i(ChunkPositionX, ChunkPositionZ);

        /// <summary>
        /// Задать координаты чанка
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetPositionChunk(int x, int y, int z)
        {
            ChunkPositionPrevX = x;
            ChunkPositionPrevY = y;
            ChunkPositionPrevZ = z;
        }

        #endregion

        #region Методы для Position и Rotation

        /// <summary>
        /// Получить вектор позиции
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 GetPositionVec() => new Vector3(PosX, PosY, PosZ);

        /// <summary>
        /// Изменена ли позиция
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsPositionChange()
            => PosX != PosPrevX || PosY != PosPrevY || PosZ != PosPrevZ;

        /// <summary>
        /// Вернуть строку расположения
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToStringPosition()
            => string.Format("{0:0.00}; {1:0.00}; {2:0.00}", PosX, PosY, PosZ);

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
        }

        #endregion

        #region AxisAlignedBB

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

        /// <summary>
        /// Возвращает, пересекается ли данная ограничивающая рамка с этой сущностью
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IntersectsWith(AxisAlignedBB other) => GetBoundingBox().IntersectsWith(other);

        #endregion

        #region Physic

        /// <summary>
        /// Пробудить физику
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AwakenPhysicSleep()
        {
            if (Physics != null)
            {
                Physics.AwakenPhysics();
            }
        }

        /// <summary>
        /// Пробудить физику выбранной сущности.
        /// Текущаяя сущность выступает в роли заказчика физики,
        /// так-как она может быть на сервере или на клиенте у игроков
        /// </summary>
        public void AwakenPhysicSleep(EntityBase entity)
        {
            entity.AwakenPhysicSleep();
            _SetAwakenPhysicSleep(entity.Id);
        }

        /// <summary>
        /// Задать выбранной сущности импульс
        /// </summary>
        protected virtual void _SetAwakenPhysicSleep(int id) { }

        /// <summary>
        /// Спит ли физика, если физики нет, не спит (false)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsPhysicSleep() 
            => Physics != null ? Physics.IsPhysicSleep() : false;

        /// <summary>
        /// Спит ли физика для отладки
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsPhysicSleepDebug()
            => Physics != null ? Physics.IsPhysicSleep() : _physicSleepDebug;

        /// <summary>
        /// Сон физики для отладки
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetPhysicsSleepDebug(bool sleep) => _physicSleepDebug = sleep;

        /// <summary>
        /// Задать импульс
        /// </summary>
        public virtual void SetPhysicsImpulse(float x, float y, float z)
        {
            if (Physics != null)
            {
                Physics.ImpulseX += x;
                Physics.ImpulseY += y;
                Physics.ImpulseZ += z;

                if  (Physics.IsPhysicSleep()) Physics.AwakenPhysics();
            }
        }

        /// <summary>
        /// Задать выбранной сущности импульс
        /// </summary>
        protected virtual void _SetEntityPhysicsImpulse(int id, float x, float y, float z) { }

        /// <summary>
        /// Задать импулс выбранной сущности.
        /// Текущаяя сущность выступает в роли заказчика физики,
        /// так-как она может быть на сервере или на клиенте у игроков
        /// </summary>
        public void SetEntityPhysicsImpulse(EntityBase entity, float x, float y, float z)
        {
            entity.SetPhysicsImpulse(x, y, z);
            _SetEntityPhysicsImpulse(entity.Id, x, y, z);
        }

        /// <summary>
        /// Применяет скорость к каждому из объектов, отталкивая их друг от друга. На сервере!
        /// </summary>
        public void ApplyEntityCollision(EntityBase entityIn)
        {
            if (!entityIn.NoClip() && !NoClip())
            {
                float x = entityIn.PosX - PosX;
                float z = entityIn.PosZ - PosZ;

                float k = Mth.Max(Mth.Abs(x), Mth.Abs(z));

                if (k >= 0.00999999f)
                {
                    k = Mth.Sqrt(k);
                    x /= k;
                    z /= k;
                    float k2 = 1f / k;
                    if (k2 > 1f) k2 = 1f;

                    x *= k2;
                    z *= k2;
                    x *= .05f;
                    z *= .05f;

                    SetPhysicsImpulse(-x, 0, -z);
                    SetEntityPhysicsImpulse(entityIn, x, 0, z);
                }
            }
        }

        /// <summary>
        /// Будет ли эта сущность проходить сквозь блоки
        /// </summary>
        public virtual bool NoClip() => false;
        /// <summary>
        /// Возвращает true, если этот объект можно толкать и толкает другие объекты при столкновении
        /// </summary>
        public virtual bool CanBePushed() => true;
        /// <summary>
        /// Вес сущности для определения импулса между сущностями,
        /// У кого больше вес тот больше толкает или меньше потдаётся импульсу.
        /// </summary>
        public virtual float GetWeight() => 0;
        /// <summary>
        /// Получить коэффициент силы импульса к сущности
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetWeightImpulse(EntityBase entity)
        {
            float ew = entity.GetWeight();
            float f = GetWeight() + ew;
            if (f == 0) return 1f;
            return ew / f;
        }

        #endregion

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
        //public Vector3[] ToVector3Array(float motionX, float motionY, float motionZ)
        //{
        //    AxisAlignedBB axis = new AxisAlignedBB(-Width, 0, -Width, Width, Height, Width);
        //    Vector3[] vectors = axis.ToVector3Array();
        //    motionX += PosX;
        //    motionY += PosY;
        //    motionZ += PosZ;
        //    Vector3 vec;
        //    for (int i = 0; i < 8; i++)
        //    {
        //        vec = Glm.Rotate(vectors[i], RotationYaw, new Vector3(0, -1, 0));
        //        vectors[i].X = vec.X + motionX;
        //        vectors[i].Y = vec.Y + motionY;
        //        vectors[i].Z = vec.Z + motionZ;
        //    }
        //    return vectors;
        //}
    }
}
