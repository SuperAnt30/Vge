using System;
using Vge.Management;
using Vge.Util;
using WinGL.Util;

namespace Vge.Renderer.World
{
    /// <summary>
    /// Объект отвечает за прорисовку курсора блока и чанка в мире
    /// </summary>
    public class CursorRender : IDisposable
    {
        /// <summary>
        /// Объект рендера мира
        /// </summary>
        private readonly WorldRenderer _worldRenderer;
        /// <summary>
        /// Объект игрока на клиенте
        /// </summary>
        private readonly PlayerClient _player;
        /// <summary>
        /// Сетка курсора
        /// </summary>
        private readonly MeshLine _meshBlock;
        /// <summary>
        /// Скрыт ли курсор блока
        /// </summary>
        private bool _hiddenBlock = true;
        /// <summary>
        /// Позиция глаз прошлого такта
        /// </summary>
        private Vector3 _posPrev;
        /// <summary>
        /// Объект луча прошлого такта
        /// </summary>
        private MovingObjectPosition _movingObjectPrev;
        /// <summary>
        /// Сетка курсора
        /// </summary>
        private readonly MeshLine _meshChunk;
        /// <summary>
        /// Скрыт ли курсор чанка
        /// </summary>
        private bool _hiddenChunk = true;

        public CursorRender(PlayerClient player, WorldRenderer worldRenderer)
        {
            _worldRenderer = worldRenderer;
            _player = player;
            _meshBlock = new MeshLine(_worldRenderer.GetOpenGL());
            _meshChunk = new MeshLine(_worldRenderer.GetOpenGL());
        }

        public void Dispose()
        {
            _meshBlock.Dispose();
            _meshChunk.Dispose();
        }

        /// <summary>
        /// Прорисовка
        /// </summary>
        public void RenderDraw()
        {
            _RenderBlock();
            // Прорисовка
            if (!_hiddenBlock || !_hiddenChunk)
            {
                _worldRenderer.Render.ShaderBindLine(_player.View);
            }
            if (!_hiddenBlock)
            {
                _meshBlock.Draw();
            }
            if (!_hiddenChunk)
            {
                _meshChunk.Draw();
            }
        }

        /// <summary>
        /// Рендер блока
        /// </summary>
        private void _RenderBlock()
        {
            MovingObjectPosition moving = _player.MovingObject;
            if (moving.IsBlock())
            {
                _hiddenBlock = false;
                Vector3 pos = _player.PositionFrame.GetVector3();

                bool compiled = false;
                if (!_posPrev.Equals(pos))
                {
                    compiled = true;
                    _posPrev = pos;
                }
                if (!moving.Equals(_movingObjectPrev))
                {
                    compiled = true;
                    _movingObjectPrev = moving;
                }

                if (compiled)
                {
                    // Готовим сетку
                    Vector3 blockCenter = moving.BlockPosition.ToVector3Center();
                    float dis = Glm.Distance(pos, blockCenter) * .01f;
                    dis *= dis;
                    dis += .001f;
                    pos = blockCenter - pos;
                    Vector3 from = pos - .5f - dis;// b moving.BlockPosition.ToVector3() +  - 0.02f;
                    Vector3 to = pos + .5f + dis;

                    _meshBlock.Reload(MeshLine.CubeLine(from.X, from.Y, from.Z,
                        to.X, to.Y, to.Z, 1, 1, 0, 1));
                }
            }
            else
            {
                _hiddenBlock = true;
            }
        }

        
    }
}
