using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using Vge.Renderer.World;
using Vge.Util;

namespace Vge.World.Block
{
    /// <summary>
    /// Базовый объект Блока
    /// </summary>
    public class BlockBase
    {
        /// <summary>
        /// Индекс блока из таблицы
        /// </summary>
        public ushort Id { get; private set; }
        /// <summary>
        /// Псевдоним блока из таблицы
        /// </summary>
        public string Alias { get; private set; }

        /// <summary>
        /// Блок рендера
        /// </summary>
        public BlockRenderFull BlockRender;

        #region Группа

        // У блока может быть один из параметров true FullBlock || Liquid || IsUnique || IsAir

        /// <summary>
        /// Ограничительная рамка занимает весь блок, для оптимизации, без проверки AABB блока
        /// </summary>
        public bool FullBlock { get; private set; } = true;
        /// <summary>
        /// Блок жидкости: вода, лава, нефть
        /// </summary>
        public bool Liquid { get; private set; } = false;
        /// <summary>
        /// Является ли эта модель не блоком, со всеми сторонами и прозрачной
        /// </summary>
        public bool IsUnique { get; private set; } = false;
        /// <summary>
        /// Явлыется ли блок небом
        /// </summary>
        public bool IsAir { get; private set; } = false;

        #endregion

        #region Для логики

        /// <summary>
        /// Имеет ли блок данные
        /// </summary>
        public bool IsMetadata;

        /// <summary>
        /// Сколько света вычитается для прохождения этого блока Air = 0
        /// В VoxelEngine он в public static byte GetBlockLightOpacity(EnumBlock eblock)
        /// получть инфу не создавая блок
        /// </summary>
        public byte LightOpacity { get; protected set; } = 15;
        /// <summary>
        /// Количество излучаемого света (плафон)
        /// </summary>
        public int LightValue { get; protected set; }
        /// <summary>
        /// Отмечает, относится ли этот блок к типу, требующему случайной пометки в тиках. 
        /// Объект ChunkStorage подсчитывает блоки, чтобы в целях эффективности отобрать фрагмент из 
        /// случайного списка обновлений фрагментов.
        /// </summary>
        public bool NeedsRandomTick { get; protected set; }
        /// <summary>
        /// Может на этот блок поставить другой, к примеру трава
        /// </summary>
        public bool IsReplaceable { get; protected set; } = false;

        #endregion

        /// <summary>
        /// Блок не прозрачный
        /// Для рендера и RayCast
        /// </summary>
        public bool IsNotTransparent { get; protected set; }
        /// <summary>
        /// Индекс картинки частички
        /// </summary>
        public int Particle { get; protected set; } = 0;
        /// <summary>
        /// Имеется ли у блока частичка
        /// </summary>
        public bool IsParticle { get; protected set; } = true;

        #region Для физики

        /// <summary>
        /// Можно ли выбирать блок
        /// </summary>
        public bool IsAction { get; protected set; } = true;
        /// <summary>
        /// Может ли блок сталкиваться
        /// </summary>
        public bool IsCollidable { get; protected set; } = true;

        #endregion

        #region Для Render

        /// <summary>
        /// Прорисовка возможно с обеих сторон, для уникальных блоков, типа трава, листва и подобное
        /// </summary>
        //public bool BothSides { get; private set; } = false;
        /// <summary>
        /// Полупрозрачный, альфа блок, вода, стекло...
        /// </summary>
        public bool Translucent = false;

        /// <summary>
        /// Флаг, если блок должен использовать самое яркое значение соседнего света как свое собственное
        /// Пример: листва, вода, стекло
        /// </summary>
        public bool UseNeighborBrightness = false;

        /// <summary>
        /// Все стороны принудительно, пример: трава, стекло, вода, лава
        /// *** Продумать, возможно заменить...
        /// </summary>
        public bool AllSideForcibly = false;
        /// <summary>
        /// При значении flase у AllSideForcibly + обнотипные блоков не будет между собой сетки, пример: вода, блок стекла
        /// *** Продумать, возможно заменить...
        /// </summary>
        public bool BlocksNotSame = true;

        /// <summary>
        /// Обрабатывается блок эффектом АmbientOcclusion
        /// </summary>
        public bool АmbientOcclusion = true;
        /// <summary>
        /// Обрабатывается блок эффектом Плавного перехода цвета между биомами
        /// </summary>
        public bool BiomeColor = false;
        /// <summary>
        /// Может ли быть тень сущности на блоке, только для целых блоков
        /// </summary>
        public bool Shadow { get; protected set; } = true;

        /// <summary>
        /// Стороны целого блока для прорисовки блока quads
        /// </summary>
        protected QuadSide[][] _quads = new QuadSide[][] { new QuadSide[] { new QuadSide() } };

        #endregion

        #region Init

        /// <summary>
        /// Инициализировать блок
        /// </summary>
        public virtual void Initialization(ushort id, string alias)
        {
            Id = id;
            Alias = alias;
            BlockRender = Gi.BlockRendFull;
            // Задать что блок не прозрачный
            if (LightOpacity > 13) IsNotTransparent = true;


        }

        /// <summary>
        /// Дополнительная инициализация блока после инициализации предметов
        /// </summary>
        public virtual void InitializationAfterItems() { }

        #endregion

        #region Методы для физики

        /// <summary>
        /// Является ли блок проходимым, т.е. можно ли ходить через него
        /// </summary>
        public virtual bool IsPassable(uint met) => false;

        /// <summary>
        /// Является ли блок проходимым на нём, т.е. можно ли ходить по нему
        /// </summary>
        public virtual bool IsPassableOnIt(uint met) => !IsPassable(met);

        #endregion

        #region Методы для группы

        /// <summary>
        /// Задать блок воздуха
        /// </summary>
        protected void SetAir()
        {
           // Material = Materials.GetMaterialCache(EnumMaterial.Air);
            IsAir = true;
            FullBlock = false;
            IsAction = false;
            IsParticle = false;
            АmbientOcclusion = false;
            Shadow = false;
            IsReplaceable = true;
            LightOpacity = 0;
            //canDropPresent = false;
        }

        #endregion

        #region Методы для Render

        /// <summary>
        /// Стороны целого блока для рендера
        /// </summary>
        public virtual QuadSide[] GetQuads(uint met, int xb, int zb) => _quads[0];

        /// <summary>
        /// Инициализация коробок всех одной текстурой с параметром Нет бокового затемнения, пример: трава, цветы
        /// </summary>
        protected void _InitQuads(int numberTexture)
        {
            _quads = new QuadSide[][] { new QuadSide[] {
                new QuadSide().SetTexture(numberTexture).SetSide(Pole.Up),
                new QuadSide().SetTexture(numberTexture).SetSide(Pole.Down),
                new QuadSide().SetTexture(numberTexture).SetSide(Pole.East),
                new QuadSide().SetTexture(numberTexture).SetSide(Pole.West),
                new QuadSide().SetTexture(numberTexture).SetSide(Pole.North),
                new QuadSide().SetTexture(numberTexture).SetSide(Pole.South)
            } };
        }

        #endregion

        public override string ToString() => Id.ToString() + " " + Alias;
    }
}
