using Vge.Util;
using WinGL.OpenGL;

namespace Vge.Renderer
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
        /// Текстур заставки
        /// </summary>
        private uint splash = 0;

        /// <summary>
        /// Массив текстур
        /// </summary>
        private uint[] textures;

        /// <summary>
        /// Создать объект текстур
        /// </summary>
        /// <param name="gl">Объект методов OpenGL</param>
        public TextureMap(GL gl) => this.gl = gl;

        /// <summary>
        /// Указать количество текстур
        /// </summary>
        public void SetCount(int count) => textures = new uint[count];

        #region Splash

        /// <summary>
        /// Запустить текстуру заставки
        /// </summary>
        public void BindSplash()
        {
            if (splash != 0)
            {
                gl.ActiveTexture(GL.GL_TEXTURE0);
                gl.BindTexture(GL.GL_TEXTURE_2D, splash);
            }
        }

        /// <summary>
        /// Удалить текстуру заставки
        /// </summary>
        public void DeleteSplash()
        {
            if (splash != 0)
            {
                gl.DeleteTextures(1, new uint[] { splash });
            }
        }

        /// <summary>
        /// Удалить текстуру, указав индекс текстуры массива
        /// </summary>
        public void DeleteTexture(int index)
        {
            if (index < textures.Length)
            {
                gl.DeleteTextures(1, new uint[] { textures[index] });
                textures[index] = 0;
            }
        }

        /// <summary>
        /// Внесение в кеш текстур заставки
        /// </summary>
        /// <param name="image">рисунок</param>
        public void SetSplash(BufferedImage image)
        {
            uint key = SetTexture(splash, image);
            if (splash == 0)
            {
                splash = key;
            }
        }

        #endregion

        /// <summary>
        /// Запустить текстуру, указав индекс текстуры массива
        /// </summary>
        /// <param name="texture">OpenGL.GL_TEXTURE0 + texture</param>
        public void BindTexture(int index, uint texture = 0)
        {
            if (index < textures.Length)
            {
                gl.ActiveTexture(GL.GL_TEXTURE0 + texture);
                gl.BindTexture(GL.GL_TEXTURE_2D, textures[index]);
            }
        }

        /// <summary>
        /// Внесение в кеш текстур
        /// </summary>
        /// <param name="index">индекс текстуры массива</param>
        /// <param name="image">рисунок</param>
        public void SetTexture(int index, BufferedImage image)
        {
            if (index < textures.Length)
            {
                uint key = SetTexture(textures[index], image);
                if (textures[index] == 0)
                {
                    textures[index] = key;
                }
            }
        }

        /// <summary>
        /// Внесение в кеш текстур
        /// </summary>
        private uint SetTexture(uint key, BufferedImage image)
        {
            bool isCreate = key == 0;
            if (isCreate)
            {
                uint[] id = new uint[1];
                gl.GenTextures(1, id);
                key = id[0];
            }
            gl.ActiveTexture(GL.GL_TEXTURE0 + image.ActiveTextureIndex);
            //gl.PixelStore(GL.GL_UNPACK_ALIGNMENT, 1);// отключаем ограничение выравнивания байтов
            gl.BindTexture(GL.GL_TEXTURE_2D, key);

            if (isCreate)
            {
                if (image.FlagMipMap)
                {
                    // У текстуры имеется MipMap
                    gl.TexImage2D(GL.GL_TEXTURE_2D, 0, GL.GL_RGBA, image.Width, image.Height,
                            0, GL.GL_BGRA, GL.GL_UNSIGNED_BYTE, image.Buffer);

                    int count = image.CountMipMap();
                    if (count > 0)
                    {
                        // Содаём текстуру со своими текстурами для каждого уровня mipmap
                        BufferedImage bufferedImage;
                        for (int i = 0; i < image.CountMipMap(); i++)
                        {
                            bufferedImage = image.GetLevelMipMap(i);
                            gl.TexImage2D(GL.GL_TEXTURE_2D, i + 1, GL.GL_RGBA, bufferedImage.Width, bufferedImage.Height,
                                0, GL.GL_BGRA, GL.GL_UNSIGNED_BYTE, bufferedImage.Buffer);

                        }
                        gl.TexParameter(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MAX_LEVEL, count);
                    }
                    else
                    {
                        // Содаём автоматическую текстуру для каждого уровня mipmap
                        gl.TexParameter(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MAX_LEVEL, 5);
                        gl.GenerateMipmapEXT(GL.GL_TEXTURE_2D);
                    }

                    gl.TexParameter(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_WRAP_S, GL.GL_REPEAT);
                    gl.TexParameter(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_WRAP_T, GL.GL_REPEAT);
                    gl.TexParameter(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MIN_FILTER, GL.GL_NEAREST_MIPMAP_NEAREST);
                    gl.TexParameter(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MAG_FILTER, GL.GL_NEAREST);
                }
                else
                {
                    // У текстуры НЕТ MipMapа

                    gl.TexImage2D(GL.GL_TEXTURE_2D, 0, GL.GL_RGBA, image.Width, image.Height,
                        0, GL.GL_BGRA, GL.GL_UNSIGNED_BYTE, image.Buffer);

                    gl.TexParameter(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_WRAP_S, GL.GL_CLAMP_TO_BORDER);
                    gl.TexParameter(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_WRAP_T, GL.GL_CLAMP_TO_BORDER);
                    gl.TexParameter(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MIN_FILTER, GL.GL_NEAREST);
                    gl.TexParameter(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MAG_FILTER, GL.GL_NEAREST);
                }
            }
            else
            {
                gl.TexSubImage2D(GL.GL_TEXTURE_2D, 0, 0, 0, image.Width, image.Height,
                        GL.GL_BGRA, GL.GL_UNSIGNED_BYTE, image.Buffer);
            }
            return key;
        }


    }
}
