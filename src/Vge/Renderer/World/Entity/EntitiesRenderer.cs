using Vge.Entity;
using Vge.Games;
using WinGL.OpenGL;

namespace Vge.Renderer.World.Entity
{
    /// <summary>
    /// Объект рендера всех сущностей
    /// </summary>
    public class EntitiesRenderer : WarpRenderer
    {
        /// <summary>
        /// Объект OpenGL для элемента управления
        /// </summary>
        private readonly GL gl;

        private HitboxEntityRender _owner;

        public EntitiesRenderer(GameBase game) : base(game)
        {
            gl = GetOpenGL();
            _owner = new HitboxEntityRender(gl, _game.Player);
        }

        /// <summary>
        /// Метод для прорисовки кадра
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public override void Draw(float timeIndex)
        {

            
        }

        /// <summary>
        /// Метод для прорисовки основного игрока
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public void DrawOwner(float timeIndex)
        {
            _game.WorldRender.Render.ShaderBindLine(_game.Player.View, 0, 0, 0);
            _owner.Draw(timeIndex);
        }

        public override void Dispose()
        {
            _owner.Dispose();
        }

        private void _RenderEntity(EntityBase entity, float timeIndex)
        {
            _game.WorldRender.Render.ShaderBindLine(_game.Player.View,
                _game.Player.PosFrameX,
                _game.Player.PosFrameY,
                _game.Player.PosFrameZ);
           // _owner.Draw();
        }

    }
}
