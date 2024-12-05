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
            BlocksReg.RegisterBlockClass("Glass", new BlockBase());
            BlocksReg.RegisterBlockClass("GlassRed", new BlockUniTranslucent());
            BlocksReg.RegisterBlockClass("GlassGreen", new BlockUniTranslucent());
            BlocksReg.RegisterBlockClass("GlassBlue", new BlockUniTranslucent());
            BlocksReg.RegisterBlockClass("GlassPurple", new BlockUniTranslucent());
            
            BlocksReg.RegisterBlockClass("FlowerClover", new BlockBase()); // 195
            BlocksReg.RegisterBlockClass("Water", new BlockLiquid(true));
            BlocksReg.RegisterBlockClass("Lava", new BlockLiquid(false));
            BlocksReg.RegisterBlockClass("Brol", new BlockBase());

            //for (int i = 0; i < 500; i++)
            //{
            //    BlocksReg.RegisterBlockClass("Clay" + i, new BlockUniSolid(70));
            //}
        }
    }
}
