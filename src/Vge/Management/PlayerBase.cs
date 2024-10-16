using WinGL.Util;

namespace Vge.Management
{
    /// <summary>
    /// Абстрактный класс игроков
    /// </summary>
    public abstract class PlayerBase
    {
        /// <summary>
        /// Уникальный порядковый номер игрока
        /// </summary>
        public int Id { get; protected set; }
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
        /// Последнее время пинга в милисекундах
        /// </summary>
        protected long _lastTimeServer;


        #region Debug

        public Vector2i chPos = new Vector2i(0);
        public byte idWorld = 1;
        public bool isPos = false;

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
        /// Игровой такт
        /// </summary>
        public virtual void Update() { }
    }
}
