using System;
using System.Runtime.CompilerServices;
using Vge.Entity.Player;

namespace Vge.Renderer.World
{
    /// <summary>
    /// Объект отвечает за прорисовку неба
    /// </summary>
    public class SkyRender : IDisposable
    {
        /// <summary>
        /// Объект рендера мира
        /// </summary>
        protected readonly WorldRenderer _worldRenderer;
        /// <summary>
        /// Объект игрока на клиенте
        /// </summary>
        protected readonly PlayerClientOwner _player;

        public SkyRender(PlayerClientOwner player, WorldRenderer worldRenderer)
        {
            _worldRenderer = worldRenderer;
            _player = player;
        }

        /// <summary>
        /// Прорисовка неба
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void DrawSky(float timeIndex) { }

        /// <summary>
        /// Прорисовка облака неба
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void DrawClouds(float timeIndex) { }

        public virtual void Dispose() { }
    }
}
