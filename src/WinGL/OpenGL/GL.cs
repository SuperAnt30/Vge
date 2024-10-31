using System;
using System.Runtime.InteropServices;
using System.Text;

namespace WinGL.OpenGL
{
    /// <summary>
    /// Объект методов OpenGL
    /// </summary>
    public partial class GL
    {
        /// <summary>
        /// Делает экземпляр OpenGL текущим
        /// </summary>
        private static GL currentOpenGLInstance;
        /// <summary>
        /// Постоянный контекст рендеринга
        /// </summary>
        private IntPtr hRC;

        #region OpenGL Extensions

        /// <summary>
        /// Получить указатель на неуправляемую функцию в делегат
        /// </summary>
        private Delegate GetDelegate<T>()
        {
            Type delegateType = typeof(T);
            IntPtr proc = wglGetProcAddress(delegateType.Name);
            if (proc == IntPtr.Zero)
            {
                throw new Exception(Sr.GetString(Sr.ExtensionFunctionIsNotSupported, delegateType.Name));
            }
            return Marshal.GetDelegateForFunctionPointer(proc, delegateType);
        }

        #region Delegates

        private delegate void glActiveTexture(uint texture);
        private glActiveTexture delegateActiveTexture;

        private delegate void glBindVertexArray(uint array);
        private glBindVertexArray delegateBindVertexArray;
        private delegate void glDeleteVertexArrays(int n, uint[] arrays);
        private glDeleteVertexArrays delegateDeleteVertexArrays;
        private delegate void glGenVertexArrays(int n, uint[] arrays);
        private glGenVertexArrays delegateGenVertexArrays;

        private delegate void glGenBuffers(int n, uint[] buffers);
        private glGenBuffers delegateGenBuffers;
        private delegate void glBindBuffer(uint target, uint buffer);
        private glBindBuffer delegateBindBuffer;
        private delegate void glDeleteBuffers(int n, uint[] buffers);
        private glDeleteBuffers delegateDeleteBuffers;
        private delegate void glBufferData(uint target, int size, IntPtr data, uint usage);
        private glBufferData delegateBufferData;

        private delegate void glEnableVertexAttribArray(uint index);
        private glEnableVertexAttribArray delegateEnableVertexAttribArray;
        private delegate void glVertexAttribPointer(uint index, int size, uint type, bool normalized, int stride, IntPtr pointer);
        private glVertexAttribPointer delegateVertexAttribPointer;

        private delegate uint glCreateShader(uint type);
        private glCreateShader delegateCreateShader;
        private delegate void glShaderSource(uint shader, int count, string[] source, int[] length);
        private glShaderSource delegateShaderSource;

        private delegate void glCompileShader(uint shader);
        private glCompileShader delegateCompileShader;
        private delegate void glDeleteShader(uint shader);
        private glDeleteShader delegateDeleteShader;
        private delegate void glGetShaderiv(uint shader, uint pname, int[] parameters);
        private glGetShaderiv delegateGetShader;
        private delegate void glGetShaderInfoLog(uint shader, int bufSize, IntPtr length, StringBuilder infoLog);
        private glGetShaderInfoLog delegateGetShaderInfoLog;
        private delegate uint glCreateProgram();
        private glCreateProgram delegateCreateProgram;
        private delegate void glAttachShader(uint program, uint shader);
        private glAttachShader delegateAttachShader;
        private delegate void glBindAttribLocation(uint program, uint index, string name);
        private glBindAttribLocation delegateBindAttribLocation;
        private delegate void glLinkProgram(uint program);
        private glLinkProgram delegateLinkProgram;
        private delegate void glDetachShader(uint program, uint shader);
        private glDetachShader delegateDetachShader;
        private delegate void glDeleteProgram(uint program);
        private glDeleteProgram delegateDeleteProgram;
        private delegate int glGetAttribLocation(uint program, string name);
        private glGetAttribLocation delegateGetAttribLocation;
        private delegate void glUseProgram(uint program);
        private glUseProgram delegateUseProgram;
        private delegate void glGetProgramiv(uint program, uint pname, int[] parameters);
        private glGetProgramiv delegateGetProgram;
        private delegate void glGetProgramInfoLog(uint program, int bufSize, IntPtr length, StringBuilder infoLog);
        private glGetProgramInfoLog delegateGetProgramInfoLog;

