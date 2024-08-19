using System;

namespace WinGL.OpenGL
{
    [Serializable]
    public class ShaderCompilationException : Exception
    {
        public string CompilerOutput { get; private set; }

        public ShaderCompilationException(string compilerOutput)
        {
            CompilerOutput = compilerOutput;
        }
        public ShaderCompilationException(string message, string compilerOutput)
            : base(message)
        {
            CompilerOutput = compilerOutput;
        }
        public ShaderCompilationException(string message, Exception inner, string compilerOutput)
            : base(message, inner)
        {
            CompilerOutput = compilerOutput;
        }
        protected ShaderCompilationException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
