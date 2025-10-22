using System;
using System.Collections.Generic;
using Vge.Gui.Controls;
using Vge.Network.Packets.Client;
using Vge.Renderer.Font;
using WinGL.Actions;
using WinGL.Util;

namespace Vge.Gui.Screens
{
    /// <summary>
    /// Скрин окна чата
    /// </summary>
    public class ScreenChat : ScreenWindow
    {
        /// <summary>
        /// Максимальное количество строк в чате
        /// </summary>
        protected int _countMaxLine = 16;

        /// <summary>
        /// Флаг дублирования клавиши с WM_KEYDOWN и WM_CHAR
        /// Чат открывается по WM_KEYDOWN и сразу же пишет букву
        /// </summary>
        private bool _flagDuplication = true;
        /// <summary>
        /// Строка списка сообщений
        /// </summary>
        private Label _labelMessages;
        /// <summary>
        /// Массив игроков
        /// </summary>
        private Label[] _labelplayer;

        /// <summary>
        /// Контрол написания текста
        /// </summary>
        private readonly TextBox _textBoxMessage;

        /// <summary>
        /// Сохраняет положение того, какое сообщение чата вы выберете при нажатии вверх
        /// (не увеличивается для дублированных сообщений, отправленных сразу после друг друга)
        /// </summary>
        private int _sentHistoryCursor = -1;
        /// <summary>
        /// Позиция скролинга текста
        /// </summary>
        private int _scrollPos = 0;
        private string _historyBuffer = "";

        public ScreenChat(WindowMain window, int width, int height) : base(window, 512f, width, height, true)
        {
            FontBase font = window.Render.FontMain;
            _labelMessages = new Label(window, 
                window.Game.Player.Chat.Font, "", true).SetTextAlight(EnumAlight.Left, EnumAlightVert.Bottom);
            _labelMessages.SetSize(Gi.WindowsChatWidthMessage, Gi.WindowsChatHeightMessage);
            _labelMessages.Multiline();
            _textBoxMessage = new TextBox(window, window.Render.FontMain, WidthWindow - 64, "", 
                TextBox.EnumRestrictions.All, 255);
            _textBoxMessage.FixFocus();

            _sentHistoryCursor = window.Game.Player.Chat.SentMessages.Count;

            _labelplayer = new Label[window.Game.Players.Count];
            int i = 0;
            foreach (KeyValuePair<int, string> entry in window.Game.Players)
            {
                _labelplayer[i] = new Label(window, font, 160, 40, entry.Value);
                _labelplayer[i++].Click += _ScreenChat_Click;
            }

            _PageUpdate();

            window.Game.Hud.ChatOn();
        }

        private void _ScreenChat_Click(object sender, EventArgs e)
            => _textBoxMessage.SetText(((Label)sender).Text + ": " + _textBoxMessage.Text);

        /// <summary>
        /// Игровой такт
        /// </summary>
        /// <param name="deltaTime">Дельта последнего тика в mc</param>
        public override void OnTick(float deltaTime)
        {
            base.OnTick(deltaTime);
            if (window.Game.Player.Chat.FlagUpdate)
            {
                _UpMessages();
            }
        }

        /// <summary>
        /// Запускается при создании объекта и при смене режима FullScreen
        /// </summary>
        protected override void _OnInitialize()
        {
            base._OnInitialize();
            _AddControls(_textBoxMessage);
            _AddControls(_labelMessages);
            for (int i = 0; i < _labelplayer.Length; i++)
            {
                _AddControls(_labelplayer[i]);
            }
            _UpMessages();
        }

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        protected override void _OnResized()
        {
            PosX = 8;
            PosY = Height - HeightWindow - 8;

            base._OnResized();
            _textBoxMessage.SetPosition(PosX, PosY + 314);// Height - 48);
            _labelMessages.SetPosition(PosX + 8, PosY + 30);// Height - 324);

            // Список игроков в игре
            int w2 = Width - 172;
            for (int i = 0; i < _labelplayer.Length; i++)
            {
                _labelplayer[i].SetPosition(w2, 164 + i * 20);
            }
        }

