namespace Vge.World.Block.List
{
    /// <summary>
    /// Блок с полупрозрачными текстурами, для отдельной сетки которая сортируется
    /// </summary>
    public class BlockAlpha : BlockBase
    {
        public BlockAlpha()
        {
            Alpha = true;
            AlphaSort = true;
        }

        /// <summary>
        /// Инициализация объекта рендера для блока
        /// </summary>
        protected override void _InitBlockRender()
            => BlockRender = Gi.BlockAlphaRendFull;
    }
}
