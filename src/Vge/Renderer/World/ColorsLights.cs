using WinGL.Util;

namespace Vge.Renderer.World
{
    /// <summary>
    /// Структура для вершин цвета и освещения
    /// </summary>
    public class ColorsLights
    {
        public readonly byte[] ColorR = new byte[4];
        public readonly byte[] ColorG = new byte[4];
        public readonly byte[] ColorB = new byte[4];
        public readonly byte[] Light = new byte[4];

        public void InitColorsLights(Vector3 color1, Vector3 color2, Vector3 color3, Vector3 color4,
            byte light1, byte light2, byte light3, byte light4)
        {
            ColorR[0] = (byte)(color1.X * 255);
            ColorR[1] = (byte)(color2.X * 255);
            ColorR[2] = (byte)(color3.X * 255);
            ColorR[3] = (byte)(color4.X * 255);

            ColorG[0] = (byte)(color1.Y * 255);
            ColorG[1] = (byte)(color2.Y * 255);
            ColorG[2] = (byte)(color3.Y * 255);
            ColorG[3] = (byte)(color4.Y * 255);

            ColorB[0] = (byte)(color1.Z * 255);
            ColorB[1] = (byte)(color2.Z * 255);
            ColorB[2] = (byte)(color3.Z * 255);
            ColorB[3] = (byte)(color4.Z * 255);

            Light[0] = light1;
            Light[1] = light2;
            Light[2] = light3;
            Light[3] = light4;
        }
    }
}
