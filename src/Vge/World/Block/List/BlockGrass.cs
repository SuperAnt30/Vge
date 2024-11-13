using Vge.Renderer.World;
using Vge.Util;
using WinGL.Util;

namespace Vge.World.Block.List
{
    /// <summary>
    /// Блок травы
    /// </summary>
    public class BlockGrass : BlockBase
    {
        protected bool _biomeColor;

        public BlockGrass(int numberTexture)
        {
            //Material = Materials.GetMaterialCache(EnumMaterial.Sapling);
            //IsDamagedBlockBlack = true;
            Particle = numberTexture;

            IsUnique = true;
            FullBlock = false;

            //BlocksNotSame = false;
            AllSideForcibly = true;
            LightOpacity = 0;
            АmbientOcclusion = false;
            Shadow = false;
            //BothSides = bothSides;

            IsCollidable = false;
            //Combustibility = true;
            //IgniteOddsSunbathing = 5;
            //BurnOdds = 5;// 100;
            //Resistance = 0f;
            //samplesPut = samplesBreak = new AssetsSample[] { AssetsSample.DigGrass1, AssetsSample.DigGrass2, AssetsSample.DigGrass3, AssetsSample.DigGrass4 };

            float[] offsetMetX = new float[] { 0, .1875f, -.1875f, .1875f, -.1875f };
            float[] offsetMetZ = new float[] { 0, .1875f, .1875f, -.1875f, -.1875f };

            _quads = new QuadSide[5][];
            for (int i = 0; i < 5; i++)
            {
                _quads[i] = new QuadSide[] {
                    new QuadSide((byte)(_biomeColor ? 1 : 0)).SetTexture(Particle)
                    .SetSide(Pole.South, true, -2, 0, 8, 18, 16, 8).SetRotate(Glm.Pi45)
                    .SetTranslate(offsetMetX[i], 0, offsetMetZ[i]),//.Wind(),
                    new QuadSide((byte)(_biomeColor ? 1 : 0)).SetTexture(Particle)
                    .SetSide(Pole.East, true, 8, 0, -2, 8, 16, 18).SetRotate(Glm.Pi45)
                    .SetTranslate(offsetMetX[i], 0, offsetMetZ[i]),//.Wind()

                    new QuadSide((byte)(_biomeColor ? 1 : 0)).SetTexture(Particle, 16, 0, 0, 16)
                    .SetSide(Pole.North, true, -2, 0, 8, 18, 16, 8).SetRotate(Glm.Pi45)
                    .SetTranslate(offsetMetX[i], 0, offsetMetZ[i]),//.Wind(),
                    new QuadSide((byte)(_biomeColor ? 1 : 0)).SetTexture(Particle, 16, 0, 0, 16)
                    .SetSide(Pole.West, true, 8, 0, -2, 8, 16, 18).SetRotate(Glm.Pi45)
                    .SetTranslate(offsetMetX[i], 0, offsetMetZ[i])//.Wind()
                };
            }
        }

        /// <summary>
        /// Инициализация объекта рендера для блока
        /// </summary>
        protected override void _InitBlockRender()
            => BlockRender = Gi.BlockUniqueRendFull;
    }
}
