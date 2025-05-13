namespace Vge.Entity.Animation
{
    /// <summary>
    /// Класс, представляющий отдельный анимационный клип (например, бег, прицеливание, ходьбу)
    /// </summary>
    public class AnimationClip
    {
        /// <summary>
        /// Пустой ключевой кадр позиции или ориентации кости
        /// </summary>
        private static BoneAnimationFrame _boneAnimationFrameNull = new BoneAnimationFrame(0, 0, 0);

        /// <summary>
        /// Массив костей скелета в заданный момент времени
        /// </summary>
        public readonly BonePose[] CurrentPoseBones;

        /// <summary>
        /// Модель отдельного анимационного клипа
        /// </summary>
        private readonly ModelAnimationClip _modelClip;
        /// <summary>
        /// Модель сущности, для которого создан данный клип 
        /// </summary>
        private readonly ModelEntity _modelEntity;
        /// <summary>
        /// Количество костей
        /// </summary>
        private readonly byte _countBones;

        /// <summary>
        /// Счётчик времени, милисекунды
        /// </summary>
        private float _currentTime;


        public AnimationClip(ModelEntity modelEntity, ModelAnimationClip modelClip)
        {
            _modelEntity = modelEntity;
            _modelClip = modelClip;
            // Количество костей в текущей модели
            _countBones = (byte)_modelEntity.Bones.Length;
            CurrentPoseBones = new BonePose[_countBones];
        }

        /// <summary>
        /// Увеличивает счётчик прошедшего с начала анимации времени
        /// </summary>
        /// <param name="delta">Дельта последнего кадра в mc</param>
        public void IncreaseCurrentTime(float delta)
        {
            _currentTime += delta;
            if (_currentTime > _modelClip.Duration)
            {
                _currentTime -= _modelClip.Duration;
            }
        }

        /// <summary>
        /// Генерируем кости текущей позы
        /// </summary>
        public void GenBoneCurrentPose()
        {
            BoneAnimationChannel boneAnimationChannel;
            for (byte i = 0; i < _countBones; i++)
            {
                boneAnimationChannel = _modelClip.Bones[i];
                // Если имеется у кости анимация, то корректируем её
                if (boneAnimationChannel.IsAnimation)
                {
                    // Получаем среднее значение позиции кости в текущий момент времени
                    BoneAnimationFrame position
                        = _GetMixedAdjacentFrames(boneAnimationChannel.PositionFrames);

                    // Получаем среднее значение ориентации кости в текущий момент времени
                    BoneAnimationFrame rotation
                        = _GetMixedAdjacentFrames(boneAnimationChannel.OrientationFrames);

                    CurrentPoseBones[i].SetPositionRotation(
                        position.X, position.Y, position.Z,
                        rotation.X, rotation.Y, rotation.Z
                        );
                }
            }
        }

        /// <summary>
        /// Имеется ли в данной кости анимация
        /// </summary>
        public bool IsAnimation(byte boneIndex) => _modelClip.Bones[boneIndex].IsAnimation;

        /// <summary>
        /// Получает среднее значение позиции
        /// </summary>
        private BoneAnimationFrame _GetMixedAdjacentFrames(BoneAnimationFrame[] positionFrames)
        {
            if (positionFrames.Length == 0)
            {
                return _boneAnimationFrameNull;
            }
            if (positionFrames.Length == 1)
            {
                return positionFrames[0];
            }

            // Ищем ближайшие кадры
            BoneAnimationFrame frame;
            for (int i = 0; i < positionFrames.Length; i++)
            {
                frame = positionFrames[i];
                if (frame.Time == _currentTime)
                {
                    return frame;
                }
                if (frame.Time > _currentTime)
                {
                    return BoneAnimationFrame.Lerp(positionFrames[i - 1], frame, _currentTime);
                }
            }
            return _boneAnimationFrameNull;
        }
    }
}
