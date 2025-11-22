using Vge.World.Gen.Layer;

namespace Mvk2.World.Gen.Layer
{
    /// <summary>
    /// По биому выставляем коэффициент высоты
    /// </summary>
    public class GenLayerBiomeHeight : GenLayer
    {
        /// <summary>
        /// Уровень земель над водой
        /// </summary>
        private const int _levelEarth = 51;

        /// <summary>
        /// Масссив высоты для каждого биома /2
        /// 48 - уровень моря
        /// </summary>
        private static readonly int[] _levels = new int[] {
            11, // Sea
            37, // River
            _levelEarth, // Plain
            _levelEarth, // Desert
            47, // Beach
            _levelEarth, // MixedForest
            _levelEarth, // ConiferousForest
            _levelEarth, // BirchForest
            _levelEarth, // Tropics
            47, // Swamp
            73, // Mountains 96
            63  // MountainsDesert 72
        };

        public GenLayerBiomeHeight(GenLayer parent) => _parent = parent;

        public override int[] GetInts(int areaX, int areaZ, int width, int height)
        {
            int[] arParent = _parent.GetInts(areaX, areaZ, width, height);
            int count = width * height;
            int[] ar = new int[count];

            for (int i = 0; i < count; i++)
            {
                ar[i] = _levels[arParent[i]];
            }
            return ar;
        }
    }
}
