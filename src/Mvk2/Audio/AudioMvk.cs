using Mvk2.Util;
using System;
using Vge.Audio;
using Vge.Util;

namespace Mvk2.Audio
{
    /// <summary>
    /// Класс звуков для Малювек 2
    /// </summary>
    public class AudioMvk : AudioBase
    {
        /// <summary>
        /// Получить количество шагов для загрузки, они же количество семплов
        /// </summary>
        public int GetCountStep() => AudioIndexs.Count();

        /// <summary>
        /// Загрузка сэмпла
        /// </summary>
        public override void InitializeSample()
        {
            AudioSample sample;
            int count = GetCountStep();
            for (int i = 0; i < count; i++)
            {
                sample = new AudioSample();
                sample.LoadOgg(Options.PathSounds + AudioIndexs.GetPath(i));
                _items[i] = sample;
                OnStep();
            }

            AudioIndexs.ClearPath();

            //AudioSample sample = new AudioSample();
            //sample.LoadWave(Options.PathSounds + "say1.wav");
            //_items[2] = sample;
            //OnStep();
            //_StepOgg(1, "Click", "Click.ogg");
            //_StepOgg(0, "DigStone4", "DigStone4.ogg");
        }

        #region Event

        /// <summary>
        /// Событие шаг
        /// </summary>
        public event EventHandler Step;
        /// <summary>
        /// Событие шаг
        /// </summary>
        protected void OnStep() => Step?.Invoke(this, new EventArgs());

        #endregion
    }
}
