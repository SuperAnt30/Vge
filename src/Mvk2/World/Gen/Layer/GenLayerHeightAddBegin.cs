using Vge.World.Gen.Layer;

namespace Mvk2.World.Gen.Layer
{
    /// <summary>
    /// Объект по добавлении высот (холмов и луж)
    /// </summary>
    public class GenLayerHeightAddBegin : GenLayerHeightAddParam
    {
        public GenLayerHeightAddBegin(long baseSeed, GenLayer parent) : base(baseSeed, parent) { }

        protected override int _GetParam(int param)
        {
            if (param > 48 && _NextInt(6) == 0)
            {
                // Добавляем холм
                param += _NextInt(20) + 1;
            }
            else if (param > 50 && _NextInt(6) == 0)
            {
                // Добавляем лужу
                param -= _NextInt(4) + 1;
            }
            return param;
        }
    }
}
