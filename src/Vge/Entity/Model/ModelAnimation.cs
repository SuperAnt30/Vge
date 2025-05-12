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
        /// Длинна ролика в секундах
        /// </summary>
        private readonly float _length;
        /// <summary>
        /// Имя анимации из Blockbench
        /// </summary>
        private readonly string _name;
        /// <summary>
        /// Анимация без перерыва, цикл
        /// </summary>
        private readonly bool _loop;
        /// <summary>
        /// Анимация кости
        /// </summary>
        private readonly List<AnimationBone> _bones = new List<AnimationBone>();

        public ModelAnimation(string name, bool loop, float length)
        {
            _name = name;
            _loop = loop;
            _length = length;
        }

        public void BoneAdd(AnimationBone bone) => _bones.Add(bone);

        /// <summary>
        /// Создать модель отдельного анимационного клипа
        /// </summary>
        /// <param name="amountBoneIndex">Количество костей</param>
        public ModelAnimationClip CreateModelAnimationClip(byte amountBoneIndex)
            => new ModelAnimationClip(_name, _loop, _length * 1000f,
                _GetBoneAnimationClip(amountBoneIndex));

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

            return new BoneAnimationChannel(_name, _loop,
                positionFrames.ToArray(), orientationFrames.ToArray());
        }

        public override string ToString() => _name + " " + _bones.Count;

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

        public struct KeyFrames
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
