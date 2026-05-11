using Vge.Entity;
using Vge.Entity.Animation;
using Vge.Entity.Player;
using Vge.Entity.Render;
using Vge.Renderer.World.Entity;

namespace Mvk2.Entity.Render
{
    /// <summary>
    /// Рендер основного игрока, для игры с глаз
    /// </summary>
    public class EntityRenderOwner : EntityRenderEyeMouth
    {
        private PlayerClientOwner _clientOwner;

        public EntityRenderOwner(PlayerClientOwner clientOwner, EntitiesRenderer entities, ResourcesEntity resourcesEntity)
           : base(clientOwner, entities, resourcesEntity)
        {
            _clientOwner = clientOwner;
        }

        /// <summary>
        /// Обновить рассчитать матрицы для кадра основного игрока с глаз
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public override void UpdateMatrixOwnerEye(float timeIndex)
        {
            // Возвращаем значения костей в исходное положение, Оригинал
            for (byte i = 0; i < _countBones; i++)
            {
                if (_bonesFlagModify[i])
                {
                    _resourcesEntity.Bones[i].SetBonePose(ref _bones[i]);
                    _bonesFlagModify[i] = false;
                }
            }

            // Генерируем кости текущих поз из анимации
            _GenBoneCurrentPoses();
            // Собираем конечные матрицы
            _GetMatrixPalette(_clientOwner.GetRotationFrameYaw(timeIndex),
                _clientOwner.GetRotationFramePitch(timeIndex));
        }

        /// <summary>
        /// Получение матричной палитры для позы
        /// </summary>
        private void _GetMatrixPalette(float yaw, float pitch)
        {
            // Корневая кость
            _bonesTransforms[0].Clear();

            if (yaw != 0)
            {
                _bonesTransforms[0].RotateY(-yaw);
            }
            // Вращаем всё тело по накулону, центр вращения где глаза
            if (pitch != 0)
            {
                _bonesTransforms[0].Translate(0, _clientOwner.GetEyeHeight(), 0);
                _bonesTransforms[0].RotateX(pitch + .6f); // .8
                _bonesTransforms[0].Translate(0, -_clientOwner.GetEyeHeight(), 0);
            }

            // У корневой кости нет родительской матрицы, поэтому заносим в матричную палитру матрицу перехода как есть
            _bonesTransforms[0].Multiply(_bones[0].GetBoneMatrix());
            _bonesTransforms[0].Multiply(_resourcesEntity.Bones[0].MatrixInverse);
            _bonesTransforms[0].ConvArray4x3(_bufferBonesTransforms, 0);

            Bone bone;
            // Пользуясь порядком хранения, проходим по всем локальным позам и выполняем умножения на родительские матрицы и обратные
            for (int i = 1; i < _countBones; i++)
            {
                // Перемножаем матрицы в положение как выставленно в Blockbench
                bone = _resourcesEntity.Bones[i];

                _bonesTransforms[bone.ParentIndex].Copy(_bonesTransforms[i]);
                _bonesTransforms[i].Multiply(_bones[i].GetBoneMatrix());

                // Если надо вращаем Pitch, голова
                if (bone.IsHead)
                {
                    // Если головы нет, мы её перемещаем далеко вниз, чтоб не мешала игры с глаз
                    _bonesTransforms[i].Translate(0, -1000, 0);
                }

                // Умножаем обратную матрицу
                _bonesTransforms[i].Multiply(bone.MatrixInverse);
                // Отправляем в кеш
                _bonesTransforms[i].ConvArray4x3(_bufferBonesTransforms, i * 12);
            }
        }

    }
}
