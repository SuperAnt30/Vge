namespace Vge.World.Block.List
{
    /// <summary>
    /// Отсутствующий блок, может появится если в новой версии не будет прошлого блока
    /// </summary>
    public class BlockNull : BlockBase
    {
        /// <summary>
        /// Отсутствующий блок, может появится если в новой версии не будет прошлого блока
        /// </summary>
        public BlockNull()
        {
            _SetAir();

            // TODO::2025-08-18 надо подумать для отсутствующего блока, сколько может быть вариантов.
            // Ибо если их много надо все хранить. По индексу может вытащить всё это!!!
            _cullFaces = new bool[16][];
            _maskCullFaces = new ulong[16][][];
            for (int j = 0; j < 16; j++)
            {
                _cullFaces[j] = new bool[6];
                _maskCullFaces[j] = new ulong[6][];
                for (int i = 0; i < 6; i++)
                {
                    _maskCullFaces[j][i] = new ulong[4];
                }
            }
        }

        /// <summary>
        /// Является ли блок проходимым, т.е. можно ли ходить через него
        /// </summary>
        public override bool IsPassable(uint met) => true;
    }
}
