using System.Runtime.CompilerServices;
using WinGL.Util;

namespace Vge.Renderer.World
{
    /// <summary>
    /// Объект отвечает за цвет биома
    /// </summary>
    public class ColorsBiom
    {
        /// <summary>
        /// Массив цвета травы для каждого биомов по ID
        /// </summary>
        private Vector3[] _grassColors;

        /// <summary>
        /// Массив цвета воды для каждого биомов по ID
        /// </summary>
        private Vector3[] _waterColors;

        public ColorsBiom() { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CreateGrass(Vector3[] colors) => _grassColors = colors;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CreateWater(Vector3[] colors) => _waterColors = colors;

        /// <summary>
        /// Получить цвет травы по ID биома
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 Grass(byte idBiome) => _grassColors[idBiome];

        /// <summary>
        /// Получить цвет воды по ID биома
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 Water(byte idBiome) => _waterColors[idBiome];
    }
}