        private delegate void glUniform1i(int location, int v0);
        private glUniform1i delegateUniform1i;
        private delegate void glUniform1f(int location, float v0);
        private glUniform1f delegateUniform1f;
        private delegate void glUniform2f(int location, float v0, float v1);
        private glUniform2f delegateUniform2;
        private delegate void glUniform3f(int location, float v0, float v1, float v2);
        private glUniform3f delegateUniform3;
        private delegate void glUniform4f(int location, float v0, float v1, float v2, float v3);
        private glUniform4f delegateUniform4;
        private delegate void glUniformMatrix4fv(int location, int count, bool transpose, float[] value);
        private glUniformMatrix4fv delegateUniformMatrix4;
        private delegate int glGetUniformLocation(uint program, string name);
        private glGetUniformLocation delegateGetUniformLocation;
        private delegate void glGenerateMipmapEXT(uint target);
        private glGenerateMipmapEXT delegateGenerateMipmapEXT;

        private delegate IntPtr wglCreateContextAttribsARB(IntPtr hDC, IntPtr hShareContext, int[] attribList);
        private wglCreateContextAttribsARB delegateCreateContextAttribsARB;
        private delegate bool wglSwapIntervalEXT(int interval);
        private wglSwapIntervalEXT delegateSwapIntervalEXT;

        #endregion

        public bool SwapIntervalEXT(int interval)
        {
            if (delegateSwapIntervalEXT == null)
                delegateSwapIntervalEXT = GetDelegate<wglSwapIntervalEXT>() as wglSwapIntervalEXT;
            return delegateSwapIntervalEXT(interval);
        }

        /// <summary>
        /// Выбрать активный текстурный блок
        /// </summary>
        public void ActiveTexture(uint texture)
        {
            if (delegateActiveTexture == null)
                delegateActiveTexture = GetDelegate<glActiveTexture>() as glActiveTexture;
            delegateActiveTexture(texture);
        }

        public void BindVertexArray(uint array)
        {
            if (delegateBindVertexArray == null)
                delegateBindVertexArray = GetDelegate<glBindVertexArray>() as glBindVertexArray;
            delegateBindVertexArray(array);
        }
        public void DeleteVertexArrays(int n, uint[] arrays)
        {
            if (delegateDeleteVertexArrays == null)
                delegateDeleteVertexArrays = GetDelegate<glDeleteVertexArrays>() as glDeleteVertexArrays;
            delegateDeleteVertexArrays(n, arrays);
        }
        public void GenVertexArrays(int n, uint[] arrays)
        {
            if (delegateGenVertexArrays == null)
                delegateGenVertexArrays = GetDelegate<glGenVertexArrays>() as glGenVertexArrays;
            delegateGenVertexArrays(n, arrays);
        }

        public void GenBuffers(int n, uint[] buffers)
        {
            if (delegateGenBuffers == null)
                delegateGenBuffers = GetDelegate<glGenBuffers>() as glGenBuffers;
            delegateGenBuffers(n, buffers);
        }
        public void BindBuffer(uint target, uint buffer)
        {
            if (delegateBindBuffer == null)
                delegateBindBuffer = GetDelegate<glBindBuffer>() as glBindBuffer;
            delegateBindBuffer(target, buffer);
        }
        public void DeleteBuffers(int n, uint[] buffers)
        {
            if (delegateDeleteBuffers == null)
                delegateDeleteBuffers = GetDelegate<glDeleteBuffers>() as glDeleteBuffers;
            delegateDeleteBuffers(n, buffers);
        }
        public void BufferData(uint target, int size, IntPtr data, uint usage)
        {
            if (delegateBufferData == null)
                delegateBufferData = GetDelegate<glBufferData>() as glBufferData;
            delegateBufferData(target, size, data, usage);
        }
        public void BufferData(uint target, float[] data, uint usage)
        {
            int size = data.Length * sizeof(float);
            IntPtr p = Marshal.AllocHGlobal(size);
            Marshal.Copy(data, 0, p, data.Length);
            BufferData(target, size, p, usage);
            Marshal.FreeHGlobal(p);
        }
        public void BufferData(uint target, int count, float[] data, uint usage)
        {
            int size = count * sizeof(float);
            IntPtr p = Marshal.AllocHGlobal(size);
            Marshal.Copy(data, 0, p, count);
            BufferData(target, size, p, usage);
            Marshal.FreeHGlobal(p);
        }
        public void BufferData(uint target, int[] data, uint usage)
        {
            int size = data.Length * sizeof(int);
            IntPtr p = Marshal.AllocHGlobal(size);
            Marshal.Copy(data, 0, p, data.Length);
            BufferData(target, size, p, usage);
            Marshal.FreeHGlobal(p);
        }

