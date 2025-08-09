using Vge.Entity.Render;
using Vge.Games;
using Vge.World;

namespace Vge.Entity.Player
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

        protected PlayerClient(GameBase game)
        {
            SolidHeadWithBody = false;
            _game = game;
            _InitMetaData();
            _InitSize();
        }

        /// <summary>
        /// Создание других игроков, сетевого, для клиента
        /// </summary>
        public PlayerClient(GameBase game, int id, string uuid, string login, byte idWorld) : this(game)
        {
            Login = login;
            UUID = uuid;
            Id = id;
            IdWorld = idWorld;
            _InitIndexPlayer();
            InitRender(IndexEntity, _game.WorldRender.Entities);
        }

        /// <summary>
        /// Игровой такт на клиенте
        /// </summary>
        /// <param name="deltaTime">Дельта последнего тика в mc</param>
        public override void UpdateClient(WorldClient world, float deltaTime)
        {
            UpdatePositionServer();

            // Поворот тела от поворота головы или движения
            RotationBody();

            Render.UpdateClient(world, deltaTime);
        }
    }
}
