using Vge.Games;
using Vge.Renderer.World.Entity;

namespace Vge.Management
{
    /// <summary>
    /// Объект игроков на клиентской стороне
    /// </summary>
    public class PlayerClient : PlayerBase
    {
        #region PositionFrame 

        /// <summary>
        /// Позиция этой сущности по оси X
        /// </summary>
        public float PosFrameX;
        /// <summary>
        /// Позиция этой сущности по оси Y
        /// </summary>
        public float PosFrameY;
        /// <summary>
        /// Позиция этой сущности по оси Z
        /// </summary>
        public float PosFrameZ;

        /// <summary>
        /// Вращение этой сущности по оси Y
        /// </summary>
        public float RotationFrameYaw;
        /// <summary>
        /// Вращение этой сущности вверх вниз
        /// </summary>
        public float RotationFramePitch;

        #endregion

        /// <summary>
        /// Класс игры
        /// </summary>
        protected readonly GameBase _game;

        public PlayerClient(GameBase game)
        {
            _game = game;
            Eye = Height * .85f;
            Type = Entity.EnumEntity.Player;
            if (!(this is PlayerClientOwner))// game.WorldRender != null)
            {
                Render = new EntityRenderClient(this, game.WorldRender.Entities);
            }
        }

        /// <summary>
        /// Задать данные игрока
        /// </summary>
        public void SetDataPlayer(int id, string uuid, string login, byte idWorld)
        {
            Login = login;
            UUID = uuid;
            Id = id;
            IdWorld = idWorld;
        }

        public override void Update()
        {
            if (LevelMotionChange == 2)
            {
                LevelMotionChange = 1;
            }
            else if (LevelMotionChange == 1)
            {
                UpdatePrev();
                LevelMotionChange = 0;
            }
        }
    }
}
