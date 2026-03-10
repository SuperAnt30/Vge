using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Vge.Audio;

namespace Mvk2.Audio
{
    /// <summary>
    /// Статический класс индексов семплов
    /// </summary>
    public static class AudioIndexs
    {
        /// <summary>
        /// Индекс семпла клика
        /// </summary>
        public static int Click;

        /// <summary>
        /// Массив ключей
        /// </summary>
        private static string[] _keys;

        /// <summary>
        /// Массив путей
        /// </summary>
        private static string[] _paths;

        /// <summary>
        /// Инициализация интексов семплов. Нужен так же для 
        /// </summary>
        public static void Initialization(bool isClient)
        {
            List<Sample> samples = new List<Sample>();

            string dig = "Dig" + Path.DirectorySeparatorChar;
            string step = "Step" + Path.DirectorySeparatorChar;

            // Регистрация всех семплов, так же нужны для сервера. Но без пути
            
            // Для GUI клик
            samples.Add(new Sample("Click")); Click = 0;

            samples.Add(new Sample("DigGrass1", dig));
            samples.Add(new Sample("DigGrass2", dig));
            samples.Add(new Sample("DigGrass3", dig));
            samples.Add(new Sample("DigGrass4", dig));
            samples.Add(new Sample("DigSand1", dig));
            samples.Add(new Sample("DigSand2", dig));
            samples.Add(new Sample("DigSand3", dig));
            samples.Add(new Sample("DigSand4", dig));
            samples.Add(new Sample("DigStone1", dig));
            samples.Add(new Sample("DigStone2", dig));
            samples.Add(new Sample("DigStone3", dig));
            samples.Add(new Sample("DigStone4", dig));
            samples.Add(new Sample("DigWood1", dig));
            samples.Add(new Sample("DigWood2", dig));
            samples.Add(new Sample("DigWood3", dig));
            samples.Add(new Sample("DigWood4", dig));
            
            samples.Add(new Sample("StepGrass1", step));
            samples.Add(new Sample("StepGrass2", step));
            samples.Add(new Sample("StepGrass3", step));
            samples.Add(new Sample("StepGrass4", step));
            samples.Add(new Sample("StepSand1", step));
            samples.Add(new Sample("StepSand2", step));
            samples.Add(new Sample("StepSand3", step));
            samples.Add(new Sample("StepSand4", step));
            samples.Add(new Sample("StepStone1", step));
            samples.Add(new Sample("StepStone2", step));
            samples.Add(new Sample("StepStone3", step));
            samples.Add(new Sample("StepStone4", step));
            samples.Add(new Sample("StepWood1", step));
            samples.Add(new Sample("StepWood2", step));
            samples.Add(new Sample("StepWood3", step));
            samples.Add(new Sample("StepWood4", step));

            // Заполняем статические параметры
            int count = samples.Count;
            _keys = new string[count];
            if (isClient) _paths = new string[count];

            for(int i = 0; i < count; i++)
            {
                _keys[i] = samples[i].Key;
                if (isClient) _paths[i] = samples[i].Path;
            }
        }

        /// <summary>
        /// Получить количество всех семплов
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Count() => _keys.Length;

        /// <summary>
        /// Получить путь по индексу
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetPath(int index) => _paths[index];

        /// <summary>
        /// Очиситить массив путей
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ClearPath() => _paths = null;

        /// <summary>
        /// Получить ключ звукового эффекта по псевдониму.
        /// Если не найден равен -1
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetKey(string key)
        {
            for (int i = 0; i < _keys.Length; i++)
            {
                if (key == _keys[i]) return i;
            }
            throw new Exception(SrMvk.GetString(SrMvk.SoundSampleMissing, key));
        }

        /// <summary>
        /// Получить массив ключей
        /// </summary>
        public static int[] GetKeys(params string[] strings)
        {
            int[] result = new int[strings.Length];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = GetKey(strings[i]);
            }
            return result;
        }

        private struct Sample
        {
            public readonly string Key;
            public readonly string Path;
            
            public Sample(string key, string path = "")
            {
                Key = key;
                Path = path + key + ".ogg";
            }
        }
    }
}
