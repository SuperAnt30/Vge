using System;
using System.Text;

namespace WinGL.OpenGL
{
    /// <summary>
    /// Это базовый класс для всех шейдеров(вершинных и фрагментных). Он предлагает функциональность
    /// который является ядром всех шейдеров, таких как загрузка и привязка файлов.
    /// </summary>
    public class Shader
    {
        /// <summary>
        /// Объект шейдера OpenGL
        /// </summary>
        public uint ShaderObject { get; private set; }

        public void Create(GL gl, uint shaderType, string source)
        {
            // Создайте объект шейдера OpenGL
            ShaderObject = gl.CreateShader(shaderType);

            // Установите источник шейдера
            gl.ShaderSource(ShaderObject, source);

            // Скомпилируйте объект шейдера
            gl.CompileShader(ShaderObject);

            // Теперь, когда мы скомпилировали шейдер, проверьте статус его компиляции. 
            // Если он скомпилирован неправильно, мы создадим исключение.
            if (GetCompileStatus(gl) == false)
            {
                string log = GetInfoLog(gl);
                throw new ShaderCompilationException(
                    Sr.GetString(Sr.FailedToCompileShaderWithID, ShaderObject, log), log);
            }
        }

        public void Delete(GL gl)
        {
            gl.DeleteShader(ShaderObject);
            ShaderObject = 0;
        }

        public bool GetCompileStatus(GL gl)
        {
            int[] parameters = new int[] { 0 };
            gl.GetShader(ShaderObject, GL.GL_COMPILE_STATUS, parameters);
            return parameters[0] == GL.GL_TRUE;
        }

        public string GetInfoLog(GL gl)
        {
            // Получите длину информационного журнала
            int[] infoLength = new int[] { 0 };
            gl.GetShader(ShaderObject, GL.GL_INFO_LOG_LENGTH, infoLength);
            int bufSize = infoLength[0];

            // Получить информацию о компиляции
            StringBuilder il = new StringBuilder(bufSize);
            gl.GetShaderInfoLog(ShaderObject, bufSize, IntPtr.Zero, il);

            return il.ToString();
        }
    }
}
