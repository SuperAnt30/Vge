namespace Vge.Entity.Render
{
    /// <summary>
    /// Позиция предмета в кости
    /// </summary>
    public readonly struct PositionItemBone
    {
        /// <summary>
        /// Индекс кости
        /// </summary>
        public readonly byte Index;
        public readonly float X;
        public readonly float Y;
        public readonly float Z;

        public PositionItemBone(byte index, float x, float y, float z)
        {
            Index = index;
            X = x;
            Y = y;
            Z = z;
        }
    }
}
