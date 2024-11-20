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
            BlocksReg.RegisterBlockClass("Stone", new BlockBase());
            BlocksReg.RegisterBlockClass("Cobblestone", new BlockBase());
            BlocksReg.RegisterBlockClass("Limestone", new BlockBase());
            BlocksReg.RegisterBlockClass("Granite", new BlockBase());
            BlocksReg.RegisterBlockClass("Sandstone", new BlockUniSolid(8));
            BlocksReg.RegisterBlockClass("Dirt", new BlockUniSolid(64));
            BlocksReg.RegisterBlockClass("Turf", new BlockUniSolid(64));
            BlocksReg.RegisterBlockClass("Sand", new BlockUniSolid(68));
            BlocksReg.RegisterBlockClass("Gravel", new BlockUniSolid(69));
            BlocksReg.RegisterBlockClass("Clay", new BlockUniSolid(70));
            BlocksReg.RegisterBlockClass("GlassRed", new BlockUniTranslucent());
            BlocksReg.RegisterBlockClass("GlassGreen", new BlockUniTranslucent());
            BlocksReg.RegisterBlockClass("GlassBlue", new BlockUniTranslucent());
            BlocksReg.RegisterBlockClass("GlassPurple", new BlockUniTranslucent());
            BlocksReg.RegisterBlockClass("Grass", new BlockBase()); // 195

            //for (int i = 0; i < 500; i++)
            //{
            //    BlocksReg.RegisterBlockClass("Clay" + i, new BlockUniSolid(70));
            //}
        }
    }
}
