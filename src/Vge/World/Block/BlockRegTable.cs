using Vge.Util;
using Vge.World.Block.List;

namespace Vge.World.Block
{
    /// <summary>
    /// Таблица блоков для регистрации
    /// </summary>
    public class BlockRegTable : RegTable<BlockBase>
    {
        protected override BlockBase _CreateNull() => new BlockNull();
    }
}
