using System.Runtime.CompilerServices;

namespace Vge.World.Block.List
{
    /// <summary>
    /// Блок травы
    /// </summary>
    public class BlockGrass : BlockBase
    {
        protected bool _biomeColor;

        /// <summary>
        /// Массив сторон прямоугольных форм для рендера
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override QuadSide[] GetQuads(int met, int xb, int zb)
        {
            int i = (xb + zb) & 7;
            if (i > 4) i -= 4;
            return _quads[i];
        }

        /*

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
                    .SetTranslate(offsetMetX[i], 0, offsetMetZ[i]).SetWind().SetSharpness(),
                    new QuadSide((byte)(_biomeColor ? 1 : 0)).SetTexture(Particle)
                    .SetSide(Pole.East, true, 8, 0, -2, 8, 16, 18).SetRotate(Glm.Pi45)
                    .SetTranslate(offsetMetX[i], 0, offsetMetZ[i]).SetWind().SetSharpness(),

                    new QuadSide((byte)(_biomeColor ? 1 : 0)).SetTexture(Particle, 16, 0, 0, 16)
                    .SetSide(Pole.North, true, -2, 0, 8, 18, 16, 8).SetRotate(Glm.Pi45)
                    .SetTranslate(offsetMetX[i], 0, offsetMetZ[i]).SetWind().SetSharpness(),
                    new QuadSide((byte)(_biomeColor ? 1 : 0)).SetTexture(Particle, 16, 0, 0, 16)
                    .SetSide(Pole.West, true, 8, 0, -2, 8, 16, 18).SetRotate(Glm.Pi45)
                    .SetTranslate(offsetMetX[i], 0, offsetMetZ[i]).SetWind().SetSharpness(),
                };
            }
        }
        */
    }
}
