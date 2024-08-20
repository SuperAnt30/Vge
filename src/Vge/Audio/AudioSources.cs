using System;
using System.Collections.Generic;

namespace Vge.Audio
{
    /// <summary>
    /// Объект источкиков звука
    /// </summary>
    public class AudioSources
    {
        /// <summary>
        /// Общее количество источников
        /// </summary>
        public int CountAll { get; private set; } = 0;
        /// <summary>
        /// Количество источников воспроизводившие звуки
        /// </summary>
        public int CountProcessing { get; private set; } = 0;

        /// <summary>
        /// Список всех источников звуков
        /// </summary>
        private AudioSource[] sources;

        /// <summary>
        /// Инициализировать и определеить количество источников
        /// </summary>
        public void Initialize()
        {
            IntPtr device = Al.alcOpenDevice("");
            if (device == IntPtr.Zero)
            {
                throw new Exception("Библиотека звука OpenAL не смогла инициализироваться, скорее всего файл OpenAL64.dll не подходит.");
            }
            List<AudioSource> list = new List<AudioSource>();
            bool error = false;
            int count = 10000;
            while (!error && count > 0)
            {
                count--;
                AudioSource audio = new AudioSource();
                if (audio.Initialized())
                {
                    list.Add(audio);
                }
                else
                {
                    error = true;
                }
            }
            if (count <= 0)
            {
                throw new Exception("Библиотека звука OpenAL собрала больше 10000 каналов, это подозрительно!");
            }
            sources = list.ToArray();
            CountAll = list.Count;
        }

        /// <summary>
        /// Получить свободный источник
        /// </summary>
        public AudioSource GetAudio()
        {
            foreach (AudioSource audio in sources)
            {
                if (!audio.Processing)
                {
                    return audio;
                }
            }
            return null;
        }

        /// <summary>
        /// Тактовая проверка источников
        /// </summary>
        public void AudioTick()
        {
            int count = 0;
            foreach (AudioSource audio in sources)
            {
                if (audio.CheckProcessing())
                {
                    count++;
                }
            }
            CountProcessing = count;
        }
    }
}
