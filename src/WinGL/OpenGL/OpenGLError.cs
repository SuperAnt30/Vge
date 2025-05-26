namespace WinGL.OpenGL
{
    /// <summary>
    /// Перечень ошибок OpenGL комманды GetError
    /// </summary>
    public enum OpenGLError: uint
    {
        /// <summary>
        /// GL_NO_ERROR
        /// </summary>
        NoError = 0,
        /// <summary>
        /// GL_INVALID_ENUM
        /// </summary>
        InvalidEnum = 0x0500,
        /// <summary>
        /// GL_INVALID_VALUE
        /// </summary>
        InvalidValue = 0x0501,
        /// <summary>
        /// GL_INVALID_OPERATION
        /// </summary>
        InvalidOperation = 0x0502,
        /// <summary>
        /// GL_STACK_OVERFLOW
        /// </summary>
        StackOverflow = 0x0503,
        /// <summary>
        /// GL_STACK_UNDERFLOW
        /// </summary>
        StackUnderflow = 0x0504,
        /// <summary>
        /// GL_OUT_OF_MEMORY
        /// </summary>
        OutOfMemory = 0x0505,
        /// <summary>
        /// GL_INVALID_FRAMEBUFFER_OPERATION
        /// </summary>
        InvalidFramebufferOperation = 0x0506
    }
}
