using System.Runtime.CompilerServices;
using Vge.Entity.Sizes;

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
        /// Последнее время пинга в милисекундах
        /// </summary>
        protected long _lastTimeServer;


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
        protected override void _InitSize()
            => Size = SizeLiving = new SizeEntityLiving(this, .3f, 1.8f, 1.53f, 80);


        /// <summary>
        /// Положение стоя
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Standing()
            => Size = SizeLiving = new SizeEntityLiving(this, .3f, 1.8f, 1.53f, 80);

        /// <summary>
        /// Положение сидя
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Sitting()
            => Size = SizeLiving = new SizeEntityLiving(this, .3f, 1.49f, 1.23f, 80);

    }
}
