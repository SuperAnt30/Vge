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
        /// Массив ключей
        /// </summary>
        private string[] _keys;

        /// <summary>
        /// Получить количество шагов для загрузки, они же количество семплов
        /// </summary>
        public int GetCountStep() => 3;

        /// <summary>
        /// Загрузка сэмпла
        /// </summary>
        public override void InitializeSample()
        {
            _keys = new string[_items.Length];

            AudioSample sample = new AudioSample();
            sample.LoadWave(Options.PathSounds + "say1.wav");
            _items[2] = sample;
            OnStep();
            _StepOgg(1, "Click", "Click.ogg");
            _StepOgg(0, "DigStone4", "DigStone4.ogg");
        }

        /// <summary>
        /// Задать сепл формата Ogg
        /// </summary>
        /// <param name="index">Порядковый номер в массиве</param>
        /// <param name="key">Псевдоним</param>
        /// <param name="fileName">Имя файла</param>
        private void _StepOgg(int index, string key, string fileName)
        {
            AudioSample sample = new AudioSample();
            sample.LoadOgg(Options.PathSounds + fileName);
            _items[index] = sample;
            _keys[index] = key;
            OnStep();
        }

        /// <summary>
        /// Получить ключ звукового эффекта по псевдониму.
        /// Если не найден равен -1
        /// </summary>
        public int GetIndexByKey(string key)
        {
            for(int i = 0; i < _keys.Length; i++)
            {
                if (key == _keys[i]) return i;
            }
            return -1;
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
