namespace Vge.Json
{
    /// <summary>
    /// Интерфейс для возможных объектов в массиве
    /// </summary>
    public interface IJsonArray
    {
        bool IsValue();
    }

    /// <summary>
    /// Интерфейс для переменной, массива или объекта без key
    /// </summary>
    public interface IJson
    {
        byte GetIdType();
    }
}
