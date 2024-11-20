namespace Vge.World.Block.List
{
    /// <summary>
    /// Универсальный объект твёрдых блоков
    /// </summary>
    public class BlockUniSolid : BlockBase
    {
        public BlockUniSolid(int numberTexture)
        {
            Particle = numberTexture;
        }
    }
}
