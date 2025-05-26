using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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
        private readonly GL gl;
        /// <summary>
        /// Массив текстур
        /// </summary>
        private readonly List<uint> _textures = new List<uint>();

        /// <summary>
        /// Создать объект текстур
        /// </summary>
        /// <param name="gl">Объект методов OpenGL</param>
        public TextureMap(GL gl) => this.gl = gl;

        /// <summary>
        /// Удалить текстуру, указав индекс текстуры массива
        /// </summary>
        public void DeleteTexture(uint index)
        {
            if (_textures.Contains(index))
            {
                gl.DeleteTextures(1, new uint[] { index });
                _textures.Remove(index);
            }
        }

        /// <summary>
        /// Запустить текстуру, указав индекс текстуры массива
        /// </summary>
        /// <param name="texture">OpenGL.GL_TEXTURE0 + texture</param>
        public void BindTexture(uint index, uint texture = 0)
        {
            gl.ActiveTexture(GL.GL_TEXTURE0 + texture);
            gl.BindTexture(GL.GL_TEXTURE_2D, index);
        }

        /// <summary>
        /// Внесение в кеш текстур
        /// </summary>
        /// <param name="image">рисунок</param>
        /// <param name="index">Индекс, если равен 0 то создать</param>
        public uint SetTexture(BufferedImage image, uint index = 0)
        {
            if (!_textures.Contains(index))
            {
                index = 0;
            }
            bool isCreate = index == 0;
            if (isCreate)
            {
                uint[] id = new uint[1];
                gl.GenTextures(1, id);
                index = id[0];
                _textures.Add(index);
            }
            gl.ActiveTexture(GL.GL_TEXTURE0 + image.ActiveTextureIndex);
            //gl.PixelStore(GL.GL_UNPACK_ALIGNMENT, 1);// отключаем ограничение выравнивания байтов
            gl.BindTexture(GL.GL_TEXTURE_2D, index);

            if (isCreate)
            {
                if (image.FlagMipMap)
                {
                    // У текстуры имеется MipMap
                    gl.TexImage2D(GL.GL_TEXTURE_2D, 0, GL.GL_RGBA, image.Width, image.Height,
                            0, GL.GL_BGRA, GL.GL_UNSIGNED_BYTE, image.Buffer);
                   // gl.TexImage2D(GL.GL_TEXTURE_2)

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
            return index;
        }

        #region 2D Array

        /// <summary>
        /// Запустить 2d массив текстур, указав индекс текстуры массива
        /// </summary>
        /// <param name="texture">OpenGL.GL_TEXTURE0 + texture</param>
        public void BindTexture2dArray(uint index, uint texture = 0)
        {
            gl.ActiveTexture(GL.GL_TEXTURE0 + texture);
            gl.BindTexture(GL.GL_TEXTURE_2D_ARRAY, index);
        }

        /// <summary>
        /// Создать массив текстур (GL_TEXTURE_2D_ARRAY)
        /// </summary>
        /// <param name="width">Ширина</param>
        /// <param name="height">Высота</param>
        /// <param name="depth">Глубина</param>
        /// <param name="texture">ActiveTexture</param>
        /// <param name="id">Id текстуры если надо пересоздать, id=0 - создать</param>
        public uint CreateTexture2dArray(int width, int height, int depth, uint texture = 0, uint id = 0)
        {
            if (!_textures.Contains(id))
            {
                id = 0;
            }
            if (id == 0)
            {
                uint[] uid = new uint[1];
                gl.GenTextures(1, uid);
                id = uid[0];
                _textures.Add(id);
                gl.ActiveTexture(GL.GL_TEXTURE0 + texture);
                gl.BindTexture(GL.GL_TEXTURE_2D_ARRAY, id);
                gl.TexStorage3D(GL.GL_TEXTURE_2D_ARRAY, 1, GL.GL_RGBA8, width, height, depth);
                gl.TexParameter(GL.GL_TEXTURE_2D_ARRAY, GL.GL_TEXTURE_WRAP_S, GL.GL_CLAMP_TO_BORDER);
                gl.TexParameter(GL.GL_TEXTURE_2D_ARRAY, GL.GL_TEXTURE_WRAP_T, GL.GL_CLAMP_TO_BORDER);
                gl.TexParameter(GL.GL_TEXTURE_2D_ARRAY, GL.GL_TEXTURE_MIN_FILTER, GL.GL_NEAREST);
                gl.TexParameter(GL.GL_TEXTURE_2D_ARRAY, GL.GL_TEXTURE_MAG_FILTER, GL.GL_NEAREST);
            }
            return id;
        }

        /// <summary>
        /// Добавить картинку в слой
        /// </summary>
        /// <param name="image">Картинка</param>
        /// <param name="depth">Слой</param>
        /// <param name="id">Id текстуры</param>
        public void SetImageTexture2dArray(BufferedImage image, int depth, uint id, uint texture = 0)
        {
            gl.ActiveTexture(GL.GL_TEXTURE0 + texture);
            gl.BindTexture(GL.GL_TEXTURE_2D_ARRAY, id);

            IntPtr intPtr = Marshal.AllocHGlobal(image.Buffer.Length);
            Marshal.Copy(image.Buffer, 0, intPtr, image.Buffer.Length);
            //OpenGLError e1 = gl.GetError();
            gl.TexSubImage3D(GL.GL_TEXTURE_2D_ARRAY, 0, 0, 0, depth, image.Width, image.Height,
               1, GL.GL_BGRA, GL.GL_UNSIGNED_BYTE, intPtr);

            //OpenGLError e2 = gl.GetError();
        }

        #endregion 
    }
}
