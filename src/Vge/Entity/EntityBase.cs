using System.Runtime.CompilerServices;
using Vge.Entity.Physics;
using Vge.Entity.Sizes;
using Vge.Entity.MetaData;
using Vge.Renderer.World.Entity;
using Vge.Util;
using Vge.World;
using WinGL.Util;
using Vge.Entity.Render;

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
    public abstract class EntityBase//<TSize> where TSize : ISizeEntity
    {
        /// <summary>
        /// Уникальный порядковый номер игрока в базе
        /// </summary>
        public int Id { get; protected set; }
        /// <summary>
        /// Индекс тип сущности, полученый на сервере из таблицы
        /// </summary>
        public ushort IndexEntity { get; private set; }

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
        /// Позиция этой сущности по оси X с сервера, только для клиента
        /// </summary>
        public float PosServerX;
        /// <summary>
        /// Позиция этой сущности по оси Y с сервера, только для клиента
        /// </summary>
        public float PosServerY;
        /// <summary>
        /// Позиция этой сущности по оси Z с сервера, только для клиента
        /// </summary>
        public float PosServerZ;

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
        /// Объект рендера
        /// </summary>
        public EntityRenderBase Render { get; protected set; }
        /// <summary>
        /// Объект Размер, вес и прочее сущностей которая работает с физикой
        /// </summary>
        public ISizeEntity Size { get; protected set; }

        /// <summary>
        /// Уровень перемещение. Только для сервера 2 - 0 чтоб передвавать клиентам перемещение.
        /// 1 для игроков, 2 для мобов, на один больше, чтоб клиент мог зафиксировать сон, т.е. два последнийх одинаковые
        /// </summary>
        public byte LevelMotionChange;

        /// <summary>
        /// Был ли эта сущность добавлена в чанк, в котором он находится? 
        /// </summary>
        public bool AddedToChunk;

        /// <summary>
        /// На земле
        /// </summary>
        public bool OnGround = false;

        /// <summary>
        /// Объект дополнительных данных
        /// </summary>
        public DataWatcher MetaData { get; protected set; }

        /// <summary>
        /// Спит ли физика, для отладки если Physics == null
        /// </summary>
        private bool _physicSleepDebug;

        #region Init

        /// <summary>
        /// Инициализировать индекс сущности, как игрок
        /// </summary>
        protected void _InitIndexPlayer()
        {
            if (Ce.Entities != null)
            {
                IndexEntity = Ce.Entities.IndexPlayer;
            }
        }

        /// <summary>
        /// Запуск сущности после всех инициализаций, как правило только на сервере
        /// </summary>
        public virtual void InitRun() { }

        /// <summary>
        /// Инициализация для клиента
        /// </summary>
        public void InitRender(ushort index, EntitiesRenderer entitiesRenderer)
        {
            IndexEntity = index;
            Render = new EntityRenderClient(this, entitiesRenderer, IndexEntity);
            _InitMetaData();
            _InitSize();
        }

        /// <summary>
        /// Инициализация для сервера
        /// </summary>
        public void InitServer(ushort index, CollisionBase collision)
        {
            IndexEntity = index;
            Render = new EntityRenderBase(this);
            _InitMetaData();
            _InitSize();
            _InitPhysics(collision);
        }

        /// <summary>
        /// Инициализация физики
        /// </summary>
        protected virtual void _InitPhysics(CollisionBase collision) { }

        /// <summary>
        /// Инициализация размеров сущности
        /// </summary>
        protected virtual void _InitSize() { }

        /// <summary>
        /// Инициализация
        /// </summary>
        protected virtual void _InitMetaData() => MetaData = new DataWatcher(0);

        #endregion

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
        public virtual void UpdatePositionPrev()
        {
            PosPrevX = PosX;
            PosPrevY = PosY;
            PosPrevZ = PosZ;
        }

        /// <summary>
        /// Обновить значения позиций с сервера, только для клиента
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void UpdatePositionServer()
        {
            UpdatePositionPrev();
            PosX = PosServerX;
            PosY = PosServerY;
            PosZ = PosServerZ;
        }
       

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
        /// Возвращает true, если другие Сущности не должны проходить через эту Сущность
        /// </summary>
        public virtual bool CanBeCollidedWith() => false;
        /// <summary>
        /// Возвращает true, если этот объект можно толкать и толкает другие объекты при столкновении
        /// </summary>
        public virtual bool CanBePushed() => true;

        /// <summary>
        /// Получить коэффициент силы импульса к сущности.
        /// Зависит от веса сущностей
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetWeightImpulse(EntityBase entity)
        {
            float weightAnother = entity.Size.GetWeight();
            float weightSum = Size.GetWeight() + weightAnother;
            if (weightSum == 0) return 1f;
            return weightAnother / weightSum;
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
        /// Игровой такт на сервере
        /// </summary>
        public virtual void Update() { }
        /// <summary>
        /// Игровой такт на клиенте
        /// </summary>
        /// <param name="deltaTime">Дельта последнего тика в mc</param>
        public virtual void UpdateClient(WorldClient world, float deltaTime) { }

        /// <summary>
        /// Вызывается, когда быстрая сущность сталкивается с блоком или сущностью.
        /// </summary>
        public virtual void OnImpact(WorldBase world, MovingObjectPosition moving) { }


        #region Get



        #endregion

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
