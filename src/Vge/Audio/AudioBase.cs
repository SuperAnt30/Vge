using System;
using Vge.Util;

namespace Vge.Audio
{
    /// <summary>
    /// Базовый класс звуков
    /// </summary>
    public abstract class AudioBase
    {
        /// <summary>
        /// Массив всех семплов
        /// </summary>
        protected AudioSample[] items;
        /// <summary>
        /// Объект источников звука
        /// </summary>
        private readonly AudioSources sources = new AudioSources();
        /// <summary>
        /// Строка для дэбага сколько источников и занятых
        /// </summary>
        public string StrDebug { get; private set; }

        /// <summary>
        /// Инициализация звукового драйвера
        /// </summary>
        /// <param name="count">указываем количество звуков, ключи начинаются с 0</param>
        public void Initialize(int count)
        {
            // Инициализация звука
            IntPtr pDevice = Al.alcOpenDevice(null);
            IntPtr pContext = Al.alcCreateContext(pDevice, null);
            Al.alcMakeContextCurrent(pContext);

            // Инициализация источников звука
            sources.Initialize();

            // Указываем количество звуков
            items = new AudioSample[count];
        }

        /// <summary>
        /// Загрузка сэмпла
        /// </summary>
        public virtual void InitializeSample() { }

        /// <summary>
        /// Такт
        /// </summary>
        public void Tick()
        {
            sources.AudioTick();
            StrDebug = string.Format("{0}/{1}", sources.CountProcessing, sources.CountAll);
        }

        /// <summary>
        /// Проиграть звук
        /// </summary>
        public void PlaySound(int key, float posX, float posY, float posZ, float volume, float pitch)
        {
            if (Options.SoundVolume > 0)
            {
                AudioSample sample = items[key];
                if (sample != null && sample.Size > 0)
                {
                    AudioSource source = sources.GetAudio();
                    if (source != null)
                    {
                        source.Sample(sample);
                        source.Play(posX, posY, posZ, volume * Options.SoundVolumeFloat, pitch);
                    }
                }
            }
        }
    }
}
