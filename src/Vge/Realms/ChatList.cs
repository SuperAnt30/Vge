using System;
using System.Collections.Generic;
using Vge.Renderer.Font;

namespace Vge.Realms
{
    /// <summary>
    /// Объект хранения списков для чата
    /// </summary>
    public class ChatList
    {
        /// <summary>
        /// Все строки чата разбитые под контейнер чата
        /// </summary>
        public readonly List<string> Messages = new List<string>();
        /// <summary>
        /// Список сообщений, ранее отправленных через графический интерфейс чата
        /// </summary>
        public readonly List<string> SentMessages = new List<string>();
        /// <summary>
        /// Строки чата, которые будут отображаться в окне чата
        /// </summary>
        public readonly List<ChatLine> ChatLines = new List<ChatLine>();
        /// <summary>
        /// Надо ли обновить
        /// </summary>
        public bool FlagUpdate { get; private set; }
        /// <summary>
        /// Шрифт чата, для корректного разбиение на строки
        /// </summary>
        public readonly FontBase Font;

        /// <summary>
        /// Время жизни сообщения в игровых тиках
        /// </summary>
        private readonly int _timeLife;
        /// <summary>
        /// Нужен свой счётчик времени, чтоб чат не слетал если на сервере была корректировка по времени
        /// </summary>
        private uint _tickCounter = 0;

        public ChatList(int timeLife, FontBase font)
        {
            _timeLife = timeLife;
            Font = font;
        }

        public void OnTick()
        {
            _tickCounter++;
            if (ChatLines.Count > 0 && _tickCounter - ChatLines[0].TimeCreated > _timeLife)
            {
                ChatLines.RemoveAt(0);
                FlagUpdate = true;
            }
        }

        /// <summary>
        ///  Для истории написания, чтоб можно было повторно повторить текст
        /// </summary>
        public void AddMessageHistory(string message)
        {
            int count = SentMessages.Count;
            if (count == 0 || !SentMessages[count - 1].Equals(message))
            {
                SentMessages.Add(message);
            }
        }

        /// <summary>
        /// Занести в массив сообщение
        /// </summary>
        public void AddMessage(string message, int width, int si)
        {
            if (message != "")
            {
                message = ChatStyle.Reset + message;
                // Занести в массив чата
                ChatLines.Add(new ChatLine(message, _tickCounter));
                TransferText transfer = Font.Transfer;
                transfer.Run(message, width, si);
                int nl = transfer.NumberLines;
                if (nl == 1)
                {
                    Messages.Add(message);
                }
                else
                {
                    string[] strs = transfer.OutText.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                  //  FontRenderer.ResetFont();
                    message = "";
                    string code = "";
                    for (int i = 0; i < nl; i++)
                    {
                        message = strs[i];
                        if (message != "")
                        {
                            Messages.Add(code + message);
                      //      code = FontRenderer.MessageToCodeFont(message);
                        }
                    }
                }
                FlagUpdate = true;
            }
        }

        /// <summary>
        /// Изменить флаг на сделано
        /// </summary>
        public void Update() => FlagUpdate = false;
    }
}
