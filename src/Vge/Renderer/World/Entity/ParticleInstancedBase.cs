using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using WinGL.OpenGL;

namespace Vge.Renderer.World.Entity
{
    /// <summary>
    /// Общая часть инстансинга частиц: буфер данных экземпляров, загрузка, один draw call.
    /// Сетку и способ отрисовки задают наследники.
    /// </summary>
    public abstract class ParticleInstancedBase : IDisposable
    {
        /// <summary>
        /// Данные экземпляров текущего кадра, заполняются частицами
        /// </summary>
        public readonly float[] Data;
        
        /// <summary>
        /// Сколько экземпляров добавлено в этом кадре
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Максимум экземпляров
        /// </summary>
        private readonly int _capacity;
        /// <summary>
        /// Количество Float на один экземпляр
        /// </summary>
        protected readonly int _stride;

        protected readonly GL gl;
        protected readonly uint _vao;
        protected readonly uint _vboMesh;
        private readonly uint _vboInstance;
        private readonly int _strideBytes;

        /// <summary>
        /// Data закреплён в памяти, чтобы отдавать указатель в BufferSubData без копий
        /// </summary>
        private GCHandle _handle;

        /// <param name="stride">Float на экземпляр, кратно 4 (каждые 4 float — один vec4 атрибут)</param>
        /// <param name="firstLocation">Первый location атрибутов экземпляра в шейдере</param>
        protected ParticleInstancedBase(GL gl, int capacity, int stride, uint firstLocation)
        {
            this.gl = gl;
            _capacity = capacity;
            _stride = stride;
            _strideBytes = stride * sizeof(float);
            Data = new float[capacity * stride];
            _handle = GCHandle.Alloc(Data, GCHandleType.Pinned);

            uint[] id = new uint[1];
            gl.GenVertexArrays(1, id); _vao = id[0];
            gl.GenBuffers(1, id); _vboMesh = id[0];
            gl.GenBuffers(1, id); _vboInstance = id[0];

            // Атрибуты экземпляров: по vec4 подряд, шаг — один экземпляр.
            // Сетку (location 0, 1 и т.д.) наследник настраивает в своём конструкторе
            gl.BindVertexArray(_vao);
            gl.BindBuffer(GL.GL_ARRAY_BUFFER, _vboInstance);
            gl.BufferData(GL.GL_ARRAY_BUFFER, Data.Length * sizeof(float), IntPtr.Zero, GL.GL_STREAM_DRAW);
            int countVec4 = stride / 4;
            for (int i = 0; i < countVec4; i++)
            {
                uint location = firstLocation + (uint)i;
                gl.EnableVertexAttribArray(location);
                gl.VertexAttribPointer(location, 4, GL.GL_FLOAT, false, _strideBytes,
                    new IntPtr(i * 4 * sizeof(float)));
                // Значение меняется раз на экземпляр, а не на вершину
                gl.VertexAttribDivisor(location, 1);
            }
            gl.BindVertexArray(0);
        }

        /// <summary>
        /// Начать новый кадр
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Begin() => Count = 0;

        /// <summary>
        /// Зарезервировать место под экземпляр. Возвращает смещение в Data или -1, если буфер полон
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Next() => Count < _capacity ? Count++ * _stride : -1;

        /// <summary>
        /// Загрузить данные и нарисовать все экземпляры одним вызовом.
        /// Шейдер, uniform-ы и текстуры должны быть уже привязаны
        /// </summary>
        public void Draw()
        {
            if (Count == 0) return;

            gl.BindBuffer(GL.GL_ARRAY_BUFFER, _vboInstance);
            // Орфанинг: драйвер выдаёт новый участок памяти и не ждёт, пока GPU дорисует прошлый кадр
            gl.BufferData(GL.GL_ARRAY_BUFFER, Data.Length * sizeof(float), IntPtr.Zero, GL.GL_STREAM_DRAW);

            // Закрепляем массив только на время копирования в драйвер
            gl.BufferSubData(GL.GL_ARRAY_BUFFER, 0, Count * _strideBytes, _handle.AddrOfPinnedObject());

            gl.BindVertexArray(_vao);
            _DrawInstanced(Count);
            gl.BindVertexArray(0);
        }

        /// <summary>
        /// Вызов отрисовки сетки count раз (VAO уже привязан)
        /// </summary>
        protected abstract void _DrawInstanced(int count);

        public virtual void Dispose()
        {
            gl.DeleteBuffers(2, new uint[] { _vboMesh, _vboInstance });
            gl.DeleteVertexArrays(1, new uint[] { _vao });
            if (_handle.IsAllocated) _handle.Free();
        }
    }
}