        public void EnableVertexAttribArray(uint index)
        {
            if (delegateEnableVertexAttribArray == null)
                delegateEnableVertexAttribArray = GetDelegate<glEnableVertexAttribArray>() as glEnableVertexAttribArray;
            delegateEnableVertexAttribArray(index);
        }
        public void VertexAttribPointer(uint index, int size, uint type, bool normalized, int stride, IntPtr pointer)
        {
            if (delegateVertexAttribPointer == null)
                delegateVertexAttribPointer = GetDelegate<glVertexAttribPointer>() as glVertexAttribPointer;
            delegateVertexAttribPointer(index, size, type, normalized, stride, pointer);
        }

        public uint CreateShader(uint type)
        {
            if (delegateCreateShader == null)
                delegateCreateShader = GetDelegate<glCreateShader>() as glCreateShader;
            return delegateCreateShader(type);
        }
        public void ShaderSource(uint shader, string source)
        {
            if (delegateShaderSource == null)
                delegateShaderSource = GetDelegate<glShaderSource>() as glShaderSource;
            delegateShaderSource(shader, 1, new[] { source }, new[] { source.Length });
        }

        public void CompileShader(uint shader)
        {
            if (delegateCompileShader == null)
                delegateCompileShader = GetDelegate<glCompileShader>() as glCompileShader;
            delegateCompileShader(shader);
        }
        public void DeleteShader(uint shader)
        {
            if (delegateDeleteShader == null)
                delegateDeleteShader = GetDelegate<glDeleteShader>() as glDeleteShader;
            delegateDeleteShader(shader);
        }
        public void GetShader(uint shader, uint pname, int[] parameters)
        {
            if (delegateGetShader == null)
                delegateGetShader = GetDelegate<glGetShaderiv>() as glGetShaderiv;
            delegateGetShader(shader, pname, parameters);
        }
        public void GetShaderInfoLog(uint shader, int bufSize, IntPtr length, StringBuilder infoLog)
        {
            if (delegateGetShaderInfoLog == null)
                delegateGetShaderInfoLog = GetDelegate<glGetShaderInfoLog>() as glGetShaderInfoLog;
            delegateGetShaderInfoLog(shader, bufSize, length, infoLog);
        }
        //public string GetString(uint name, uint index)
        //{
        //    if (delegateGetStringi == null)
        //        delegateGetStringi = GetDelegate<glGetStringi>() as glGetStringi;
        //    sbyte* pStr = (sbyte*)delegateGetStringi(name, index);
        //    IntPtr intPtr = delegateGetStringi(name, index);
        //    //sbyte[] pStr = (sbyte[])intPtr;

