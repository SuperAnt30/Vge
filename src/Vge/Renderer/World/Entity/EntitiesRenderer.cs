using Vge.Entity;
using Vge.Games;
using Vge.Util;
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

        private HitboxEntityRender _hitbox;

        public EntitiesRenderer(GameBase game) : base(game)
        {
            gl = GetOpenGL();
            _hitbox = new HitboxEntityRender(gl);
        }

        /// <summary>
        /// Метод для прорисовки кадра
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public override void Draw(float timeIndex)
        {
            int count = _game.World.LoadedEntityList.Count;
            EntityBase entity;
            int playerId = _game.Player.Id;
            float x = _game.Player.PosFrameX;
            float y = _game.Player.PosFrameY;
            float z = _game.Player.PosFrameZ;
            for (int i = 0; i < count; i++)
            {
                entity = _game.World.LoadedEntityList.GetAt(i) as EntityBase;
                int entityId = entity.Id;
                if (entityId != playerId)
                {
                    _game.WorldRender.Render.ShaderBindLine(_game.Player.View, 
                        entity.GetPosFrameX(timeIndex) - x,
                        entity.GetPosFrameY(timeIndex) - y,
                        entity.GetPosFrameZ(timeIndex) - z);
                    _hitbox.Draw(timeIndex, entity);
                }
            }
        }

        /// <summary>
        /// Метод для прорисовки основного игрока
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public void DrawOwner(float timeIndex)
        {
            _game.WorldRender.Render.ShaderBindLine(_game.Player.View, 0, 0, 0);
            _hitbox.Draw(timeIndex, _game.Player);
        }

        public override void Dispose()
        {
            _hitbox.Dispose();
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
