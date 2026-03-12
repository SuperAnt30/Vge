using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vge.Games;
using WinGL.OpenGL;

namespace Vge.Renderer.World
{
    /// <summary>
    /// Объект рендера всех частичек
    /// </summary>
    public class ParticlesRenderer : WarpRenderer
    {
        /// <summary>
        /// Объект OpenGL для элемента управления
        /// </summary>
        private readonly GL gl;
        /// <summary>
        /// Количество всех частичек
        /// </summary>
        private int _countFx;

        public ParticlesRenderer(GameBase game) : base(game)
        {
            gl = GetOpenGL();
        }

        /// <summary>
        /// Игровой такт
        /// </summary>
        /// <param name="deltaTime">Дельта последнего тика в mc</param>
        public override void OnTick(float deltaTime)
        {
            // Нужно для эффектов (частичек)

        }

        public override void Dispose()
        {

        }

        public override string ToString() => "Fx:" + _countFx;
    }
}
