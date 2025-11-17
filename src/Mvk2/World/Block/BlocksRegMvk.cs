using Vge.World.Block;
using Vge.World.Block.List;

namespace Mvk2.World.Block
{
    /// <summary>
    /// Регистрация блоков для Малювеки 2
    /// </summary>
    public sealed class BlocksRegMvk
    {
        public static BlockLiquid Water { get; private set; }
        public static BlockLiquid Lava { get; private set; }

        /// <summary>
        /// Камень
        /// </summary>
        public static BlockBase Stone { get; private set; }
        /// <summary>
        /// Булыжника
        /// </summary>
        public static BlockBase Cobblestone { get; private set; }
        public static BlockBase Limestone { get; private set; }
        public static BlockBase Granite { get; private set; }
        public static BlockBase Glass { get; private set; }
        public static BlockAlpha GlassRed { get; private set; }
        public static BlockAlpha GlassGreen { get; private set; }
        public static BlockAlpha GlassBlue { get; private set; }
        public static BlockAlpha GlassPurple { get; private set; }
        public static BlockBase FlowerClover { get; private set; }
        
        public static BlockBase Brol { get; private set; }

        public static void Initialization()
        {
            // Отладочный
            BlocksReg.RegisterBlockClass("Debug", new BlockDebug());

            Stone = BlocksReg.RegisterBlockClass("Stone");
            Cobblestone= BlocksReg.RegisterBlockClass("Cobblestone");
            Limestone = BlocksReg.RegisterBlockClass("Limestone");
            Granite = BlocksReg.RegisterBlockClass("Granite");
            Glass = BlocksReg.RegisterBlockClass("Glass");
            GlassRed = BlocksReg.RegisterBlockClassAlpha("GlassRed");
            GlassGreen = BlocksReg.RegisterBlockClassAlpha("GlassGreen");
            GlassBlue = BlocksReg.RegisterBlockClassAlpha("GlassBlue");
            GlassPurple = BlocksReg.RegisterBlockClassAlpha("GlassPurple");

            FlowerClover = BlocksReg.RegisterBlockClass("FlowerClover"); // 195
            Water = BlocksReg.RegisterBlockClassLiquid("Water", true);
            Lava = BlocksReg.RegisterBlockClassLiquid("Lava", false);
            Brol = BlocksReg.RegisterBlockClass("Brol");
            
            //for (int i = 0; i < 500; i++)
            //{
            //    BlocksReg.RegisterBlockClass("Clay" + i, new BlockUniSolid(70));
            //}
        }
    }
}
