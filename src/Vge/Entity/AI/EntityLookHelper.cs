using System.Runtime.CompilerServices;
using WinGL.Util;

namespace Vge.Entity.AI
{
    /// <summary>
    /// Вращение сущности, с возможностью плавности
    /// </summary>
    public class EntityLookHelper
    {
        private readonly EntityMob _entity;

        /// <summary>
        /// Количество изменений, которые вносятся при каждом обновлении для объекта, 
        /// обращенного в направлении Yaw
        /// </summary>
        private float _deltaLookYaw;

        /// <summary>
        /// Количество изменений, которые вносятся при каждом обновлении объекта, 
        /// обращенного в направлении Pitch
        /// </summary>
        private float _deltaLookPitch;

        /// <summary>
        /// Независимо от того, пытается ли сущность смотреть на что-то
        /// </summary>
        private bool _isLooking;
        /// <summary>
        /// Позиция куда смотрим
        /// </summary>
        private float _posLookingX, _posLookingY, _posLookingZ;

        private bool _isLookingOffset;
        private float _renderYawOffset;
        private float _renderPitchOffset;

        public EntityLookHelper(EntityMob entity) => _entity = entity;

        /// <summary>
        /// Устанавливает позицию для просмотра с помощью объекта
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetLookPositionWithEntity(EntityBase inEntity, float deltaYaw, float deltaPitch, float upThrower = 0)
        {
            _posLookingX = inEntity.PosX;
            _posLookingY = inEntity.PosY;
            _posLookingZ = inEntity.PosZ;

            if (inEntity is EntityLiving entityLiving)
            {
                _posLookingY += entityLiving.GetEyeHeight();
            }
            else
            {
                _posLookingY += inEntity.Size.GetHeight();
            }
            if (upThrower != 0)
            {
                _posLookingY += upThrower;
            }
            _deltaLookYaw = deltaYaw;
            _deltaLookPitch = deltaPitch;
            _isLooking = true;
        }

        /// <summary>
        /// Устанавливает позицию для просмотра
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetLookPosition(float posX, float posY, float posZ, float deltaYaw, float deltaPitch)
        {
            _posLookingX = posX;
            _posLookingY = posY;
            _posLookingZ = posZ;
            _deltaLookYaw = deltaYaw;
            _deltaLookPitch = deltaPitch;
            _isLooking = true;
        }

        /// <summary>
        /// Устанавливает позицию для просмотра вертикали
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetLookPitch(float pitch, float deltaPitch)
        {
            _isLookingOffset = true;
            _isLooking = false;
            _renderYawOffset = _entity.RotationYaw;
            _deltaLookPitch = deltaPitch;
            _renderPitchOffset = pitch;
        }

        public void OnUpdateLook()
        {
            float rotationPitch;
            float rotationYaw;

            if (_isLooking)
            {
                _isLookingOffset = true;
                _isLooking = false;
                float x = _entity.PosX - _posLookingX;
                float y = _entity.PosY + _entity.GetEyeHeight() - _posLookingY;
                float z = _entity.PosZ - _posLookingZ;
                float k = Mth.Sqrt(x * x + z * z);
                _renderYawOffset = Glm.Atan2(z, x) - Glm.Pi90;
                _renderPitchOffset = -Glm.Atan2(y, k);
                rotationYaw = _UpdateRotation(_entity.RotationYaw, _renderYawOffset, _deltaLookYaw);
                rotationPitch = _UpdateRotation(_entity.RotationPitch, _renderPitchOffset, _deltaLookPitch);
                if (rotationYaw != _entity.RotationYaw || rotationPitch != _entity.RotationPitch)
                {
                    _entity.SetRotationAI(rotationYaw, rotationPitch);
                }
            }
            else if (_isLookingOffset)
            {
                rotationYaw = _UpdateRotation(_entity.RotationYaw, _renderYawOffset, _deltaLookYaw);
                rotationPitch = _UpdateRotation(_entity.RotationPitch, _renderPitchOffset, _deltaLookPitch);

                if (rotationYaw == _entity.RotationYaw && rotationPitch == _entity.RotationPitch)
                {
                    _isLookingOffset = false;
                }
                else
                {
                    _entity.SetRotationAI(rotationYaw, rotationPitch);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float _UpdateRotation(float angle1, float angle2, float delta)
        {
            float angle = Glm.WrapAngleToPi(angle2 - angle1);
            if (angle < .00001f && angle > -.00001f) return angle1;
            if (angle > delta) angle = delta;
            if (angle < -delta) angle = -delta;
            return angle1 + angle;
        }
    }
}
