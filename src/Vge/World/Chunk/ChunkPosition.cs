namespace Vge.World.Chunk
{
    public struct ChunkPosition : IChunkPosition
    {
        public int CurrentChunkX { get; }
        public int CurrentChunkY { get; }

        public ChunkPosition(int x, int y)
        {
            CurrentChunkX = x;
            CurrentChunkY = y;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(ChunkPosition))
            {
                var pos = (ChunkPosition)obj;
                if (CurrentChunkX == pos.CurrentChunkX && CurrentChunkY == pos.CurrentChunkY)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode() => CurrentChunkX.GetHashCode() ^ CurrentChunkY.GetHashCode();

        public override string ToString() => CurrentChunkX + " : " + CurrentChunkY;
    }
}
