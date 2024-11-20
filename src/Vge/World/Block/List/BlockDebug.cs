namespace Vge.World.Block.List
{
    /// <summary>
    /// Универсальный объект твёрдых блоков
    /// </summary>
    public class BlockDebug : BlockBase
    {
        public BlockDebug()
        {
            //Particle = numberTexture;
            //_InitQuads(numberTexture);
        }

        /// <summary>
        /// Стороны целого блока для рендера
        /// </summary>
        public override QuadSide[] GetQuads(uint met, int xb, int zb) => _GetQuads((int)met);
    }
}
