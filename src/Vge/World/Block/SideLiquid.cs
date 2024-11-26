namespace Vge.World.Block
{
    /// <summary>
    /// Сторона жидкого блока
    /// </summary>
    public class SideLiquid
    {
        /// <summary>
        /// Смещение текстуры в карте
        /// </summary>
        public readonly float U;
        /// <summary>
        /// Смещение текстуры в карте
        /// </summary>
        public readonly float V;
        /// <summary>
        /// С какой стороны
        /// </summary>
        public readonly int Side;
        /// <summary>
        /// Боковое затемнение
        /// </summary>
        public float LightPole;
        /// <summary>
        /// Для анимации блока, указывается количество кадров в игровом времени (50 мс),
        /// 0 - нет анимации
        /// </summary>
        public byte AnimationFrame;
        /// <summary>
        /// Для анимации блока, указывается пауза между кадрами в игровом времени (50 мс),
        /// 0 или 1 - нет задержки, каждый такт игры смена кадра
        /// </summary>
        public byte AnimationPause;
        /// <summary>
        /// Флаг имеется ли ветер, для уникальных блоков: листвы, травы и подобного
        /// 0 - нет, 1 - вверхние точки, 2 - нижние
        /// </summary>
        public byte Wind;
        /// <summary>
        /// Цвет биома, где 0 - нет цвета, 1 - трава, 2 - листа, 3 - вода
        /// </summary>
        public readonly byte TypeColor;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pole">индекс стороны</param>
        /// <param name="shade">Отсутствие оттенка, т.е. зависит от стороны света, если true оттенка нет</param>
        /// <param name="typeColor">Задать цвет биома, где 0 - нет цвета, 1 - трава, 2 - листа, 3 - вода</param>
        public SideLiquid(int pole, bool shade, int numberTexture, byte typeColor)
        {
            Side = pole;
            LightPole = shade ? 0f : 1f - Gi.LightPoles[Side];
            U = (numberTexture % Ce.TextureAtlasBlockCount) * Ce.ShaderAnimOffset;
            V = numberTexture / Ce.TextureAtlasBlockCount * Ce.ShaderAnimOffset;
            TypeColor = typeColor;
            Side = 0;
        }

        /// <summary>
        /// Задать анимацию
        /// </summary>
        /// <param name="frame">Количество кадров в игровом времени</param>
        /// <param name="pause">Пауза между кадрами в игровом времени</param>
        public SideLiquid SetAnimal(byte frame, byte pause)
        {
            AnimationFrame = frame;
            AnimationPause = pause;
            return this;
        }

        /// <summary>
        /// Задать стороне ветер, 0 - нет движения 1 - (по умолчанию) вверх двигается низ нет, 2 - низ двигается вверх нет, 3 - двигается всё
        /// </summary>
        public SideLiquid SetWind(byte wind = 1)
        {
            Wind = wind;
            return this;
        }

        /// <summary>
        /// Имеется ли у блока смена цвета воды от биома
        /// </summary>
        public bool IsColorWater() => TypeColor == 3;
    }
}
