using System;
using Vge.Games;
using WinGL.Actions;

namespace Vge.Actions
{
    /// <summary>
    /// Объект отвечающий за отклики клавиатуры во время игры без Окна!!!
    /// </summary>
    public class Keyboard
    {
        /// <summary>
        /// Нажата клавиша Shift
        /// </summary>
        public bool KeyShift { get; private set; }

        private readonly GameBase _game;

        /// <summary>
        /// Какая клавиша была нажата ранее
        /// </summary>
        private Keys _keyPrev;
        /// <summary>
        /// Нажата клавиша F3
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
            if (!_keyF3)
            {
                // Без сочетания с F3

                if (keys == Keys.Escape || keys == Keys.Menu) _OnInGameMenu();
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
                    case Keys.W: _game.Player.KeyForward(true); break;
                    case Keys.A: _game.Player.KeyStrafeLeft(true); break;
                    case Keys.D: _game.Player.KeyStrafeRight(true); break;
                    case Keys.S: _game.Player.KeyBack(true); break;
                    case Keys.Space: _game.Player.KeyJump(true); break;
                    case Keys.ShiftKey: _game.Player.KeySneak(true); break;
                    case Keys.ControlKey: _game.Player.KeySprinting(true); break;
                    case Keys.Tab: _game.MouseFirstPersonView(false); break;
                    case Keys.F5: _game.Player.ViewCameraNext(); break;
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
        }

        /// <summary>
        /// Клавиша отпущена
        /// </summary>
        public void OnKeyUp(Keys keys)
        {
            switch (keys)
            {
                case Keys.W: _game.Player.KeyForward(false); break;
                case Keys.A: _game.Player.KeyStrafeLeft(false); break;
                case Keys.D: _game.Player.KeyStrafeRight(false); break;
                case Keys.S: _game.Player.KeyBack(false); break;
                case Keys.Space: _game.Player.KeyJump(false); break;
                case Keys.ShiftKey: _game.Player.KeySneak(false); break;
                case Keys.ControlKey: _game.Player.KeySprinting(false); break;
            }
        }

        /// <summary>
        /// Нажата клавиша в char формате
        /// </summary>
        public void OnKeyPress(char key) { }

        /// <summary>
        /// Клавиша нажата принудительно, не вожно открыт ли Screen
        /// </summary>
        public void OnKeyDownForcedly(Keys keys)
        {
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
                else if (keys == Keys.ShiftKey) KeyShift = true;
                else if (keys == Keys.F8) Debug.IsDrawVoxelLine = !Debug.IsDrawVoxelLine;
            }
            _keyPrev = keys;
        }

        /// <summary>
        /// Клавиша отпущена принудительно, не вожно открыт ли Screen
        /// </summary>
        public void OnKeyUpForcedly(Keys keys)
        {
            if (keys == Keys.F3)
            {
                _keyF3 = false;
                if (_keyPrev == Keys.F3)
                {
                    Ce.IsDebugDraw = !Ce.IsDebugDraw;
                }
            }
            if (keys == Keys.ShiftKey) KeyShift = false;
        }


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
