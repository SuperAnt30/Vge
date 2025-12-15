using Vge.World.Gen.Layer;

namespace Mvk2.World.Gen.Layer
{
    /// <summary>
    /// Объект по добавлении высот (ущелены в море и не большие неровности)
    /// </summary>
    public class GenLayerHeightAddSea : GenLayerHeightAddParam
    {
        public GenLayerHeightAddSea(long baseSeed, GenLayer parent) : base(baseSeed, parent) { }

        protected override int _GetParam(int param)
        {
            if (param < 29 && _NextInt(6) == 0)
            {
                // Делаем ущелены в море
                param = _NextInt(4) + 3;
            }
            // Добавляем не большие неровности
            //param += _NextInt(4) - 2;
            param += _NextInt(3) - 1;
            return param;
        }
    }
}
