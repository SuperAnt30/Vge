using WinGL.Util;

namespace WinGL.OpenGL
{
    /// <summary>
    /// Объект текстур
    /// </summary>
    public class TextureMap
    {
        /// <summary>
        /// Объект методов OpenGL
        /// </summary>
        protected GL gl;

        /// <summary>
        /// Массив текстур
        /// </summary>
        protected uint[] textures;

        /// <summary>
        /// Создать объект текстур
        /// </summary>
        /// <param name="gl">Объект методов OpenGL</param>
        /// <param name="count">Указать количество текстур</param>
        public TextureMap(GL gl, int count)
        {
            this.gl = gl;
            textures = new uint[count];
        }

        /// <summary>
        /// Добавить текстуру
        /// </summary>
        /// <param name="index">индекс текстуры массива</param>
        /// <param name="image">рисунок</param>
        public void AddTexture(int index, BufferedImage image)
            => SetTexture(index, image);

        /// <summary>
        /// Запустить текстуру, указав индекс текстуры массива
        /// </summary>
        public void BindTexture(int index)
        {
            if (index < textures.Length)
            {
                BindTexture(textures[index], 0);
            }
        }

        /// <summary>
        /// Запустить текстуру по ключу текстуры сгенерированным OpenGL
        /// </summary>
        /// <param name="key">ключ текстуры</param>
        /// <param name="texture">OpenGL.GL_TEXTURE0 + texture</param>
        private void BindTexture(uint key, uint texture)
        {
            gl.ActiveTexture(GL.GL_TEXTURE0 + texture);
            gl.BindTexture(GL.GL_TEXTURE_2D, key);
        }

        /// <summary>
        /// Внесение в кеш текстур
        /// </summary>
        /// <param name="index">индекс текстуры массива</param>
        /// <param name="image">рисунок</param>
        private uint SetTexture(int index, BufferedImage image)
        {
            if (index < textures.Length)
            {
                uint key = textures[index];
                if (key == 0)
                {
                    uint[] texture = new uint[1];
                    gl.GenTextures(1, texture);
                    key = texture[0];
                    textures[index] = key;
                }

                gl.BindTexture(GL.GL_TEXTURE_2D, key);
                gl.PixelStore(GL.GL_UNPACK_ALIGNMENT, 1);

                gl.TexImage2D(GL.GL_TEXTURE_2D, 0, GL.GL_RGBA, image.width, image.height,
                    0, GL.GL_BGRA, GL.GL_UNSIGNED_BYTE, image.buffer);
                gl.TexParameter(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_WRAP_S, GL.GL_REPEAT);
                gl.TexParameter(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_WRAP_T, GL.GL_REPEAT);
                gl.TexParameter(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MIN_FILTER, GL.GL_NEAREST);
                gl.TexParameter(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MAG_FILTER, GL.GL_NEAREST);

                gl.BindTexture(GL.GL_TEXTURE_2D, 0);

                return key;
            }
            return 0;
        }
    }
}
