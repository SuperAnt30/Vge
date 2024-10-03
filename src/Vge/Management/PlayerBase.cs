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
        /// Последнее время пинга в милисекундах
        /// </summary>
        protected long lastTimeServer;


        #region Debug

        public Vector2i chPos = new Vector2i(0);

        #endregion

        /// <summary>
        /// Получить время в милисекундах
        /// </summary>
        protected virtual long Time() => 0;

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
            lastTimeServer = Time();
            Ping = (Ping * 3 + (int)(lastTimeServer - time)) / 4;
        }

        /// <summary>
        /// Проверка времени игрока без пинга, если игрок не отвечал больше 30 секунд
        /// </summary>
        public bool TimeOut() => (Time() - lastTimeServer) > 30000;

        #endregion
    }
}
