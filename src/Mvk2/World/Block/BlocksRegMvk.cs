using Vge.World.Block;

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
            BlocksReg.RegisterBlockClass("Sandstone", new BlockBase());
            BlocksReg.RegisterBlockClass("Dirt", new BlockBase());
            BlocksReg.RegisterBlockClass("Turf", new BlockBase());
            BlocksReg.RegisterBlockClass("Sand", new BlockBase());
            BlocksReg.RegisterBlockClass("Gravel", new BlockBase());
            BlocksReg.RegisterBlockClass("Clay", new BlockBase());
        }
    }
}
