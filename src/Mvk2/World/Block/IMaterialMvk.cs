using Vge.World.Block;

namespace Mvk2.World.Block
{
    /// <summary>
    /// Интерфейс материала для малювек
    /// </summary>
    public interface IMaterialMvk : IMaterial
    {
        /// <summary>
        /// На каких блоках можно делать простой крафт
        /// </summary>
        bool SimpleCraft { get; }
        
    }
}
