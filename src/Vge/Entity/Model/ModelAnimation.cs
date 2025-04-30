using System.Collections.Generic;

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
        /// Анимация без перерыва, цикл
        /// </summary>
        public readonly bool Loop;
        /// <summary>
        /// Анимация кости
        /// </summary>
        public readonly List<AnimationBone> Bones = new List<AnimationBone>();

        public ModelAnimation(string name, bool loop)
        {
            Name = name;
            Loop = loop;
        }

        public override string ToString() => Name + " " + Bones.Count;

        /// <summary>
        /// Анимация кости
        /// </summary>
        public class AnimationBone
        {
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
