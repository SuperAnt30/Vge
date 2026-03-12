namespace Vge.Entity.Render
{
    /// <summary>
    /// Интерфейс отвечающий за сетку сущности
    /// </summary>
    public interface IEntityRender
    {
        /// <summary>
        /// Прорисовать сетку
        /// </summary>
        void MeshDraw();

        /// <summary>
        /// Выгрузить сетку с OpenGL
        /// </summary>
        void Dispose();
    }
}
