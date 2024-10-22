using System;
using Vge.Games;
using WinGL.Actions;
using WinGL.Util;

namespace Vge.Actions
{
    /// <summary>
    /// Объект отвечающий за отклики клавиатуры во время игры
    /// </summary>
    public class Keyboard
    {
        private readonly GameBase _game;

        /// <summary>
        /// Какая клавиша была нажата ранее
        /// </summary>
        private Keys _keyPrev;
        /// <summary>
        /// Нажатая ли F3
        /// </summary>
        private bool _keyF3 = false;

        public Keyboard(GameBase game)
        {
            _game = game;
        }

        /// <summary>
        /// Клавиша нажата
        /// </summary>
        public void OnKeyDown(Keys keys)
        {
            _keyPrev = keys;
            Debug.Text = keys.ToString();
            if (_keyF3)
            {
                // Сочетания клавиш с F3
                if (keys == Keys.C) Ce.IsDebugDrawChunks = !Ce.IsDebugDrawChunks; // F3+C
            }
            else
            {
                // Без сочетания с F3

                if (keys == Keys.F3) _keyF3 = true;
                else if (keys == Keys.Escape || keys == Keys.Menu) _OnInGameMenu();


                // TODO::2024-10-15 Отладка перемещения!!!
                bool b = false;
                if (keys == Keys.Left)
                {
                    _game.Player.chPos.X -= 1;
                    b = true;
                }
                else if (keys == Keys.Right)
                {
                    _game.Player.chPos.X += 2;
                    b = true;
                }
                else if (keys == Keys.Up)
                {
                    _game.Player.chPos.Y -= 1;
                    b = true;
                }
                else if (keys == Keys.Down)
                {
                    _game.Player.chPos.Y += 1;
                    b = true;
                }
                if (b)
                {
                    _game.Player.UpView();
                    _game.TrancivePacket(new Network.Packets.Client.PacketC04PlayerPosition(
                        new Vector3(_game.Player.chPos.X, _game.Player.chPos.Y, 0),
                        false, false, false, _game.Player.IdWorld));
                    Debug.Player = _game.Player.chPos;
                }

                if (keys == Keys.PageUp)
                {
                    if (_game.Player.OverviewChunk < 49)
                    {
                        _game.Player.SetOverviewChunk((byte)(_game.Player.OverviewChunk + 1), true);
                    }
                }
                else if (keys == Keys.PageDown)
                {
                    if (_game.Player.OverviewChunk > 0)
                    {
                        _game.Player.SetOverviewChunk((byte)(_game.Player.OverviewChunk - 1), true);
                    }
                }
                else if (keys == Keys.NumPad0)
                {
                    _game.TrancivePacket(new Network.Packets.Client.PacketC04PlayerPosition(
                        new Vector3(_game.Player.chPos.X, _game.Player.chPos.Y, 0),
                        false, false, false, 0));
                }
                else if (keys == Keys.NumPad1)
                {
                    _game.TrancivePacket(new Network.Packets.Client.PacketC04PlayerPosition(
                        new Vector3(_game.Player.chPos.X, _game.Player.chPos.Y, 0),
                        false, false, false, 1));
                }
            }
        }

        /// <summary>
        /// Клавиша отпущена
        /// </summary>
        public void OnKeyUp(Keys keys)
        {
            if (keys == Keys.F3)
            {
                _keyF3 = false;
                if (_keyPrev == Keys.F3)
                {
                    Ce.IsDebugDraw = !Ce.IsDebugDraw;
                }
            }
        }

        /// <summary>
        /// Нажата клавиша в char формате
        /// </summary>
        public void OnKeyPress(char key) { }

        #region Event

        /// <summary>
        /// Событие выход в главное меню
        /// </summary>
        public event EventHandler InGameMenu;
        protected virtual void _OnInGameMenu()
            => InGameMenu?.Invoke(this, new EventArgs());

        #endregion
    }
}
