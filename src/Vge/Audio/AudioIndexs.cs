using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Vge.Audio
{
    /// <summary>
    /// Статический класс индексов семплов
    /// </summary>
    public static class AudioIndexs
    {
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
        public static void Initialization(bool isClient, List<SampleReg> samples)
        {
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
            throw new Exception(Sr.GetString(Sr.SoundSampleMissing, key));
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
    }
}
