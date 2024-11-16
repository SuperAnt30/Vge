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
        /// Загрузка сэмпла
        /// </summary>
        public override void InitializeSample()
        {
            AudioSample sample = new AudioSample();
            sample.LoadWave(Options.PathSounds + "say1.wav");
            items[0] = sample;
            OnStep();
            sample = new AudioSample();
            sample.LoadOgg(Options.PathSounds + "Click.ogg");
            items[1] = sample;
            OnStep();
        }

        /// <summary>
        /// Получить количество шагов для загрузки
        /// </summary>
        public int GetCountStep() => 2;

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
