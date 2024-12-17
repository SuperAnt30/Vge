namespace Vge.Realms
{
    /// <summary>
    /// Структура для строки чата, для понимания когда была создана
    /// </summary>
    public struct ChatLine
    {
        /// <summary>
        /// Время создания
        /// </summary>
        public uint timeCreated;
        /// <summary>
        /// сообщение
        /// </summary>
        public string message;

        public ChatLine(string message, uint timeCreated)
        {
            this.message = message;
            this.timeCreated = timeCreated;
        }

        public override string ToString() => timeCreated.ToString() + "; " + message;
    }
}
