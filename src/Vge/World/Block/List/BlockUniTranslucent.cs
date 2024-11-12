using Vge.Renderer.World;
using Vge.Util;
using WinGL.Util;

namespace Vge.World.Block.List
{
    /// <summary>
    /// Универсальный объект полупрозрачных блоков
    /// </summary>
    public class BlockUniTranslucent : BlockBase
    {
        /// <summary>
        /// Универсальный объект полупрозрачных блоков
        /// </summary>
        /// <param name="numberTexture">Номер текстуры</param>
        /// <param name="red">Красный 0.0-1.0</param>
        /// <param name="green">Зелёный 0.0-1.0</param>
        /// <param name="blue">Синий 0.0-1.0</param>
        public BlockUniTranslucent(int numberTexture, float red, float green, float blue)
        {
            Particle = numberTexture;
            АmbientOcclusion = false;
            BlocksNotSame = false;
            AllSideForcibly = true;
            UseNeighborBrightness = true;
            LightOpacity = 2;
            Translucent = true;
            Color = new Vector3(red, green, blue);
            //IsDamagedBlockBlack = false;

            _quads = new QuadSide[][] { new QuadSide[] {
                new QuadSide(4).SetTexture(numberTexture).SetSide(Pole.Up),
                new QuadSide(4).SetTexture(numberTexture).SetSide(Pole.Down),
                new QuadSide(4).SetTexture(numberTexture).SetSide(Pole.East),
                new QuadSide(4).SetTexture(numberTexture).SetSide(Pole.West),
                new QuadSide(4).SetTexture(numberTexture).SetSide(Pole.North),
                new QuadSide(4).SetTexture(numberTexture).SetSide(Pole.South)
            } };
        }

        /// <summary>
        /// Инициализация объекта рендера для блока
        /// </summary>
        protected override void _InitBlockRender()
            => BlockRender = Gi.BlockAlphaRendFull;
    }
}
