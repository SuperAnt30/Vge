using Vge.Games;

namespace Vge.Renderer.Huds
{
    /// <summary>
    /// Абстрактный класс индикации. Heads-Up Display
    /// </summary>
    public abstract class HudBase : WarpRenderer
    {

        public HudBase(GameBase game) : base(game)
        {

        }

        /// <summary>
        /// Метод для прорисовки кадра
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public override void Draw(float timeIndex)
        {

        }

        public override void Dispose() { }
    }
}