        //    return "";
        //    //sbyte* pStr = (sbyte*)GetDelegateFor<glGetStringi>()(name, index);
        //    //var str = new string(pStr);
        //    //return str;
        //}
        public uint CreateProgram()
        {
            if (delegateCreateProgram == null)
                delegateCreateProgram = GetDelegate<glCreateProgram>() as glCreateProgram;
            return delegateCreateProgram();
        }
        public void AttachShader(uint program, uint shader)
        {
            if (delegateAttachShader == null)
                delegateAttachShader = GetDelegate<glAttachShader>() as glAttachShader;
            delegateAttachShader(program, shader);
        }
        public void BindAttribLocation(uint program, uint index, string name)
        {
            if (delegateBindAttribLocation == null)
                delegateBindAttribLocation = GetDelegate<glBindAttribLocation>() as glBindAttribLocation;
            delegateBindAttribLocation(program, index, name);
        }
        public void LinkProgram(uint program)
        {
            if (delegateLinkProgram == null)
                delegateLinkProgram = GetDelegate<glLinkProgram>() as glLinkProgram;
            delegateLinkProgram(program);
        }
        public void DetachShader(uint program, uint shader)
        {
            if (delegateDetachShader == null)
                delegateDetachShader = GetDelegate<glDetachShader>() as glDetachShader;
            delegateDetachShader(program, shader);
        }
        public void DeleteProgram(uint program)
        {
            if (delegateDeleteProgram == null)
                delegateDeleteProgram = GetDelegate<glDeleteProgram>() as glDeleteProgram;
            delegateDeleteProgram(program);
        }
        public int GetAttribLocation(uint program, string name)
        {
            if (delegateGetAttribLocation == null)
                delegateGetAttribLocation = GetDelegate<glGetAttribLocation>() as glGetAttribLocation;
            return delegateGetAttribLocation(program, name);
        }
        public void UseProgram(uint program)
        {
            if (delegateUseProgram == null)
                delegateUseProgram = GetDelegate<glUseProgram>() as glUseProgram;
            delegateUseProgram(program);
        }
        public void GetProgram(uint program, uint pname, int[] parameters)
        {
            if (delegateGetProgram == null)
                delegateGetProgram = GetDelegate<glGetProgramiv>() as glGetProgramiv;
            delegateGetProgram(program, pname, parameters);
        }
        public void GetProgramInfoLog(uint program, int bufSize, IntPtr length, StringBuilder infoLog)
        {
            if (delegateGetProgramInfoLog == null)
                delegateGetProgramInfoLog = GetDelegate<glGetProgramInfoLog>() as glGetProgramInfoLog;
            delegateGetProgramInfoLog(program, bufSize, length, infoLog);
        }
        public void Uniform1(int location, int v0)
        {
            if (delegateUniform1i == null)
                delegateUniform1i = GetDelegate<glUniform1i>() as glUniform1i;
            delegateUniform1i(location, v0);
        }
        public void Uniform1(int location, float v0)
        {
            if (delegateUniform1f == null)
                delegateUniform1f = GetDelegate<glUniform1f>() as glUniform1f;
            delegateUniform1f(location, v0);
        }
        public void Uniform2(int location, float v0, float v1)
        {
            if (delegateUniform2 == null)
                delegateUniform2 = GetDelegate<glUniform2f>() as glUniform2f;
            delegateUniform2(location, v0, v1);
        }
        public void Uniform3(int location, float v0, float v1, float v2)
        {
            if (delegateUniform3 == null)
                delegateUniform3 = GetDelegate<glUniform3f>() as glUniform3f;
            delegateUniform3(location, v0, v1, v2);
        }
        public void Uniform4(int location, float v0, float v1, float v2, float v3)
        {
            if (delegateUniform4 == null)
                delegateUniform4 = GetDelegate<glUniform4f>() as glUniform4f;
            delegateUniform4(location, v0, v1, v2, v3);
        }
        public void UniformMatrix4(int location, int count, bool transpose, float[] value)
        {
            if (delegateUniformMatrix4 == null)
                delegateUniformMatrix4 = GetDelegate<glUniformMatrix4fv>() as glUniformMatrix4fv;
            delegateUniformMatrix4(location, count, transpose, value);
        }
        public int GetUniformLocation(uint program, string name)
        {
            if (delegateGetUniformLocation == null)
                delegateGetUniformLocation = GetDelegate<glGetUniformLocation>() as glGetUniformLocation;
            return delegateGetUniformLocation(program, name);
        }
        public IntPtr CreateContextAttribsARB(IntPtr hDC, IntPtr hShareContext, int[] attribList)
        {
            if (delegateCreateContextAttribsARB == null)
                delegateCreateContextAttribsARB = GetDelegate<wglCreateContextAttribsARB>() as wglCreateContextAttribsARB;
            return delegateCreateContextAttribsARB(hDC, hShareContext, attribList);
        }
        public void GenerateMipmapEXT(uint target)
        {
            if (delegateGenerateMipmapEXT == null)
                delegateGenerateMipmapEXT = GetDelegate<glGenerateMipmapEXT>() as glGenerateMipmapEXT;
            delegateGenerateMipmapEXT(target);
        }

