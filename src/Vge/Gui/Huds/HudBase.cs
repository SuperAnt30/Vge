using System.Collections.Generic;
using Vge.Games;
using Vge.Renderer;
using WinGL.Util;

namespace Vge.Gui.Huds
{
    /// <summary>
    /// Абстрактный класс индикации. Heads-Up Display
    /// </summary>
    public abstract class HudBase : WarpRenderer
    {
        /// <summary>
        /// Сетка перекрестия (прицел)
        /// </summary>
        protected readonly MeshGuiLine _meshCrosshair;
        /// <summary>
        /// Объект части чата
        /// </summary>
        protected readonly PartHubChat _partHubChat;

        public HudBase(GameBase game) : base(game)
        {
            _meshCrosshair = new MeshGuiLine(game.GetOpenGL());
            _partHubChat = new PartHubChat(game);
            _RenderCrosshair();
        }

        /// <summary>
        /// Включился чат (ScreenChat)
        /// </summary>
        public virtual void ChatOn() => _partHubChat.Hidden = true;

        /// <summary>
        /// Выключился чат (ScreenChat)
        /// </summary>
        public virtual void ChatOff() => _partHubChat.Hidden = false;
        
        /// <summary>
        /// Изменён размер окна
        /// </summary>
        public virtual void OnResized(int width, int height)
        {
            _partHubChat.Renderer();
            _RenderCrosshair();
        }

        /// <summary>
        /// Игровой такт
        /// </summary>
        /// <param name="deltaTime">Дельта последнего тика в mc</param>
        public override void OnTick(float deltaTime)
            => _partHubChat.OnTick(deltaTime);

        /// <summary>
        /// Рендер прицела
        /// </summary>
        protected virtual void _RenderCrosshair()
        {
            int wc = Gi.Width / 2;
            int hc = Gi.Height / 2;

            List<float> buffer = new List<float>();
            buffer.AddRange(RenderFigure.RectangleLine(wc - 8, hc, wc + 8, hc, 1, 1, 1, 1));
            buffer.AddRange(RenderFigure.RectangleLine(wc, hc - 8, wc, hc + 8, 1, 1, 1, 1));
            _meshCrosshair.Reload(buffer.ToArray());
        }

        /// <summary>
        /// Метод для прорисовки кадра
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public override void Draw(float timeIndex)
        {
            // Для контура надо перекулючится без текстуры
            _game.Render.ShaderBindGuiLine();

            if (_game.Player.ViewCamera == Entity.Player.EnumViewCamera.Eye)
            {
                _meshCrosshair.Draw();
            }

            _partHubChat.Draw(timeIndex);
        }

        public override void Dispose()
        {
            _meshCrosshair.Dispose();
            _partHubChat.Dispose();
        }
    }
}
