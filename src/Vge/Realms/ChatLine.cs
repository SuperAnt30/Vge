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
        public readonly uint TimeCreated;
        /// <summary>
        /// сообщение
        /// </summary>
        public readonly string Message;

        public ChatLine(string message, uint timeCreated)
        {
            Message = message;
            TimeCreated = timeCreated;
        }

        public override string ToString() => TimeCreated.ToString() + "; " + Message;
    }
}
