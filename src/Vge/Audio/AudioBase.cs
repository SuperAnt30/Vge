using System;
using Vge.Util;

namespace Vge.Audio
{
    /// <summary>
    /// Базовый класс звуков
    /// </summary>
    public class AudioBase
    {
        /// <summary>
        /// Массив всех семплов
        /// </summary>
        protected AudioSample[] _items;
        /// <summary>
        /// Объект источников звука
        /// </summary>
        private readonly AudioSources _sources = new AudioSources();
        /// <summary>
        /// Строка для дэбага сколько источников и занятых
        /// </summary>
        public string StrDebug { get; private set; } = "";

        /// <summary>
        /// Инициализация звукового драйвера
        /// </summary>
        /// <param name="count">указываем количество звуков, ключи начинаются с 0</param>
        public void Initialize(int count, int monoSourcesLimit, int stereoSourcesLimit)
        {
            // Инициализация звука
            IntPtr pDevice = Al.alcOpenDevice(null);
            //IntPtr pContext = Al.alcCreateContext(pDevice, null);
            IntPtr pContext = Al.alcCreateContext(pDevice, new int[]
                { Al.ALC_MONO_SOURCES, monoSourcesLimit, 
                    Al.ALC_STEREO_SOURCES, stereoSourcesLimit });
            Al.alcMakeContextCurrent(pContext);

            // Инициализация источников звука
            _sources.Initialize();

            // Указываем количество звуков
            _items = new AudioSample[count];
        }

        /// <summary>
        /// Загрузка сэмпла
        /// </summary>
        public void InitializeSample()
        {
            AudioSample sample;
            int count = AudioIndexs.Count();
            for (int i = 0; i < count; i++)
            {
                sample = new AudioSample();
                sample.LoadOgg(Options.PathSounds + AudioIndexs.GetPath(i));
                _items[i] = sample;
                OnStep();
            }

            AudioIndexs.ClearPath();
        }

        /// <summary>
        /// Такт
        /// </summary>
        public void Tick()
        {
            _sources.AudioTick();
            StrDebug = string.Format("{0}/{1}", _sources.CountProcessing, _sources.CountAll);
        }

        /// <summary>
        /// Проиграть звук
        /// </summary>
        public void PlaySound(int key, float posX, float posY, float posZ, float volume, float pitch)
        {
            if (Options.SoundVolume > 0)
            {
                AudioSample sample = _items[key];
                if (sample != null && sample.Size > 0)
                {
                    AudioSource source = _sources.GetAudio();
                    if (source != null)
                    {
                        source.Sample(sample);
                        source.Play(posX, posY, posZ, volume * Options.SoundVolumeFloat, pitch);
                    }
                }
            }
        }

        /// <summary>
        /// Очистить весь буфер
        /// </summary>
        public void Clear()
        {
            if (_items != null)
            {
                foreach (AudioSample audio in _items)
                {
                    audio?.Clear();
                }
            }
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
