namespace Vge.World.Chunk
{
    /// <summary>
    /// Базовый объект чанка
    /// </summary>
    public class ChunkBase : IChunkPosition
    {
        /// <summary>
        /// Позиция X текущего чанка
        /// </summary>
        public int CurrentChunkX { get; private set; }
        /// <summary>
        /// Позиция Y текущего чанка
        /// </summary>
        public int CurrentChunkY { get; private set; }
        /// <summary>
        /// Сылка на объект мира
        /// </summary>
        public readonly WorldBase World;
        /// <summary>
        /// Совокупное количество тиков, которые якори провели в этом чанке 
        /// </summary>
        public uint InhabitedTakt { get; private set; }
        /// <summary>
        /// Присутствует, этап загрузки или начальная генерация #1 1*1
        /// </summary>
        public bool IsChunkPresent { get; private set; }
        /// <summary>
        /// Было ли декорация чанка #2 3*3
        /// </summary>
        public bool IsPopulated { get; private set; }

        public ChunkBase(WorldBase world, int chunkPosX, int chunkPosY)
        {
            World = world;
            CurrentChunkX = chunkPosX;
            CurrentChunkY = chunkPosY;
        }

        /// <summary>
        /// Задать совокупное количество тактов, которые якоря провели в этом чанке 
        /// </summary>
        public void SetInhabitedTime(uint takt) => InhabitedTakt = takt;

        /// <summary>
        /// Загрузили чанк
        /// </summary>
        public void OnChunkLoad()
        {
            IsChunkPresent = true;
            //if (!World.IsRemote && World is WorldServer worldServer)
            //{
            //    if (CurrentChunkX < 200)
            //    {
            //        worldServer.Fragment.AddWorldAnchorChunk(CurrentChunkX + 5, CurrentChunkY);
            //    }
            //}
        }

        /// <summary>
        /// Выгрузили чанк
        /// </summary>
        public void OnChunkUnload()
        {
            IsChunkPresent = false;
        }


        /// <summary>
        /// Заполнение чанка населённостью
        /// </summary>
        private void _Populate()
        {

        }


        public override string ToString() => CurrentChunkX + " : " + CurrentChunkY;
    }
}
