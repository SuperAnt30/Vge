using System;
using System.Collections.Generic;
using Vge.Entity.Player;
using Vge.Util;
using WinGL.OpenGL;
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
        private readonly PlayerClientOwner _player;
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
        private readonly MovingObjectPosition _movingObjectPrev = new MovingObjectPosition();
        /// <summary>
        /// Сетка курсора
        /// </summary>
        private readonly MeshLine _meshChunk;
        /// <summary>
        /// Скрыт ли курсор чанка
        /// </summary>
        private bool _hiddenChunk = true;
        /// <summary>
        /// Была ли сделана сетка для чанка
        /// </summary>
        private bool _renderChunk;
        /// <summary>
        /// Высота блоков в чанке
        /// </summary>
        private ushort _numberBlocks;

        public CursorRender(PlayerClientOwner player, WorldRenderer worldRenderer)
        {
            _worldRenderer = worldRenderer;
            _player = player;
            _meshBlock = new MeshLine(_worldRenderer.GetOpenGL(), GL.GL_DYNAMIC_DRAW);
            _meshChunk = new MeshLine(_worldRenderer.GetOpenGL(), GL.GL_STATIC_DRAW);
        }

        /// <summary>
        /// Задать высоту чанков
        /// </summary>
        public void SetHeightChunks(ushort numberBlocks)
        {
            _numberBlocks = numberBlocks;
            _renderChunk = true;
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
            _RenderChunk();
            // Прорисовка
            if (!_hiddenBlock)
            {
                _worldRenderer.Render.ShaderBindLine(_player.View, 0, 0, 0);
                _meshBlock.Draw();
            }
            if (!_hiddenChunk)
            {
                Vector2i posCh = new Vector2i(Mth.Floor(_player.PosFrameX) >> 4, Mth.Floor(_player.PosFrameZ) >> 4);
                posCh.X = posCh.X << 4;
                posCh.Y = posCh.Y << 4;
                _worldRenderer.Render.ShaderBindLine(_player.View,
                    posCh.X - _player.PosFrameX,
                    -_player.PosFrameY,
                    posCh.Y - _player.PosFrameZ);
                _meshChunk.Draw();
            }
        }

        /// <summary>
        /// Смена видимости курсора чанка
        /// </summary>
        public void ChunkCursorHiddenShow() => _hiddenChunk = !_hiddenChunk;

        /// <summary>
        /// Рендер блока
        /// </summary>
        private void _RenderBlock()
        {
            MovingObjectPosition moving = _player.MovingObject;
            if (moving.IsBlock())
            {
                _hiddenBlock = false;
                Vector3 pos = new Vector3(_player.PosFrameX, _player.PosFrameY, _player.PosFrameZ);

                bool compiled = false;
                if (!_posPrev.Equals(pos))
                {
                    compiled = true;
                    _posPrev = pos;
                }
                if (!moving.Equals(_movingObjectPrev))
                {
                    compiled = true;
                    _movingObjectPrev.Copy(moving);
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


        private void _RenderChunk()
        {
            if (!_hiddenChunk && _renderChunk)
            {
                _renderChunk = false;

                Vector3 colorYelow = new Vector3(1, 1, 0);
                Vector3 colorRed = new Vector3(1, 0, 0);
                Vector3 colorBlue = new Vector3(0, 0, 1);
                Vector3 color;

                int leght = _numberBlocks + 1;
                List<float> buffer = new List<float>();

                // вертикальные линии
                for (int i = 2; i < 16; i += 2)
                {
                    buffer.AddRange(_Line(0, 0, i, 0, leght, i, colorYelow));
                    buffer.AddRange(_Line(16, 0, i, 16, leght, i, colorYelow));
                    buffer.AddRange(_Line(i, 0, 0, i, leght, 0, colorYelow));
                    buffer.AddRange(_Line(i, 0, 16, i, leght, 16, colorYelow));
                }
                // вертикальные линии границы чанка
                buffer.AddRange(_Line(0, 0, 0, 0, leght, 0, colorBlue));
                buffer.AddRange(_Line(16, 0, 0, 16, leght, 0, colorBlue));
                buffer.AddRange(_Line(0, 0, 16, 0, leght, 16, colorBlue));
                buffer.AddRange(_Line(16, 0, 16, 16, leght, 16, colorBlue));

                // Кольца с низу вверх
                for (int i = 0; i <= leght; i += 2)
                {
                    color = i % 16 == 0 ? colorBlue : colorYelow;
                    buffer.AddRange(_Line(0, i, 0, 16, i, 0, color));
                    buffer.AddRange(_Line(0, i, 0, 0, i, 16, color));
                    buffer.AddRange(_Line(16, i, 0, 16, i, 16, color));
                    buffer.AddRange(_Line(0, i, 16, 16, i, 16, color));
                }
                // Вертикальные углы соседнего чанка
                for (int i = -16; i <= 32; i += 16)
                {
                    buffer.AddRange(_Line(i, 0, -16, i, leght, -16, colorRed));
                    buffer.AddRange(_Line(i, 0, 32, i, leght, 32, colorRed));
                }
                buffer.AddRange(_Line(-16, 0, 0, -16, leght, 0, colorRed));
                buffer.AddRange(_Line(-16, 0, 16, -16, leght, 16, colorRed));
                buffer.AddRange(_Line(32, 0, 0, 32, leght, 0, colorRed));
                buffer.AddRange(_Line(32, 0, 16, 32, leght, 16, colorRed));

                _meshChunk.Reload(buffer.ToArray());
            }
        }

        private float[] _Line(float x1, float y1, float z1,
            float x2, float y2, float z2, Vector3 color, float alpha = 1)
            => new float[]
            {
                x1, y1, z1, color.X, color.Y, color.Z, alpha,
                x2, y2, z2, color.X, color.Y, color.Z, alpha
            };
    }
}
