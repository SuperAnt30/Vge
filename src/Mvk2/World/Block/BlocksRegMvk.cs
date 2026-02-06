using Mvk2.World.Block.List;
using Mvk2.World.Gen.Feature;
using Vge.World.Block;
using Vge.World.Block.List;

namespace Mvk2.World.Block
{
    /// <summary>
    /// Регистрация блоков для Малювеки 2
    /// </summary>
    public sealed class BlocksRegMvk
    {
        public static BlockDebug Debug { get; private set; }

        #region Жидкие блоки

        public static BlockLiquid Water { get; private set; }
        public static BlockLiquid Lava { get; private set; }

        #endregion

        #region Твёрдые каменные блоки

        /// <summary>
        /// Коренная порода
        /// </summary>
        public static BlockBase Bedrock { get; private set; }
        /// <summary>
        /// Известняк
        /// </summary>
        public static BlockBase Limestone { get; private set; }
        /// <summary>
        /// Камень
        /// </summary>
        public static BlockBase Stone { get; private set; }
        /// <summary>
        /// Булыжника
        /// </summary>
        public static BlockBase Cobblestone { get; private set; }
        /// <summary>
        /// Гранит
        /// </summary>
        public static BlockBase Granite { get; private set; }

        #endregion

        #region Сыпучие блоки

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

        #endregion

        #region Руды

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

        #endregion

        #region Растительность

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

        #endregion

        #region Древесина

        /// <summary>
        /// Блок сухого саженца
        /// </summary>
        public static BlockBase SaplingDry { get; private set; }
        /// <summary>
        /// Блок корня древесины
        /// </summary>
        public static BlockBase TreeRoot { get; private set; }

        /// <summary>
        /// Блок бревна берёзы
        /// </summary>
        public static BlockTree LogBirch { get; private set; }
        /// <summary>
        /// Блок ветки берёзы
        /// </summary>
        public static BlockTree BranchBirch { get; private set; }
        /// <summary>
        /// Блок листвы берёзы
        /// </summary>
        public static BlockTree LeavesBirch { get; private set; }
        /// <summary>
        /// Блок саженца берёзы
        /// </summary>
        public static BlockTree SaplingBirch { get; private set; }
        /// <summary>
        /// Берёзовый плод, серёжки
        /// </summary>
        public static BlockFetus FetusBirch { get; private set; }

        /// <summary>
        /// Блок бревна дуба
        /// </summary>
        public static BlockTree LogOak { get; private set; }
        /// <summary>
        /// Блок ветки дуба
        /// </summary>
        public static BlockTree BranchOak { get; private set; }
        /// <summary>
        /// Блок листвы дуба
        /// </summary>
        public static BlockTree LeavesOak { get; private set; }
        /// <summary>
        /// Блок саженца дуба
        /// </summary>
        public static BlockTree SaplingOak { get; private set; }
        /// <summary>
        /// Дубовый плод, жёлудь
        /// </summary>
        public static BlockFetus FetusOak { get; private set; }


        #endregion

        public static BlockBase Glass { get; private set; }
        public static BlockAlpha GlassRed { get; private set; }
        public static BlockAlpha GlassGreen { get; private set; }
        public static BlockAlpha GlassBlue { get; private set; }
        public static BlockAlpha GlassPurple { get; private set; }
        
        public static BlockBase Brol { get; private set; }

        public static void Initialization()
        {
            // Отладочный
            BlocksReg.RegisterBlockClass("Debug", Debug = new BlockDebug());
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
            OreCoal = BlocksReg.RegisterBlockClass("OreCoal", "Ore");
            OreIron = BlocksReg.RegisterBlockClass("OreIron", "Ore");
            OreGold = BlocksReg.RegisterBlockClass("OreGold", "Ore");
            OreSulfur = BlocksReg.RegisterBlockClass("OreSulfur", "Ore");
            OreDiamond = BlocksReg.RegisterBlockClass("OreDiamond", "Ore");
            OreEmerald = BlocksReg.RegisterBlockClass("OreEmerald", "Ore");
            OreRuby = BlocksReg.RegisterBlockClass("OreRuby", "Ore");
            OreSapphire = BlocksReg.RegisterBlockClass("OreSapphire", "Ore");

            BlocksReg.RegisterBlockClass("Grass", Grass = new BlockGrass());
            BlocksReg.RegisterBlockClass("TallGrass", TallGrass = new BlockTallGrass());

            Tina = BlocksReg.RegisterBlockClass("Tina");

            BlocksReg.RegisterBlockClass("FlowerDandelion", FlowerDandelion = new BlockGrass());
            BlocksReg.RegisterBlockClass("FlowerClover", FlowerClover = new BlockGrass());

            //FlowerDandelion = BlocksReg.RegisterBlockClass("FlowerDandelion");
            //FlowerClover = BlocksReg.RegisterBlockClass("FlowerClover");

            Stone = BlocksReg.RegisterBlockClass("Stone");
            Cobblestone= BlocksReg.RegisterBlockClass("Cobblestone");

            SaplingDry = BlocksReg.RegisterBlockClass("SaplingDry", "Tree");
            BlocksReg.RegisterBlockClass("TreeRoot", TreeRoot = new BlockTreeBirch(BlockTree.TypeTree.Root), "Tree");

            BlocksReg.RegisterBlockClass("LogBirch", LogBirch = new BlockTreeBirch(BlockTree.TypeTree.Log), "Tree");
            BlocksReg.RegisterBlockClass("BranchBirch", BranchBirch = new BlockTreeBirch(BlockTree.TypeTree.Branch), "Tree");
            BlocksReg.RegisterBlockClass("LeavesBirch", LeavesBirch = new BlockLeavesBirch(), "Tree");
            BlocksReg.RegisterBlockClass("SaplingBirch", SaplingBirch = new BlockTreeBirch(BlockTree.TypeTree.Sapling), "Tree");
            BlocksReg.RegisterBlockClass("FetusBirch", FetusBirch = new BlockFetusBirch(), "Tree");


            BlocksReg.RegisterBlockClass("LogOak", LogOak = new BlockTreeOak(BlockTree.TypeTree.Log), "Tree");
            BlocksReg.RegisterBlockClass("BranchOak", BranchOak = new BlockTreeOak(BlockTree.TypeTree.Branch), "Tree");
            BlocksReg.RegisterBlockClass("LeavesOak", LeavesOak = new BlockLeavesOak(), "Tree");
            BlocksReg.RegisterBlockClass("SaplingOak", SaplingOak = new BlockTreeOak(BlockTree.TypeTree.Sapling), "Tree");
            BlocksReg.RegisterBlockClass("FetusOak", FetusOak = new BlockFetusOak(), "Tree");

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
