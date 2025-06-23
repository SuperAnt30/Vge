using System;
using Vge.Games;
using WinGL.Actions;

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
        /// <summary>
        /// Время клика клавиши Пробел
        /// </summary>
        private long _timeClickSpace = 0;

        public Keyboard(GameBase game)
        {
            _game = game;
        }

        /// <summary>
        /// Клавиша нажата
        /// </summary>
        public void OnKeyDown(Keys keys)
        {
            //Debug.Text = keys.ToString();
            if (_keyF3)
            {
                // Сочетания клавиш с F3
                if (keys == Keys.C) Ce.IsDebugDrawChunks = !Ce.IsDebugDrawChunks; // F3+C
                else if (keys == Keys.G) _game.WorldRender.ChunkCursorHiddenShow(); // F3+G
                else if (keys == Keys.B) _game.WorldRender.HitboxEntitiesHiddenShow(); // F3+B
            }
            else
            {
                // Без сочетания с F3

                if (keys == Keys.F3) _keyF3 = true;
                else if (keys == Keys.Escape || keys == Keys.Menu) _OnInGameMenu();
                else if (keys == Keys.Space && !_game.Player.Movement.Jump)
                {
                    // Проверка двойного нажатия пробела
                    long ms = _game.Time();
                    if (_keyPrev == keys && ms - _timeClickSpace < 300)
                    {
                        // Это двойной клик Пробела
                        _game.Player.KeyDoubleClickSpace();
                    }
                    _timeClickSpace = ms;
                }

                switch (keys)
                {
                    case Keys.W: _game.Player.Movement.Forward = true; _game.Player.InputKeyMove(); break;
                    case Keys.E: _game.Player.Movement.Forward = true; _game.Player.InputKeyMove(); break;
                    case Keys.A: _game.Player.Movement.Left = true; _game.Player.InputKeyMove(); break;
                    case Keys.D: _game.Player.Movement.Right = true; _game.Player.InputKeyMove(); break;
                    case Keys.S: _game.Player.Movement.Back = true; _game.Player.InputKeyMove(); break;
                    case Keys.Space: _game.Player.Movement.Jump = true; _game.Player.InputKeyMove(); break;
                    case Keys.ShiftKey:
                        if (!_game.Player.Movement.Sneak)
                        {
                            _game.Player.Movement.Sneak = true;
                            _game.Player.InputKeyMove();
                        }
                        break;
                    case Keys.ControlKey:
                        if (!_game.Player.Movement.Sprinting)
                        {
                            _game.Player.Movement.Sprinting = true;
                            _game.Player.InputKeyMove();
                        }
                        break;
                    case Keys.Tab: _game.MouseFirstPersonView(false); break;

                    case Keys.T: case Keys.Oemtilde: _OnInChat(); break; // Окно чата Клавиша "T" или "~"
                    case Keys.F5: _game.Player.ViewCameraEye = !_game.Player.ViewCameraEye; break;
                    case Keys.F8: Debug.IsDrawVoxelLine = !Debug.IsDrawVoxelLine; break;
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
            }
            _keyPrev = keys;
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
                        _game.Player.InputKeyMove();
                        break;
                    case Keys.A: _game.Player.Movement.Left = false; _game.Player.InputKeyMove(); break;
                    case Keys.D: _game.Player.Movement.Right = false; _game.Player.InputKeyMove(); break;
                    case Keys.S: _game.Player.Movement.Back = false; _game.Player.InputKeyMove(); break;
                    case Keys.Space: _game.Player.Movement.Jump = false; _game.Player.InputKeyMove(); break;
                    case Keys.ShiftKey: _game.Player.Movement.Sneak = false; _game.Player.InputKeyMove(); break;
                    case Keys.ControlKey:
                        if (!_game.Player.Movement.Forward)
                        {
                            _game.Player.Movement.Sprinting = false;
                            _game.Player.InputKeyMove();
                        }
                        break;
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


        /// <summary>
        /// Событие активация чата
        /// </summary>
        public event EventHandler InChat;
        protected virtual void _OnInChat()
            => InChat?.Invoke(this, new EventArgs());

        #endregion
    }
}
