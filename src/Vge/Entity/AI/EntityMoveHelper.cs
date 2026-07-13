using System.Runtime.CompilerServices;
using WinGL.Util;

namespace Vge.Entity.AI
{
    /// <summary>
    /// Перемещение сущности по координатам
    /// </summary>
    public class EntityMoveHelper
    {
        /// <summary>
        /// Скорость перемещения
        /// </summary>
        public float Speed { get; private set; } = 0;

        private readonly EntityMob _entity;

        private float _posX, _posY, _posZ;

        private bool _move;

        /// <summary>
        /// Обновить перемещение
        /// </summary>
        private bool _updateMove;
        /// <summary>
        /// Обновить прыжок
        /// </summary>
        private bool _updateJump;
        /// <summary>
        /// Ускорение
        /// </summary>
        private bool _updateSprinting;
        /// <summary>
        /// Обновить сесть
        /// </summary>
        private bool _updateSneak;
        /// <summary>
        /// Пробуждаем физику
        /// </summary>
        private bool _awakenPhysics;

        public EntityMoveHelper(EntityMob entity)
        {
            _entity = entity;
            _posX = entity.PosX;
            _posY = entity.PosY;
            _posZ = entity.PosZ;
        }

        /// <summary>
        /// Устанавливает скорость и место для перемещения
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetMoveTo(Vector3 pos, float speed) => SetMoveTo(pos.X, pos.Y, pos.Z, speed);
        /// <summary>
        /// Устанавливает скорость и место для перемещения
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetMoveTo(float x, float y, float z, float speed)
        {
            _posX = x;
            _posY = y;
            _posZ = z;
            Speed = speed;
            _updateMove = true;
        }

        /// <summary>
        /// Задать прыжок
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetJumping() => _updateJump = true;

        /// <summary>
        /// Задать ускорение
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetSprinting() => _updateSprinting = true;

        /// <summary>
        /// Задать крастся
        /// </summary>
        /// <param name="awakenPhysics">Пробуждаем физику</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetSneak(bool awakenPhysics = true)
        {
            _updateSneak = true;
            _awakenPhysics = awakenPhysics;
        }

        public void OnUpdateMove()
        {
            if (_move)
            {
                _move = false;
                _entity.Physics.Movement.SetStop();
            }
            if (_updateMove)
            {
                _updateMove = false;
                float x = _entity.PosX - _posX;
                float y = _posY - _entity.PosY;
                float z = _entity.PosZ - _posZ;
                float rotationYaw = Glm.Atan2(z, x) - Glm.Pi90;
                if (rotationYaw != _entity.RotationYaw)
                {
                    _entity.SetRotationAI(rotationYaw, 0);
                }
                // Если сущность может летать, она же плавать, есть возможность управлять вверх и вниз
                if (_entity.Physics.IsFlying && x * x + z * z < 1f)
                {
                    if (y > .5f) _entity.Physics.Movement.SetJump(true);
                    else if (y < .5f) _entity.Physics.Movement.SetSneak(true);
                }
                _entity.Physics.Movement.SetForward(Speed);
                _entity.Physics.AwakenPhysics();
                _move = true;
            }
            if (_updateJump)
            {
                _updateJump = false;
                _entity.Physics.Movement.SetJump(true);
                _entity.Physics.AwakenPhysics();
                _move = true;
            }
            if (_updateSprinting)
            {
                _updateSprinting = false;
                _entity.Physics.Movement.SetSprinting(true);
                _entity.Physics.AwakenPhysics();
                _move = true;
            }
            if (_updateSneak)
            {
                _updateSneak = false;
                _entity.Physics.Movement.SetSneak(true);
                if (_awakenPhysics)
                {
                    _entity.Physics.AwakenPhysics();
                }
                _move = true;
            }
        }
    }
}
