﻿using Vge.Renderer;

namespace Mvk2.Renderer
{
    /// <summary>
    /// Класс переменных индексов текстур для малювек
    /// </summary>
    public class TextureIndexMvk : TextureIndex
    {
        /// <summary>
        /// Текстура окна настроек
        /// </summary>
        public uint Options;
        /// <summary>
        /// Текстура чат окна
        /// </summary>
        public uint Chat;
        /// <summary>
        /// Текстура Heads-Up Display
        /// </summary>
        public uint Hud;
        /// <summary>
        /// Текстура Inventory
        /// </summary>
        public uint Inventory;
        /// <summary>
        /// Текстура контейнера хранилища Storage
        /// </summary>
        public uint ConteinerStorage;
        
    }


    /// <summary>
    /// Перечисление разных ключей текстур для малювек
    /// </summary>
    public enum EnumTextureMvk
    {
        /// <summary>
        /// Мелкий шрифт (8 пиксель)
        /// </summary>
        FontSmall,
        /// <summary>
        /// Крупный шрифт (16 пиксель)
        /// </summary>
        FontLarge,
        /// <summary>
        /// Окно настроек
        /// </summary>
        Options,
        /// <summary>
        /// Чат окно
        /// </summary>
        Chat,
        /// <summary>
        /// Heads-Up Display
        /// </summary>
        Hud,
        /// <summary>
        /// Текстура Inventory
        /// </summary>
        Inventory,
        /// <summary>
        /// Текстура контейнера хранилища Storage
        /// </summary>
        ConteinerStorage
    }

}
