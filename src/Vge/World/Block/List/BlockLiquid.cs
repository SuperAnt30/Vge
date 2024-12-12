using Vge.Json;
using WinGL.Util;

namespace Vge.World.Block.List
{
    /// <summary>
    /// Объект жидких блоков.
    /// Всё жидкости должны быть в отдельной маске для смещения защиты от Z-Fighting
    /// </summary>
    public class BlockLiquid : BlockBase
    {
        /// <summary>
        /// Стороны для прорисовки жидкого блока
        /// </summary>
        protected SideLiquid[] _sideLiquids;

        public BlockLiquid(bool alphaSort)
        {
            Alpha = true;
            AlphaSort = alphaSort;
            Liquid = true;
            FullBlock = false;
            CullFaceAll = true;
            LiquidOutside = 1024;
            NotLiquidOutside = new int[] { 0, 0, 0, 0, 0, 0 };
        }

        /// <summary>
        /// Инициализация объекта рендера для блока
        /// </summary>
        protected override void _InitBlockRender()
            => BlockRender = Gi.BlockLiquidAlphaRendFull;

        /// <summary>
        /// Имеется ли отбраковка конкретной стороны, конкретного варианта
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
        /// Проверка масок сторон
        /// </summary>
        public override bool ChekMaskCullFace(int indexSide, uint met, BlockBase blockSide, uint metSide) => false;

        /// <summary>
        /// Получить модель
        /// </summary>
        protected override void _ShapeDefinition(JsonCompound state, JsonCompound shapes)
        {
            BlockShapeDefinition shapeDefinition = new BlockShapeDefinition(this);
            _sideLiquids = shapeDefinition.RunShapeLiquidFromJson(state, shapes);
            _maskCullFaces = shapeDefinition.MaskCullFaces;
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
                // 14 это ограничение стыка между разными типами жидкости, для блокировки волны
                if (l11 == 14) l11 = 15;
                if (l01 > 0)
                {
                    if (l01 == 14) vec.X -= l11 - 15;
                    else vec.X -= l11 - l01;
                }
                if (l10 > 0)
                {
                    if (l10 == 14) vec.Z -= l11 - 15;
                    else vec.Z -= l11 - l10;
                }
                if (l21 > 0)
                {
                    if (l21 == 14) vec.X += l11 - 15;
                    else vec.X += l11 - l21;
                }
                if (l12 > 0)
                {
                    if (l12 == 14) vec.Z += l11 - 15;
                    else vec.Z += l11 - l12;
                }
                vec.Y -= 6f;
                vec = vec.Normalize();
            }

            return (vec.X == 0f && vec.Z == 0f) ? -1000f : Glm.Atan2(vec.Z, vec.X) - Glm.Pi90;
        }
    }
}
