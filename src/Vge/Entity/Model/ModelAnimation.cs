using System.Collections.Generic;
using Vge.Entity.Animation;
using WinGL.Util;

namespace Vge.Entity.Model
{
    /// <summary>
    /// Объект анимации модели
    /// </summary>
    public class ModelAnimation
    {
        /// <summary>
        /// Имя анимации из Blockbench
        /// </summary>
        public readonly string Name;
        /// <summary>
        /// Длинна ролика в секундах
        /// </summary>
        private readonly float _length;
        /// <summary>
        /// Анимация без перерыва, цикл
        /// </summary>
        private readonly ModelLoop _loop;
        /// <summary>
        /// Время микса в милисекундах
        /// </summary>
        private readonly int _timeMix;
        /// <summary>
        /// Анимация кости
        /// </summary>
        private readonly List<AnimationBone> _bones = new List<AnimationBone>();

        public ModelAnimation(string name, ModelLoop loop, float length, int timeMix)
        {
            Name = name;
            _loop = loop;
            _length = length;
            _timeMix = timeMix;
        }

        public void BoneAdd(AnimationBone bone) => _bones.Add(bone);

        /// <summary>
        /// Создать модель отдельного анимационного клипа
        /// </summary>
        /// <param name="amountBoneIndex">Количество костей</param>
        public ModelAnimationClip CreateModelAnimationClip(byte amountBoneIndex, float speed)
            => new ModelAnimationClip(Name, _loop, _length * 1000f, _timeMix,
                speed, _GetBoneAnimationClip(amountBoneIndex));

        /// <summary>
        /// Сгенерировать cписки ключевых кадров для каждой кости скелета
        /// </summary>
        private BoneAnimationChannel[] _GetBoneAnimationClip(byte amountBoneIndex)
        {
            // Массив костей
            BoneAnimationChannel[] animationChannels = new BoneAnimationChannel[amountBoneIndex];

            for (int i = 0; i < amountBoneIndex; i++)
            {
                animationChannels[i] = _CreateBoneAnimation(i);
            }
            return animationChannels;
        }

        /// <summary>
        /// Создать класс набора ключевых кадров отдельной кости
        /// </summary>
        private BoneAnimationChannel _CreateBoneAnimation(int boneIndex)
        {
            // Массив кадров с позицией кости
            List<BoneAnimationFrame> positionFrames = new List<BoneAnimationFrame>();
            // Массив кадров с ориентацией кости
            List<BoneAnimationFrame> orientationFrames = new List<BoneAnimationFrame>();

            foreach(AnimationBone animationBone in _bones)
            {
                // Ищем нужную кость
                if (animationBone.Index == boneIndex)
                {
                    foreach(KeyFrames keyFrames in animationBone.Frames)
                    {
                        if (keyFrames.IsRotation)
                        {
                            orientationFrames.Add(new BoneAnimationFrame(keyFrames.Time * 1000f,
                                -Glm.Radians(keyFrames.X), -Glm.Radians(keyFrames.Y), Glm.Radians(keyFrames.Z)));
                        }
                        else
                        {
                            positionFrames.Add(new BoneAnimationFrame(keyFrames.Time * 1000f,
                                keyFrames.X / 16f, keyFrames.Y / 16f, keyFrames.Z / 16f));
                        }
                    }
                }
            }
            positionFrames.Sort();
            orientationFrames.Sort();
            return new BoneAnimationChannel(Name, _loop,
                positionFrames.ToArray(), orientationFrames.ToArray());
        }

        public override string ToString() => Name + " " + _bones.Count;

        /// <summary>
        /// Анимация кости
        /// </summary>
        public class AnimationBone
        {
            /// <summary>
            /// Индекс кости
            /// </summary>
            public readonly int Index;

            /// <summary>
            /// Кадры
            /// </summary>
            public readonly List<KeyFrames> Frames = new List<KeyFrames>();

            public AnimationBone(int index) => Index = index;

            public override string ToString() => Index + " " + Frames.Count;
        }

        public readonly struct KeyFrames
        {
            public readonly bool IsRotation;
            public readonly float Time;
            public readonly float X;
            public readonly float Y;
            public readonly float Z;

            public KeyFrames(bool isRotation, float time, float x, float y, float z)
            {
                IsRotation = isRotation;
                Time = time;
                X = x;
                Y = y;
                Z = z;
            }

            public override string ToString() => Time.ToString("0.00");
        }

    }
}
