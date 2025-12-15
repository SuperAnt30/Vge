using Mvk2.World.Block.List;
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
        /// Коренная порода
        /// </summary>
        public static BlockBase Bedrock { get; private set; }
        /// <summary>
        /// Известняк
        /// </summary>
        public static BlockBase Limestone { get; private set; }
        /// <summary>
        /// Глина
        /// </summary>
        public static BlockBase Clay { get; private set; }
        /// <summary>
        /// Песок
        /// </summary>
        public static BlockBase Sand { get; private set; }
        /// <summary>
        /// Суглинок
        /// </summary>
        public static BlockBase Loam { get; private set; }
        /// <summary>
        /// Чернозём
        /// </summary>
        public static BlockBase Humus { get; private set; }
        /// <summary>
        /// Дёрн
        /// </summary>
        public static BlockBase Turf { get; private set; }
        /// <summary>
        /// Дёрн на суглинке
        /// </summary>
        public static BlockBase TurfLoam { get; private set; }
        /// <summary>
        /// Гравий
        /// </summary>
        public static BlockBase Gravel { get; private set; }
        /// <summary>
        /// Угольная руда
        /// </summary>
        public static BlockBase OreCoal { get; private set; }
        /// <summary>
        /// Железная руда
        /// </summary>
        public static BlockBase OreIron { get; private set; }
        /// <summary>
        /// Золотая руда
        /// </summary>
        public static BlockBase OreGold { get; private set; }
        /// <summary>
        /// Руда серы
        /// </summary>
        public static BlockBase OreSulfur { get; private set; }
        /// <summary>
        /// Руда алмаза
        /// </summary>
        public static BlockBase OreDiamond { get; private set; }
        /// <summary>
        /// Руда изумруда
        /// </summary>
        public static BlockBase OreEmerald { get; private set; }
        /// <summary>
        /// Руда рубина
        /// </summary>
        public static BlockBase OreRuby { get; private set; }
        /// <summary>
        /// Руда сапфира
        /// </summary>
        public static BlockBase OreSapphire { get; private set; }

        /// <summary>
        /// Камень
        /// </summary>
        public static BlockBase Stone { get; private set; }
        /// <summary>
        /// Булыжника
        /// </summary>
        public static BlockBase Cobblestone { get; private set; }
        /// <summary>
        /// Блок травы
        /// </summary>
        public static BlockBase Grass { get; private set; }
        /// <summary>
        /// Цветок одуванчик
        /// </summary>
        public static BlockBase FlowerDandelion { get; private set; }
        /// <summary>
        /// Цветок клевер белый
        /// </summary>
        public static BlockBase FlowerClover { get; private set; }
        /// <summary>
        /// Блок высокой травы
        /// </summary>
        public static BlockBase TallGrass { get; private set; }
        /// <summary>
        /// Блок тины
        /// </summary>
        public static BlockBase Tina { get; private set; }
        

        public static BlockBase Granite { get; private set; }
        public static BlockBase Glass { get; private set; }
        public static BlockAlpha GlassRed { get; private set; }
        public static BlockAlpha GlassGreen { get; private set; }
        public static BlockAlpha GlassBlue { get; private set; }
        public static BlockAlpha GlassPurple { get; private set; }
        
        public static BlockBase Brol { get; private set; }

        public static void Initialization()
        {
            // Отладочный
            BlocksReg.RegisterBlockClass("Debug", new BlockDebug());
            Water = BlocksReg.RegisterBlockClassLiquid("Water", true);
            Bedrock = BlocksReg.RegisterBlockClass("Bedrock");
            Limestone = BlocksReg.RegisterBlockClass("Limestone");
            Granite = BlocksReg.RegisterBlockClass("Granite");
            Clay = BlocksReg.RegisterBlockClass("Clay");
            Sand = BlocksReg.RegisterBlockClass("Sand");
            Loam = BlocksReg.RegisterBlockClass("Loam");
            Humus = BlocksReg.RegisterBlockClass("Humus");
            Turf = BlocksReg.RegisterBlockClass("Turf");
            TurfLoam = BlocksReg.RegisterBlockClass("TurfLoam");
            Gravel = BlocksReg.RegisterBlockClass("Gravel");
            OreCoal = BlocksReg.RegisterBlockClass("OreCoal");
            OreIron = BlocksReg.RegisterBlockClass("OreIron");
            OreGold = BlocksReg.RegisterBlockClass("OreGold");
            OreSulfur = BlocksReg.RegisterBlockClass("OreSulfur");
            OreDiamond = BlocksReg.RegisterBlockClass("OreDiamond");
            OreEmerald = BlocksReg.RegisterBlockClass("OreEmerald");
            OreRuby = BlocksReg.RegisterBlockClass("OreRuby");
            OreSapphire = BlocksReg.RegisterBlockClass("OreSapphire");

            BlocksReg.RegisterBlockClass("Grass", Grass = new BlockGrass());
            BlocksReg.RegisterBlockClass("TallGrass", TallGrass = new BlockTallGrass());

            Tina = BlocksReg.RegisterBlockClass("Tina");


            FlowerDandelion = BlocksReg.RegisterBlockClass("FlowerDandelion");
            FlowerClover = BlocksReg.RegisterBlockClass("FlowerClover");

            Stone = BlocksReg.RegisterBlockClass("Stone");
            Cobblestone= BlocksReg.RegisterBlockClass("Cobblestone");
            
            Glass = BlocksReg.RegisterBlockClass("Glass");
            GlassRed = BlocksReg.RegisterBlockClassAlpha("GlassRed");
            GlassGreen = BlocksReg.RegisterBlockClassAlpha("GlassGreen");
            GlassBlue = BlocksReg.RegisterBlockClassAlpha("GlassBlue");
            GlassPurple = BlocksReg.RegisterBlockClassAlpha("GlassPurple");

            
            
            Lava = BlocksReg.RegisterBlockClassLiquid("Lava", false);
            Brol = BlocksReg.RegisterBlockClass("Brol");
            
            //for (int i = 0; i < 500; i++)
            //{
            //    BlocksReg.RegisterBlockClass("Clay" + i, new BlockUniSolid(70));
            //}
        }
    }
}
