using System;
using System.Collections.Generic;
using System.Text;

namespace WinGL.OpenGL
{
    public abstract class ShaderProgram
    {
        /// <summary>
        /// Программный объект шейдера
        /// </value>
        public uint ShaderProgramObject { get; private set; }

        private readonly Shader vertexShader = new Shader();
        private readonly Shader fragmentShader = new Shader();

        /// <summary>
        /// Создает программу шейдера
        /// </summary>
        /// <param name="vertexShaderSource">Строка вершинного шейдера</param>
        /// <param name="fragmentShaderSource">Строка фрагментного шейдера</param>
        /// <param name="attributeLocations">Расположение атрибутов. Это необязательный массив расположения атрибутов uint для их имен.</param>
        public void Create(GL gl, string vertexShaderSource, string fragmentShaderSource,
            Dictionary<uint, string> attributeLocations)
        {
            // Создание шейдоров
            vertexShader.Create(gl, GL.GL_VERTEX_SHADER, vertexShaderSource);
            fragmentShader.Create(gl, GL.GL_FRAGMENT_SHADER, fragmentShaderSource);

            // Создайте программу, прикрепите шейдеры
            ShaderProgramObject = gl.CreateProgram();
            gl.AttachShader(ShaderProgramObject, vertexShader.ShaderObject);
            gl.AttachShader(ShaderProgramObject, fragmentShader.ShaderObject);

            // Прежде чем мы свяжем, привяжите все местоположения атрибутов вершин
            if (attributeLocations != null)
            {
                foreach (var vertexAttributeLocation in attributeLocations)
                    gl.BindAttribLocation(ShaderProgramObject, vertexAttributeLocation.Key, vertexAttributeLocation.Value);
            }

            // Теперь мы можем связать программу
            gl.LinkProgram(ShaderProgramObject);

            // Теперь, когда мы скомпилировали и связали шейдер, проверьте статус его связи.
            // Если он не связан должным образом, мы создадим исключение.
            if (GetLinkStatus(gl) == false)
            {
                throw new ShaderCompilationException(string.Format("Не удалось связать шейдерную программу с ID {0}.",
                    ShaderProgramObject), GetInfoLog(gl));
            }
        }

        public void Delete(GL gl)
        {
            gl.DetachShader(ShaderProgramObject, vertexShader.ShaderObject);
            gl.DetachShader(ShaderProgramObject, fragmentShader.ShaderObject);
            vertexShader.Delete(gl);
            fragmentShader.Delete(gl);
            gl.DeleteProgram(ShaderProgramObject);
            ShaderProgramObject = 0;
        }

        public int GetAttributeLocation(GL gl, string attributeName)
            => gl.GetAttribLocation(ShaderProgramObject, attributeName);

        public void BindAttributeLocation(GL gl, uint location, string attribute)
            => gl.BindAttribLocation(ShaderProgramObject, location, attribute);

        public void Bind(GL gl) => gl.UseProgram(ShaderProgramObject);

        public void Unbind(GL gl) => gl.UseProgram(0);

        public bool GetLinkStatus(GL gl)
        {
            int[] parameters = new int[] { 0 };
            gl.GetProgram(ShaderProgramObject, GL.GL_LINK_STATUS, parameters);
            return parameters[0] == GL.GL_TRUE;
        }

        public string GetInfoLog(GL gl)
        {
            //  Get the info log length.
            int[] infoLength = new int[] { 0 };
            gl.GetProgram(ShaderProgramObject, GL.GL_INFO_LOG_LENGTH, infoLength);
            int bufSize = infoLength[0];

            //  Get the compile info.
            StringBuilder il = new StringBuilder(bufSize);
            gl.GetProgramInfoLog(ShaderProgramObject, bufSize, IntPtr.Zero, il);

            return il.ToString();
        }

        public void AssertValid(GL gl)
        {
            if (vertexShader.GetCompileStatus(gl) == false)
                throw new Exception(vertexShader.GetInfoLog(gl));
            if (fragmentShader.GetCompileStatus(gl) == false)
                throw new Exception(fragmentShader.GetInfoLog(gl));
            if (GetLinkStatus(gl) == false)
                throw new Exception(GetInfoLog(gl));
        }

        public void SetUniform1(GL gl, string uniformName, float v1)
            => gl.Uniform1(GetUniformLocation(gl, uniformName), v1);

        public void SetUniform2(GL gl, string uniformName, float v1, float v2)
            => gl.Uniform2(GetUniformLocation(gl, uniformName), v1, v2);

        public void SetUniform3(GL gl, string uniformName, float v1, float v2, float v3)
            => gl.Uniform3(GetUniformLocation(gl, uniformName), v1, v2, v3);

        public void SetUniform4(GL gl, string uniformName, float v1, float v2, float v3, float v4)
            => gl.Uniform4(GetUniformLocation(gl, uniformName), v1, v2, v3, v4);

        public void SetUniformMatrix4(GL gl, string uniformName, float[] m)
            => gl.UniformMatrix4(GetUniformLocation(gl, uniformName), 1, false, m);

        //public int GetUniformLocation(GL gl, string uniformName)
        //    => gl.GetUniformLocation(ShaderProgramObject, uniformName);
        public int GetUniformLocation(GL gl, string uniformName)
        {
            if (uniformNamesToLocations.ContainsKey(uniformName) == false)
            {
                uniformNamesToLocations.Add(uniformName, gl.GetUniformLocation(ShaderProgramObject, uniformName));
            }
            return uniformNamesToLocations[uniformName];
        }

        /// <summary>
        /// A mapping of uniform names to locations. This allows us to very easily specify 
        /// uniform data by name, quickly looking up the location first if needed.
        /// </summary>
        private readonly Dictionary<string, int> uniformNamesToLocations = new Dictionary<string, int>();
    }
}