        #endregion

        #region Wrapped OpenGL Functions

        /// <summary>
        /// Call this function after creating a texture to finalise creation of it, 
        /// or to make an existing texture current.
        /// </summary>
        /// <param name="target">The target type, e.g TEXTURE_2D.</param>
        /// <param name="texture">The OpenGL texture object.</param>
        public void BindTexture(uint target, uint texture) 
            => glBindTexture(target, texture);

        /// <summary>
		/// This function deletes a set of Texture objects.
		/// </summary>
		/// <param name="n">Number of textures to delete.</param>
		/// <param name="textures">The array containing the names of the textures to delete.</param>
		public void DeleteTextures(int n, uint[] textures)
            => glDeleteTextures(n, textures);

        /// <summary>
		/// This function sets the current blending function.
		/// </summary>
		/// <param name="sfactor">Source factor.</param>
		/// <param name="dfactor">Destination factor.</param>
		public void BlendFunc(uint sfactor, uint dfactor) 
            => glBlendFunc(sfactor, dfactor);

        /// <summary>
		/// This function clears the buffers specified by mask.
		/// </summary>
		/// <param name="mask">Which buffers to clear.</param>
		public void Clear(uint mask) => glClear(mask);

        /// <summary>
		/// This function sets the color that the drawing buffer is 'cleared' to.
		/// </summary>
		/// <param name="red">Red component of the color (between 0 and 1).</param>
		/// <param name="green">Green component of the color (between 0 and 1).</param>
		/// <param name="blue">Blue component of the color (between 0 and 1)./</param>
		/// <param name="alpha">Alpha component of the color (between 0 and 1).</param>
		public void ClearColor(float red, float green, float blue, float alpha) 
            => glClearColor(red, green, blue, alpha);

        /// <summary>
        /// Specify the clear value for the depth buffer.
        /// </summary>
        /// <param name="depth">Specifies the depth value used	when the depth buffer is cleared. The initial value is 1.</param>
		public void ClearDepth(double depth) => glClearDepth(depth);

        /// <summary>
		/// Call this function to enable an OpenGL capability.
		/// </summary>
		/// <param name="cap">The capability you wish to enable.</param>
		public void Enable(uint cap) => glEnable(cap);

        /// <summary>
		/// This function sets the current depth buffer comparison function, the default it LESS.
		/// </summary>
		/// <param name="func">The comparison function to set.</param>
		public void DepthFunc(uint func) => glDepthFunc(func);

        /// <summary>
		/// Call this function to disable an OpenGL capability.
		/// </summary>
		/// <param name="cap">The capability to disable.</param>
		public void Disable(uint cap) => glDisable(cap);

        /// <summary>
        /// Render	primitives from	array data.
        /// </summary>
        /// <param name="mode">Specifies what kind of primitives to render. Symbolic constants OpenGL.POINTS, OpenGL.LINE_STRIP, OpenGL.LINE_LOOP, OpenGL.LINES, OpenGL.TRIANGLE_STRIP, OpenGL.TRIANGLE_FAN, OpenGL.TRIANGLES, OpenGL.QUAD_STRIP, OpenGL.QUADS, and OpenGL.POLYGON are accepted.</param>
        /// <param name="first">Specifies the starting	index in the enabled arrays.</param>
        /// <param name="count">Specifies the number of indices to be rendered.</param>
		public void DrawArrays(uint mode, int first, int count) => glDrawArrays(mode, first, count);

        /// <summary>
        /// Render primitives from array data. Uses OpenGL.GL_UNSIGNED_INT as the data type.
        /// </summary>
        /// <param name="mode">Specifies what kind of primitives to	render. Symbolic constants OpenGL.POINTS, OpenGL.LINE_STRIP, OpenGL.LINE_LOOP, OpenGL.LINES, OpenGL.TRIANGLE_STRIP, OpenGL.TRIANGLE_FAN, OpenGL.TRIANGLES, OpenGL.QUAD_STRIP, OpenGL.QUADS, and OpenGL.POLYGON are accepted.</param>
        /// <param name="count">Specifies the number of elements to be rendered.</param>
        /// <param name="indices">Specifies a pointer to the location where the indices are stored.</param>
        public void DrawElements(uint mode, int count, uint[] indices) 
            => glDrawElements(mode, count, GL_UNSIGNED_INT, indices);

