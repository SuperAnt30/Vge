using Vge.Util;
using WinGL.Util;

namespace Vge.Entity.Sizes
{
    /// <summary>
    /// Интерйейс объекта для размера, веса и прочего сущностей которая работает с физикой
    /// </summary>
    public interface ISizeEntity
    {
        /// <summary>
        /// Вес сущности для определения импулса между сущностями,
        /// У кого больше вес тот больше толкает или меньше потдаётся импульсу.
        /// В килограммах.
        /// </summary>
        int GetWeight();

        /// <summary>
        /// Пол ширины сущности
        /// </summary>
        float GetWidth();

        /// <summary>
        /// Высота сущности
        /// </summary>
        float GetHeight();

        /// <summary>
        /// Возвращает, пересекается ли данная ограничивающая рамка с этой сущностью
        /// </summary>
        bool IntersectsWith(AxisAlignedBB other);
        /// <summary>
        /// Возвращает, пересекается ли данная ограничивающая рамка с блоком
        /// </summary>
        bool IntersectsWithBlock(int x, int y, int z);

        /// <summary>
        /// Рассчитать точку пересечения Hitbox и отрезка, в виде вектора от pos1 до pos2
        /// </summary>
        PointIntersection CalculateIntercept(Vector3 pos1, Vector3 pos2);

        /// <summary>
        /// Если экземпляр и ограничивающие рамки аргумента перекрываются в измерениях Y и Z, 
        /// вычислите смещение между ними в измерении X. вернуть offset, если ограничивающие 
        /// рамки не перекрываются или если offset ближе к 0, чем вычисленное смещение. 
        /// В противном случае вернуть рассчитанное смещение.
        /// </summary>
        /// <param name="offset">смещение</param>
        float CalculateXOffset(AxisAlignedBB other, float offset);

        /// <summary>
        /// Если экземпляр и ограничивающие рамки аргумента перекрываются в измерениях X и Z, 
        /// вычислите смещение между ними в измерении Y. вернуть offset, если ограничивающие 
        /// рамки не перекрываются или если offset ближе к 0, чем вычисленное смещение. 
        /// В противном случае вернуть рассчитанное смещение.
        /// </summary>
        /// <param name="offset">смещение</param>
        float CalculateYOffset(AxisAlignedBB other, float offset);

        /// <summary>
        /// Если экземпляр и ограничивающие рамки аргумента перекрываются в измерениях Y и X, 
        /// вычислите смещение между ними в измерении Z. вернуть offset, если ограничивающие 
        /// рамки не перекрываются или если offset ближе к 0, чем вычисленное смещение. 
        /// В противном случае вернуть рассчитанное смещение.
        /// </summary>
        /// <param name="offset">смещение</param>
        float CalculateZOffset(AxisAlignedBB other, float offset);
    }

    /// <summary>
    /// Интерйейс объекта для размера, дополнение, чтоб создать рамку
    /// </summary>
    public interface ISizeEntityBox : ISizeEntity
    {
        /// <summary>
        /// Получить ограничительную рамку сущности
        /// </summary>
        AxisAlignedBB GetBoundingBox();
        /// <summary>
        /// Получить ограничительную рамку сущности со смещением
        /// </summary>
        AxisAlignedBB GetBoundingBoxOffset(float x, float y, float z);
    }

    /// <summary>
    /// Интерйейс объекта для размера, дополнение, глаза
    /// </summary>
    //public interface ISizeEntityEye : ISizeEntityBox
    //{
    //    /// <summary>
    //    /// Высота глаз
    //    /// </summary>
    //    float GetEye();
    //}

}
