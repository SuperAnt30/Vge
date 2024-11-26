using Vge.Json;
using WinGL.Util;

namespace Vge.World.Block.List
{
    /// <summary>
    /// Объект жидких блоков
    /// </summary>
    public class BlockLiquid : BlockBase
    {
        /// <summary>
        /// Стороны для прорисовки жидкого блока
        /// </summary>
        protected SideLiquid[] _sideLiquids;

        public BlockLiquid()
        {
            Alpha = true;
            Liquid = true;
            CullFaceAll = true;
        }

        /// <summary>
        /// Инициализация объекта рендера для блока
        /// </summary>
        protected override void _InitBlockRender()
            //=> BlockRender = Gi.BlockAlphaRendFull;
            => BlockRender = Gi.BlockLiquidAlphaRendFull;

        /// <summary>
        /// Имеется ли отбраковка конкретноц стороны, конкретного варианта
        /// </summary>
        public override bool IsCullFace(uint met, int indexSide) => false;
        /// <summary>
        /// Надо ли принудительно рисовать сторону, конкретного варианта
        /// </summary>
        public override bool IsForceDrawFace(uint met, int indexSide) => false;
        /// <summary>
        /// Надо ли принудительно рисовать не крайнюю сторону, конкретного варианта
        /// </summary>
        public override bool IsForceDrawNotExtremeFace(uint met, int indexSide) => false;


        /// <summary>
        /// Получить модель
        /// </summary>
        protected override void _ShapeDefinition(JsonCompound state, JsonCompound shapes)
        {
            BlockShapeDefinition shapeDefinition = new BlockShapeDefinition(this);
            _sideLiquids = shapeDefinition.RunShapeLiquidFromJson(state, shapes);
            _maskCullFaces = shapeDefinition.MaskCullFaces;

            //base._ShapeDefinition(state, shapes);

            //_sideLiquids = new SideLiquid[]
            //{
            //    new SideLiquid(0, 0, 0),
            //    new SideLiquid(1, 0, 0),
            //    new SideLiquid(2, 0, 0),
            //    new SideLiquid(3, 0, 0),
            //    new SideLiquid(4, 0, 0),
            //    new SideLiquid(5, 0, 0)
            //};
        }

        /// <summary>
        /// Получить сторону для прорисовки жидкого блока
        /// </summary>
        public override SideLiquid GetSideLiquid(int index) => _sideLiquids[index];


        /// <summary>
        /// Получить угол течения
        /// </summary>
        public static float GetAngleFlow(int l11, int l01, int l10, int l12, int l21)
        {
            Vector3 vec = new Vector3(0);
            if (l11 > 0)
            {
                if (l01 > 0) vec.X -= l11 - l01;
                if (l10 > 0) vec.Z -= l11 - l10;
                if (l21 > 0) vec.X += l11 - l21;
                if (l12 > 0) vec.Z += l11 - l12;
                vec.Y -= 6f;
                vec = vec.Normalize();
            }

            return (vec.X == 0f && vec.Z == 0f) ? -1000f : Glm.Atan2(vec.Z, vec.X) - Glm.Pi90;
        }
    }
}