        /// <summary>
        /// Render primitives from array data.
        /// </summary>
        /// <param name="mode">Specifies what kind of primitives to	render. Symbolic constants OpenGL.GL_POINTS, OpenGL.GL_LINE_STRIP, OpenGL.GL_LINE_LOOP, OpenGL.GL_LINES, OpenGL.GL_TRIANGLE_STRIP, OpenGL.TRIANGLE_FAN, OpenGL.GL_TRIANGLES, OpenGL.GL_QUAD_STRIP, OpenGL.GL_QUADS, and OpenGL.GL_POLYGON are accepted.</param>
        /// <param name="count">Specifies the number of elements to be rendered.</param>
        /// <param name="type">Specifies the type of the values in indices.	Must be one of OpenGL.GL_UNSIGNED_BYTE, OpenGL.GL_UNSIGNED_SHORT, or OpenGL.GL_UNSIGNED_INT.</param>
        /// <param name="indices">Specifies a pointer to the location where the indices are stored.</param>
        public void DrawElements(uint mode, int count, uint type, IntPtr indices)
            => glDrawElements(mode, count, type, indices);

        /// <summary>
		/// Use this function to query OpenGL parameter values.
		/// </summary>
		/// <param name="pname">The Parameter to query</param>
		/// <param name="parameters">An array to put the values into.</param>
		public void GetInteger(uint pname, int[] parameters) => glGetIntegerv(pname, parameters);

        /// <summary>
		/// Create a set of unique texture names.
		/// </summary>
		/// <param name="n">Number of names to create.</param>
		/// <param name="textures">Array to store the texture names.</param>
		public void GenTextures(int n, uint[] textures) => glGenTextures(n, textures);

        /// <summary>
        /// Specify implementation-specific hints.
        /// </summary>
        /// <param name="target">Specifies a symbolic constant indicating the behavior to be controlled.</param>
        /// <param name="mode">Specifies a symbolic constant indicating the desired behavior.</param>
		public void Hint(uint target, uint mode) => glHint(target, mode);

        /// <summary>
		/// Call this function to load the identity matrix into the current matrix stack.
		/// </summary>
		public void LoadIdentity() => glLoadIdentity();

        /// <summary>
		/// Set the current matrix mode (the matrix that matrix operations will be 
		/// performed on).
		/// </summary>
		/// <param name="mode">The mode, normally PROJECTION or MODELVIEW.</param>
		public void MatrixMode(uint mode) => glMatrixMode(mode);

        /// <summary>
		/// This function creates an orthographic projection matrix (i.e one with no 
		/// perspective) and multiplies it to the current matrix stack, which would
		/// normally be 'PROJECTION'.
		/// </summary>
		/// <param name="left">Left clipping plane.</param>
		/// <param name="right">Right clipping plane.</param>
		/// <param name="bottom">Bottom clipping plane.</param>
		/// <param name="top">Top clipping plane.</param>
		/// <param name="zNear">Near clipping plane.</param>
		/// <param name="zFar">Far clipping plane.</param>
		public void Ortho(double left, double right, double bottom,
            double top, double zNear, double zFar) 
            => glOrtho(left, right, bottom, top, zNear, zFar);

        /// <summary>
        /// Set pixel storage modes.
        /// </summary>
        /// <param name="pname">Specifies the symbolic	name of	the parameter to be set.</param>
        /// <param name="param">Specifies the value that pname	is set to.</param>
        public void PixelStore(uint pname, int param) => glPixelStorei(pname, param);

        /// <summary>
        /// Select flat or smooth shading.
        /// </summary>
        /// <param name="mode">Specifies a symbolic value representing a shading technique. Accepted values are OpenGL.FLAT and OpenGL.SMOOTH. The default is OpenGL.SMOOTH.</param>
		public void ShadeModel(uint mode) => glShadeModel(mode);