        public override void OnKeyDown(Keys keys)
        {
            if (keys == Keys.Escape)
            {
                _Close();
            }
            else if (keys == Keys.Enter)
            {
                // Тут действие текста
                if (_textBoxMessage.Text != "")
                {
                    window.Game.Player.Chat.AddMessageHistory(_textBoxMessage.Text);
                    window.Game.TrancivePacket(new PacketC14Message(_textBoxMessage.Text,
                        window.Game.Player.MovingObject));
                }
                _Close();
            }
            else if (keys == Keys.PageUp)
            {
                _PageBackNext(true);
            }
            else if (keys == Keys.PageDown)
            {
                _PageBackNext(false);
            }
            else if (keys == Keys.Up)
            {
                _SentHistory(-1);
            }
            else if (keys == Keys.Down)
            {
                _SentHistory(1);
            }
            else
            {
                base.OnKeyDown(keys);
            }
        }

        /// <summary>
        /// Нажата клавиша в char формате
        /// </summary>
        public override void OnKeyPress(char key)
        {
            if (_flagDuplication)
            {
                _flagDuplication = false;
            }
            else
            {
                base.OnKeyPress(key);
            }
        }

        /// <summary>
        /// Вращение колёсика мыши
        /// </summary>
        /// <param name="delta">смещение</param>
        public override void OnMouseWheel(int delta, int x, int y)
        {
            if (delta != 0) _PageBackNext(delta > 0);
        }

        public override void Dispose()
        {
            base.Dispose();
            window.Game.Hud.ChatOff();
        }

        /// <summary>
        /// Обновить сообщения чата
        /// </summary>
        private void _UpMessages()
        {
            string message = "";
            string textResult = "";

            int count = window.Game.Player.Chat.Messages.Count;
            if (count > 0)
            {
                int i;
                int countMax = 0;
                count--;
                count -= _scrollPos;
                for (i = count; i >= 0; i--)
                {
                    message = window.Game.Player.Chat.Messages[i];
                    countMax++;
                    if (textResult == "")
                    {
                        textResult = message;
                    }
                    else
                    {
                        textResult = message + "\r\n" + textResult;
                    }
                    if (countMax >= _countMaxLine) { break; }
                }
            }
            window.Game.Player.Chat.Update();
            _labelMessages.SetText(textResult);
        }

        /// <summary>
        /// Показать шаг истории
        /// </summary>
        private void _SentHistory(int step)
        {
            int cursor = _sentHistoryCursor + step;
            int count = window.Game.Player.Chat.SentMessages.Count;
            cursor = Mth.Clamp(cursor, 0, count);

            if (cursor != _sentHistoryCursor)
            {
                if (cursor == count)
                {
                    _sentHistoryCursor = count;
                    _textBoxMessage.SetText(_historyBuffer);
                }
                else
                {
                    if (_sentHistoryCursor == count)
                    {
                        _historyBuffer = _textBoxMessage.Text;
                    }
                    _textBoxMessage.SetText(window.Game.Player.Chat.SentMessages[cursor]);
                    _sentHistoryCursor = cursor;
                }
                _textBoxMessage.UpCursor();
            }
        }

        private void _PageBackNext(bool next)
        {
            int old = _scrollPos;
            int countMax = window.Game.Player.Chat.Messages.Count;
            _scrollPos += next ? 1 : -1;
            if (_scrollPos > countMax - _countMaxLine) _scrollPos = countMax - _countMaxLine;
            if (_scrollPos < 0) _scrollPos = 0;
        //    if (old != _scrollPos)
            {
                _UpMessages();
                _PageUpdate();
               // _isRender = true;
            }
        }

        private void _PageUpdate()
        {
            //if (_scrollPos > 0) buttonBack.Enabled = true;
            //else buttonBack.Enabled = false;
            //if (_scrollPos + _countMaxLine < listMessages.Count) buttonNext.Enabled = true;
            //else buttonNext.Enabled = false;
        }
    }
}
