namespace Vge.World.Block
{
    /// <summary>
    /// Объект материала блока
    /// </summary>
    public class MaterialBase : IMaterial
    {
        /// <summary>
        /// Получить уникальный номер материала
        /// </summary>
        public int IndexMaterial { get; private set; }
        /// <summary>
        /// Возвращает, если блоки этих материалов являются жидкостями
        /// </summary>
        public bool IsLiquid { get; private set; }
        /// <summary>
        /// Не требует инструмента для разрушения блока
        /// </summary>
        public bool RequiresNoTool { get; private set; }
        /// <summary>
        /// Является ли блок стеклянным, блок или панель
        /// </summary>
        public bool Glass { get; private set; }
        /// <summary>
        /// Дёрн не сохнет, под этим блоком
        /// </summary>
        public bool TurfDoesNotDry { get; private set; }
        /// <summary>
        /// Воспламеняет (лава или огонь)
        /// </summary>
        public bool Ignites { get; private set; }
        /// <summary>
        /// Растёт корень
        /// </summary>
        public bool RootGrowing { get; private set; }

        public MaterialBase(int index) => IndexMaterial = index;

        /// <summary>
        /// Задать материал жидкостью
        /// </summary>
        public MaterialBase Liquid()
        {
            IsLiquid = true;
            return this;
        }

        /// <summary>
        /// Задать Не требует инструмента для разрушения блока
        /// </summary>
        public MaterialBase SetRequiresNoTool()
        {
            RequiresNoTool = true;
            return this;
        }

        /// <summary>
        /// Задать Является ли блок стеклянным, блок или панель
        /// </summary>
        public MaterialBase SetGlass()
        {
            Glass = true;
            return this;
        }

        /// <summary>
        /// Задать Дёрн не сохнет, под этим блоком
        /// </summary>
        public MaterialBase SetTurfDoesNotDry()
        {
            TurfDoesNotDry = true;
            return this;
        }

        /// <summary>
        /// Задать воспламенение (огонь или лава)
        /// </summary>
        public MaterialBase SetIgnites()
        {
            Ignites = true;
            return this;
        }

        /// <summary>
        /// Задать на этих блоках может рости корень дерева
        /// </summary>
        public MaterialBase SetRootGrowing()
        {
            RootGrowing = true;
            return this;
        }

        /// <summary>
        /// Строка
        /// </summary>
        public override string ToString() => IndexMaterial.ToString();
    }
}
