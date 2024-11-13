using Vge.World.Block;
using Vge.World.Block.List;

namespace Mvk2.World.Block
{
    /// <summary>
    /// Регистрация блоков для Малювеки 2
    /// </summary>
    public sealed class BlocksRegMvk
    {
        public static void Initialization()
        {
            BlocksReg.RegisterBlockClass("Stone", new BlockUniSolid(2));
            BlocksReg.RegisterBlockClass("Cobblestone", new BlockUniSolid(3));
            BlocksReg.RegisterBlockClass("Limestone", new BlockUniSolid(4));
            BlocksReg.RegisterBlockClass("Granite", new BlockUniSolid(5));
            BlocksReg.RegisterBlockClass("Sandstone", new BlockUniSolid(8));
            BlocksReg.RegisterBlockClass("Dirt", new BlockUniSolid(64));
            BlocksReg.RegisterBlockClass("Turf", new BlockUniSolid(64));
            BlocksReg.RegisterBlockClass("Sand", new BlockUniSolid(68));
            BlocksReg.RegisterBlockClass("Gravel", new BlockUniSolid(69));
            BlocksReg.RegisterBlockClass("Clay", new BlockUniSolid(70));
            BlocksReg.RegisterBlockClass("GlassRed", new BlockUniTranslucent(322, .8f, .4f, .2f));
            BlocksReg.RegisterBlockClass("GlassGreen", new BlockUniTranslucent(322, .2f, .9f, .2f));
            BlocksReg.RegisterBlockClass("GlassBlue", new BlockUniTranslucent(322, .7f, .8f, 1f));
            BlocksReg.RegisterBlockClass("GlassPurple", new BlockUniTranslucent(322, 1f, .5f, 1f));
            BlocksReg.RegisterBlockClass("Grass", new BlockGrass(197)); // 195
        }
    }
}
