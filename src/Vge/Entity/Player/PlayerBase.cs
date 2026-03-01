using System.Runtime.CompilerServices;
using Vge.Entity.Sizes;
using Vge.World;

namespace Vge.Entity.Player
{
    /// <summary>
    /// Абстрактный класс игрока
    /// </summary>
    public abstract class PlayerBase : EntityLiving
    {
        /// <summary>
        /// Псевдоним игрока
        /// </summary>
        public string Login { get; protected set; }
        /// <summary>
        /// Хэш пароль игрока
        /// </summary>
        public string Token { get; protected set; }
        /// <summary>
        /// Хэш игрока по псевдониму
        /// </summary>
        public string UUID { get; protected set; }
        /// <summary>
        /// Пинг клиента в мс
        /// </summary>
        public int Ping { get; protected set; } = -1;

        /// <summary>
        /// Обзор сколько видит якорь чанков вокруг себя
        /// </summary>
        public byte OverviewChunk { get; private set; } = 1;
        /// <summary>
        /// Обзор чанков прошлого такта
        /// </summary>
        public byte OverviewChunkPrev { get; private set; } = 1;
        /// <summary>
        /// Индекс мира, где находится игрок
        /// </summary>
        public byte IdWorld { get; protected set; } = 0;

        /// <summary>
        /// Творческий режим, бесконечный инвентарь, и всё ломаем за один клик
        /// </summary>
        public bool CreativeMode { get; protected set; } = false;

        /// <summary>
        /// Последнее время пинга в милисекундах
        /// </summary>
        protected long _lastTimeServer;

        #region MetaData

        /// <summary>
        /// Наблюдение
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSpectator() => GetFlag(5);
        /// <summary>
        /// Задать наблюдение сущность
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetSpectator(bool invisible) => SetFlag(5, invisible);

        #endregion

        #region Overview

        /// <summary>
        /// Обновить обзор прошлого такта
        /// </summary>
        protected void _UpOverviewChunkPrev() => OverviewChunkPrev = OverviewChunk;

        /// <summary>
        /// Задать обзор чанков у клиента
        /// </summary>
        public virtual void SetOverviewChunk(byte overviewChunk) => OverviewChunk = overviewChunk;

        #endregion

        /// <summary>
        /// Получить время в милисекундах
        /// </summary>
        protected virtual long _Time() => 0;

        /// <summary>
        /// Игрок на сервере, получить данные
        /// </summary>
        public void PlayerOnTheServer(int id, string uuid)
        {
            Id = id;
            UUID = uuid;
        }

        #region Ping

        /// <summary>
        /// Задать время пинга
        /// </summary>
        public void SetPing(long time)
        {
            _lastTimeServer = _Time();
            Ping = (Ping * 3 + (int)(_lastTimeServer - time)) / 4;
        }

        /// <summary>
        /// Проверка времени игрока без пинга, если игрок не отвечал больше 30 секунд
        /// </summary>
        public bool TimeOut() => (_Time() - _lastTimeServer) > 30000;

        #endregion

        /// <summary>
        /// Получить название для рендеринга
        /// </summary>
        public override string GetName() => Login;

        /// <summary>
        /// Возвращает true, если другие Сущности не должны проходить через эту Сущность
        /// </summary>
        public override bool CanBeCollidedWith() => true;

        /// <summary>
        /// Инициализация размеров сущности
        /// </summary>
        protected override void _InitSize() => Standing();

        /// <summary>
        /// Положение стоя
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Standing() // 60 единиц в Blocknench
                                        //=> Size = SizeLiving = new SizeEntityLiving(this, .3f, 1.8f, 1.68f, 80);
            => Size = SizeLiving = new SizeEntityLiving(this, .6f, 3.6f, 3.36f, 80); // 50см 59 единиц в Blocknench

        /// <summary>
        /// Положение сидя
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Sitting() // 50 единиц в Blocknench согласно интерполяции
                                       //=> Size = SizeLiving = new SizeEntityLiving(this, .3f, 1.49f, 1.38f, 80);
            => Size = SizeLiving = new SizeEntityLiving(this, .6f, 2.98f, 2.76f, 80); // 50 см 

        /// <summary>
        /// Получить объект мира
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual WorldBase GetWorld() => null;
    }
}
