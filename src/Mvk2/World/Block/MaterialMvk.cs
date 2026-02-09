using Vge.World.Block;

namespace Mvk2.World.Block
{
    /// <summary>
    /// Объект материала блока
    /// </summary>
    public class MaterialMvk : MaterialBase, IMaterialMvk
    {
        /// <summary>
        /// На каких блоках можно делать простой крафт
        /// </summary>
        public bool SimpleCraft { get; private set; }

        public MaterialMvk(int index) : base(index) { }

        /// <summary>
        /// Задать на каких блоках можно делать простой крафт
        /// </summary>
        public MaterialMvk SetSimpleCraft()
        {
            SimpleCraft = true;
            return this;
        }
    }
}