        /// <summary>
		/// This function sets the image for the currently binded texture.
		/// </summary>
		/// <param name="target">The type of texture, TEXTURE_2D or PROXY_TEXTURE_2D.</param>
		/// <param name="level">For mip-map textures, ordinary textures should be '0'.</param>
		/// <param name="internalformat">The format of the data you are want OpenGL to create, e.g  RGB16.</param>
		/// <param name="width">The width of the texture image (must be a power of 2, e.g 64).</param>
		/// <param name="height">The height of the texture image (must be a power of 2, e.g 32).</param>
		/// <param name="border">The width of the border (0 or 1).</param>
		/// <param name="format">The format of the data you are passing, e.g. RGBA.</param>
		/// <param name="type">The type of data you are passing, e.g GL_BYTE.</param>
		/// <param name="pixels">The actual pixel data.</param>
		public void TexImage2D(uint target, int level, uint internalformat, int width, int height, int border, uint format, uint type, byte[] pixels)
            => glTexImage2D(target, level, internalformat, width, height, border, format, type, pixels);

        /// <summary>
		///	This function sets the parameters for the currently binded texture object.
		/// </summary>
		/// <param name="target">The type of texture you are setting the parameter to, e.g. TEXTURE_2D</param>
		/// <param name="pname">The parameter to set.</param>
		/// <param name="param">The value to set it to.</param>
		public void TexParameter(uint target, uint pname, float param)
            => glTexParameterf(target, pname, param);

        /// <summary>
		/// This sets the viewport of the current Render Context. Normally x and y are 0
		/// and the width and height are just those of the control/graphics you are drawing
		/// to.
		/// </summary>
		/// <param name="x">Top-Left point of the viewport.</param>
		/// <param name="y">Top-Left point of the viewport.</param>
		/// <param name="width">Width of the viewport.</param>
		/// <param name="height">Height of the viewport.</param>
		public void Viewport(int x, int y, int width, int height) 
            => glViewport(x, y, width, height);

        /// <summary>
		/// This sets the current drawing mode of polygons (points, lines, filled).
		/// </summary>
		/// <param name="face">The faces this applies to (front, back or both).</param>
		/// <param name="mode">The mode to set to (points, lines, or filled).</param>
		public void PolygonMode(uint face, uint mode)
            => glPolygonMode(face, mode);

        #endregion

        #region Old

        /// <summary>
        /// Specify the Alpha Test function.
        /// </summary>
        /// <param name="func">Specifies the alpha comparison function. Symbolic constants OpenGL.NEVER, OpenGL.LESS, OpenGL.EQUAL, OpenGL.LEQUAL, OpenGL.GREATER, OpenGL.NOTEQUAL, OpenGL.GEQUAL and OpenGL.ALWAYS are accepted. The initial value is OpenGL.ALWAYS.</param>
        /// <param name="reference">Specifies the reference	value that incoming alpha values are compared to. This value is clamped to the range 0	through	1, where 0 represents the lowest possible alpha value and 1 the highest possible value. The initial reference value is 0.</param>
        public void AlphaFunc(uint func, float reference)
            => glAlphaFunc(func, reference);

        /// <summary>
		/// Restore the previously saved state of the current matrix stack.
		/// </summary>
		public void PopMatrix() => glPopMatrix();

        /// <summary>
        /// Save the current state of the current matrix stack.
        /// </summary>
        public void PushMatrix() => glPushMatrix();

        /// <summary>
        /// Begin drawing geometry in the specified mode.
        /// </summary>
        /// <param name="mode">The mode to draw in, e.g. OpenGL.POLYGONS.</param>
        public void Begin(uint mode) => glBegin(mode);

        /// <summary>
		/// This function calls a certain display list.
		/// </summary>
		/// <param name="list">The display list to call.</param>
		public void CallList(uint list) => glCallList(list);

        /// <summary>
		/// Sets the current color.
		/// </summary>
		/// <param name="red">Red color component (between 0 and 1).</param>
		/// <param name="green">Green color component (between 0 and 1).</param>
		/// <param name="blue">Blue color component (between 0 and 1).</param>
		/// <param name="alpha">Alpha color component (between 0 and 1).</param>
		public void Color(float red, float green, float blue, float alpha) 
            => glColor4f(red, green, blue, alpha);

