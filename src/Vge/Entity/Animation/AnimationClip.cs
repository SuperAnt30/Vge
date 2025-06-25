using System;
using System.Runtime.CompilerServices;

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
        /// Ресурсы сущности, для которого создан данный клип 
        /// </summary>
        private readonly ResourcesEntity _resourcesEntity;
        /// <summary>
        /// Количество костей
        /// </summary>
        private readonly byte _countBones;
        /// <summary>
        /// Начальное время микса в милисекундах
        /// </summary>
        private readonly float _timeMixBegin;
        /// <summary>
        /// Конечное время микса в милисекундах
        /// </summary>
        private readonly float _timeMixEnd;

        /// <summary>
        /// Счётчик времени, милисекунды
        /// </summary>
        private float _currentTime;
        /// <summary>
        /// Счётчик времени целиком сколько длится ролик даже если в цикле, милисекунды
        /// </summary>
        private float _currentTimeFull;
        /// <summary>
        /// Время фиксации остановки клипа, если = 0 то остановки не было
        /// </summary>
        private float _stopTime;

        public AnimationClip(ResourcesEntity resourcesEntity, ModelAnimationClip modelClip)
        {
            _resourcesEntity = resourcesEntity;
            _modelClip = modelClip;
            // Количество костей в текущей модели
            _countBones = (byte)_resourcesEntity.Bones.Length;
            CurrentPoseBones = new BonePose[_countBones];
            // Установка времени смешивания вначале клипа
            _timeMixBegin = 150;
            // Установка времени смешивания вконце клипа
            _timeMixEnd = 250;
        }

        /// <summary>
        /// Cбросить данные в исходное положение
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset() => _currentTimeFull = _currentTime = _stopTime = 0;
        /// <summary>
        /// Cбросить стоп
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ResetStop() => _currentTimeFull = _stopTime = 0;
        /// <summary>
        /// Проверяем закончено ли остановка клипа.
        /// После этого можно удалить клип.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsStoped() 
            => _stopTime != 0 && _timeMixEnd <= _currentTimeFull - _stopTime;
        /// <summary>
        /// Начинаем остановку клипа
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Stoping() => _stopTime = _currentTimeFull;

        /// <summary>
        /// Получить коэффициент микса 0..1
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetCoefMix()
        {
            if (_timeMixBegin != 0 && _timeMixBegin > _currentTimeFull)
            {
                return _currentTimeFull / _timeMixBegin;
            }
            if (_timeMixEnd != 0 && _stopTime != 0)
            {
                return 1f - (_currentTimeFull - _stopTime) / _timeMixEnd;
            }
            return 1f;
        }

        /// <summary>
        /// Увеличивает счётчик прошедшего с начала анимации времени
        /// </summary>
        /// <param name="delta">Дельта последнего кадра в mc</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncreaseCurrentTime(float delta)
        {
            _currentTimeFull += delta;
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
