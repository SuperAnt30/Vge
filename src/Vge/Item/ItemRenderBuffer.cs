using System;
using System.Runtime.CompilerServices;
using Vge.Entity.Render;
using Vge.Json;
using Vge.World.Block;

namespace Vge.Item
{
    /// <summary>
    /// Объект буферов для рендера
    /// </summary>
    public class ItemRenderBuffer
    {
        /// <summary>
        /// Объёмная ли фигура
        /// </summary>
        public bool Valume;

        /// <summary>
        /// Буфер сетки для рендера
        /// </summary>
        private VertexEntityBuffer _buffer;
        /// <summary>
        /// Буфер сетки для рендера для Gui
        /// </summary>
        private VertexEntityBuffer _bufferGui;
        /// <summary>
        /// Буфер сетки для рендера для сущности держать
        /// </summary>
        private VertexEntityBuffer _bufferHold;

        /// <summary>
        /// Инициализация предметов данные с json, 
        /// это не спрайти, а модель предмета или модель с блока
        /// </summary>
        public void InitAndShape(string alias, JsonCompound state, JsonCompound shape, bool isItem)
        {
            ItemShapeDefinition shapeDefinition = new ItemShapeDefinition(alias);
            _buffer = ItemShapeSprite.Convert(
                shapeDefinition.RunShapeItemFromJson(state.GetObject(Cti.View), shape, 1)
            );
            shapeDefinition = new ItemShapeDefinition(alias);
            _bufferHold = ItemShapeSprite.Convert(
                shapeDefinition.RunShapeItemFromJson(state.GetObject(Cti.ViewHold), shape, 1)
            );
        }

        /// <summary>
        /// Инициализация предметов данные с json, 
        /// это спрайт
        /// </summary>
        public void InitAndSprite(string alias, JsonCompound state)
        {
            ItemShapeSprite shapeSprite = new ItemShapeSprite(alias, state.GetString(Cti.Sprite));
            
            ShapeAdd shapeAdd = new ShapeAdd();
            shapeAdd.RunShape(state.GetObject(Cti.View), false, 1);
            _buffer = shapeSprite.GenBuffer(shapeAdd);

            shapeAdd = new ShapeAdd();
            shapeAdd.RunShape(state.GetObject(Cti.ViewHold), false, 1);
            _bufferHold = shapeSprite.GenBuffer(shapeAdd);

            //_bufferHold = _buffer.CopyBufferMesh();
        }

        /// <summary>
        /// Инициализация модели json для Gui, 
        /// это не спрайти, а модель предмета или модель с блока
        /// </summary>
        public void InitShapeGui(string alias, JsonCompound state, JsonCompound shape, int sizeSprite)
        {
            Valume = true;
            ItemShapeDefinition shapeDefinition = new ItemShapeDefinition(alias);
            _bufferGui = ItemShapeSprite.ConvertGui(
                shapeDefinition.RunShapeItemFromJson(state.GetObject(Cti.ViewGui), shape, sizeSprite)
            );
        }

        /// <summary>
        /// Инициализация модели json для Gui, 
        /// это спрайт
        /// </summary>
        public void InitSpriteGui(string alias, JsonCompound state, int sizeSprite)
        {
            ItemShapeSprite shapeSprite = new ItemShapeSprite(alias, state.GetString(Cti.SpriteGui));
            _bufferGui = shapeSprite.GenBufferGui(sizeSprite);
        }

        public void Clear()
        {
            _buffer = null;
            _bufferGui = null;
        }

        /// <summary>
        /// Получить буфер сетки предмета для рендера
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public VertexEntityBuffer GetBuffer() => _buffer;

        /// <summary>
        /// Получить буфер сетки предмета для рендера
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public VertexEntityBuffer GetBufferHold() => _bufferHold;

        /// <summary>
        /// Получить буфер сетки предмета для рендера Gui
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public VertexEntityBuffer GetBufferGui() => _bufferGui;
    }
}