        /// <summary>
		/// This function generates 'range' number of contiguos display list indices.
		/// </summary>
		/// <param name="range">The number of lists to generate.</param>
		/// <returns>The first list.</returns>
		public uint GenLists(int range) => glGenLists(range);

        /// <summary>
		/// Signals the End of drawing.
		/// </summary>
		public void End() => glEnd();

        /// <summary>
		/// Ends the current display list compilation.
		/// </summary>
		public void EndList() => glEndList();

        /// <summary>
		/// This function starts compiling a new display list.
		/// </summary>
		/// <param name="list">The list to compile.</param>
		/// <param name="mode">Either COMPILE or COMPILE_AND_EXECUTE.</param>
		public void NewList(uint list, uint mode) => glNewList(list, mode);

        /// <summary>
		/// This function sets the current texture coordinates.
		/// </summary>
		/// <param name="s">Texture Coordinate.</param>
		/// <param name="t">Texture Coordinate.</param>
		public void TexCoord(float s, float t) => glTexCoord2f(s, t);

        /// <summary>
		/// This function applies a translation transformation to the current matrix.
		/// </summary>
		/// <param name="x">The amount to translate along the x axis.</param>
		/// <param name="y">The amount to translate along the y axis.</param>
		/// <param name="z">The amount to translate along the z axis.</param>
		public void Translate(float x, float y, float z)
            => glTranslatef(x, y, z);

        /// <summary>
        /// Set the current vertex (must be called between 'Begin' and 'End').
        /// </summary>
        /// <param name="x">X Value.</param>
        /// <param name="y">Y Value.</param>
        public void Vertex(int x, int y) => glVertex2i(x, y);
        public void Vertex(float x, float y) => glVertex2f(x, y);

        #endregion

        #region Вспомогательные методы для OpenGL

        /// <summary>
        /// Создать экземпляр OpenGL
        /// </summary>
        /// <param name="hDC">Приватный контекст устройства GDI</param>
        public void Create(IntPtr hDC, OpenGLVersion version)
        {
            // Установить контекст рендеринга
            hRC = wglCreateContext(hDC);
            if (hRC == IntPtr.Zero)
            {
                throw new Exception(Sr.CantCreateAOpenGLRenderingContext);
            }

            // Активировать контекст рендеринга
            if (wglMakeCurrent(hDC, hRC))
            {
                if (version == OpenGLVersion.OpenGL2_1)
                {
                    currentOpenGLInstance = this;
                }
                else
                {
                    int[] attributes =
                    {
                        WGL_CONTEXT_MAJOR_VERSION_ARB, version == OpenGLVersion.OpenGL3_3 ? 3 : 4,
                        WGL_CONTEXT_MINOR_VERSION_ARB, version == OpenGLVersion.OpenGL3_3 ? 3 : 6,
                        WGL_CONTEXT_FLAGS_ARB, WGL_CONTEXT_FORWARD_COMPATIBLE_BIT_ARB,
                        WGL_CONTEXT_PROFILE_MASK_ARB, WGL_CONTEXT_CORE_PROFILE_BIT_ARB
                    };
                    hRC = CreateContextAttribsARB(hDC, IntPtr.Zero, attributes);
                    if (wglMakeCurrent(hDC, hRC))
                    {
                        currentOpenGLInstance = this;
                    }
                    else
                    {
                        throw new Exception(Sr.GetString(Sr.CantActivateTheOpenGLRenderingContext,
                            version == OpenGLVersion.OpenGL3_3 ? "3.3." : "4.6"));
                    }
                }
            }
            else
            {
                throw new Exception(Sr.GetString(Sr.CantActivateTheOpenGLRenderingContext, "2.1"));
            }
        }

        /// <summary>
        /// Уничтожить
        /// </summary>
        public void Destroy()
        {
            if (hRC != IntPtr.Zero)
            {
                wglMakeCurrent(IntPtr.Zero, IntPtr.Zero);
                wglDeleteContext(hRC);
                hRC = IntPtr.Zero;
                currentOpenGLInstance = null;
            }
        }

        #endregion
    }
}
