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
            //Debug.Text = keys.ToString();
            if (_keyF3)
            {
                // Сочетания клавиш с F3
                if (keys == Keys.C) Ce.IsDebugDrawChunks = !Ce.IsDebugDrawChunks; // F3+C
                else if (keys == Keys.G) _game.WorldRender.ChunkCursorHiddenShow(); // F3+G
            }
            else
            {
                // Без сочетания с F3

                if (keys == Keys.F3) _keyF3 = true;
                else if (keys == Keys.Escape || keys == Keys.Menu) _OnInGameMenu();

                switch (keys)
                {
                    case Keys.W: _game.Player.Movement.Forward = true; break;
                    case Keys.E: _game.Player.Movement.Forward = true; break;
                    case Keys.A: _game.Player.Movement.Left = true; break;
                    case Keys.D: _game.Player.Movement.Right = true; break;
                    case Keys.S: _game.Player.Movement.Back = true; break;
                    case Keys.Space: _game.Player.Movement.Jump = true; break;
                    case Keys.ShiftKey: _game.Player.Movement.Sneak = true; break;
                    case Keys.ControlKey: _game.Player.Movement.Sprinting = true; break;
                    case Keys.Tab: _game.MouseFirstPersonView(false); break;

                    case Keys.F8: Debug.IsDrawVoxelLine = !Debug.IsDrawVoxelLine; break;
                }


                // TODO::2024-10-15 Отладка перемещения!!!

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
                        _game.Player.Position, false, false, false, 0));
                }
                else if (keys == Keys.NumPad1)
                {
                    _game.TrancivePacket(new Network.Packets.Client.PacketC04PlayerPosition(
                        _game.Player.Position, false, false, false, 1));
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
            else
            { 
                switch (keys)
                {
                    case Keys.W:
                        _game.Player.Movement.Forward = false;
                        _game.Player.Movement.Sprinting = false;
                        break;
                    case Keys.A: _game.Player.Movement.Left = false; break;
                    case Keys.D: _game.Player.Movement.Right = false; break;
                    case Keys.S: _game.Player.Movement.Back = false; break;
                    case Keys.Space: _game.Player.Movement.Jump = false; break;
                    case Keys.ShiftKey: _game.Player.Movement.Sneak = false; break;
                    case Keys.ControlKey: _game.Player.Movement.Sprinting = false; break;
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
