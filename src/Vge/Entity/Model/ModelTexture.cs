using Vge.Util;

namespace Vge.Entity.Model
{
    /// <summary>
    /// Текстура модели
    /// </summary>
    public class ModelTexture
    {
        /// <summary>
        /// картинка текстуры
        /// </summary>
        public readonly BufferedImage Image;
        /// <summary>
        /// Имя текстуры
        /// </summary>
        public readonly string Name = "";
        /// <summary>
        /// Текстура для слоя
        /// </summary>
        public readonly bool IsLayer;
        /// <summary>
        /// Используется
        /// </summary>
        public bool Used;

        /// <summary>
        /// Создать текстуру, если имя не пустое, то это слой
        /// </summary>
        public ModelTexture(string name, BufferedImage image)
        {
            Name = name;
            Image = image;
            IsLayer = name != "";
        }

        public override string ToString() => (IsLayer ? "Layer " : "") + Name;
    }
}
