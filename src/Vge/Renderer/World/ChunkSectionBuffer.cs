namespace Vge.Renderer.World
{
    /// <summary>
    /// Буфер секций чанка
    /// </summary>
    public class ChunkSectionBuffer
    {
        /// <summary>
        /// Буфер для склейки рендера, Float данных
        /// </summary>
        public float[] BufferFloat;

        /// <summary>
        /// Буфер для склейки рендера, Byte данных
        /// </summary>
        public byte[] BufferByte;

        public void Clear()
        {
            BufferFloat = new float[0];
            BufferByte = new byte[0];
        }

        public override string ToString()
            => "F:" + (BufferFloat == null ? "Null" : BufferFloat.Length.ToString())
               + " B:" + (BufferByte == null ? "Null" : BufferByte.Length.ToString());
    }
}
