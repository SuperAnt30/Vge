using Mvk2.Entity.Inventory;
using Mvk2.Renderer;
using Vge.Games;
using Vge.Gui.Huds;
using Vge.Item;
using Vge.Renderer;
using Vge.Util;
using WinGL.OpenGL;

namespace Mvk2.Gui
{
    /// <summary>
    /// Индикация для Малювек 
    /// </summary> 
    public class HudMvk : HudDebug // HudBase HubDebug
    {
        private readonly RenderMvk _renderMvk;
        /// <summary>
        /// Инвентарь игрока малювек 2
        /// </summary>
        private readonly InventoryPlayerMvk _inventoryPlayer;

        /// <summary>
        /// Сетка фона инвентаря
        /// </summary>
        private readonly MeshGuiColor _meshInventoryBg;
        /// <summary>
        /// Сетка выбранного слота
        /// </summary>
        private readonly MeshGuiColor _meshInventorySelect;
        /// <summary>
        /// Рендер слотов
        /// </summary>
        private readonly RenderSlots _renderSlots;
        /// <summary>
        /// Список буфера для построения сеток
        /// </summary>
        private readonly ListFlout _listBuffer = new ListFlout();

        /// <summary>
        /// Объект OpenGL для элемента управления
        /// </summary>
        private readonly GL gl;

        /// <summary>
        /// Колличество корманов 
        /// </summary>
        private byte _limitPocket;

        public HudMvk(GameBase game, RenderMvk renderMvk) : base(game)
        {
            _renderMvk = renderMvk;
            gl = game.GetOpenGL();
            _meshInventoryBg = new MeshGuiColor(gl);
            _meshInventorySelect = new MeshGuiColor(gl);
            _renderSlots = new RenderSlots(renderMvk);

            //_font = _renderMvk.FontMain;
            _limitPocket = InventoryPlayerMvk.PocketCount;

            _inventoryPlayer = _game.Player.Inventory as InventoryPlayerMvk;

            // Изменён лимит карманов
            _inventoryPlayer.LimitPocketChanged += (sender, e) => _RenderInventoryBg();
            // Слота задан
            _inventoryPlayer.SlotSetted += (sender, e) => _RenderInventory();
            // Изменён предмет внешности
            _inventoryPlayer.OutsideChanged += (sender, e) => _RenderInventory();
        }

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        public override void OnResized(int width, int height)
        {
            base.OnResized(width, height);
            _RenderInventoryBg();
            _RenderInventory();
        }

        #region Render

        /// <summary>
        /// Рендер фона инвенторя
        /// </summary>
        protected virtual void _RenderInventoryBg()
        {
            _limitPocket = _inventoryPlayer.LimitPocket;
            int wc = Gi.Width / 2;
            int w = 222 * Gi.Si;
            int h = 50 * Gi.Si;
            int x = wc - w;
            int y = Gi.Height - h - 8 * Gi.Si;

            _listBuffer.Clear();

            // Элементы правой руки
            _listBuffer.AddRange(RenderFigure.Rectangle(x, y, wc + w, y + h,
                0, 0, .8671875f, .09765625f));

            // Блокировка слотов
            for (int i = _limitPocket; i < InventoryPlayerMvk.PocketCount; i++)
            {
                // 36 * 50
                int x2 = x + (i * 36 + 6) * Gi.Si;
                _listBuffer.AddRange(RenderFigure.Rectangle(x2, y, x2 + 36 * Gi.Si, y + h,
                    .25f, .125f, .3203125f, .22265625f));
            }

            // Левая рука 48 * 50
            _listBuffer.AddRange(RenderFigure.Rectangle(x - 56 * Gi.Si, y, x - 8 * Gi.Si, y + h,
                0, .125f, .09375f, .22265625f));

            // Заполняем сетку фона
            _meshInventoryBg.Reload(_listBuffer.GetBufferAll(), _listBuffer.Count);

            // Сетка выбранного слота правой руки
            x += Gi.Si;
            _meshInventorySelect.Reload(RenderFigure.Rectangle(x, y, x + 46 * Gi.Si, y + 47 * Gi.Si,
                .125f, .125f, .21484375f, .216796875f));
        }

        /// <summary>
        /// Рендер текст и урон инвенторя
        /// </summary>
        protected virtual void _RenderInventory()
        {
            // Перед рендером
            _renderSlots.BeforeRender();

            int x = Gi.Width / 2 / Gi.Si - 222;
            int y = Gi.Height / Gi.Si - 53;

            // Левая рука
            ItemStack itemStack = _inventoryPlayer.GetStackInSlot(InventoryPlayerMvk.PocketCount);
            _RenderItemStack(itemStack, x - 50, y);

            // Список правой руки
            for (int i = 0; i < _limitPocket; i++)
            {
                itemStack = _inventoryPlayer.GetStackInSlot(i);
                _RenderItemStack(itemStack, x + 6 + i * 36, y);
            }
            // После рендера
            _renderSlots.AfterRender();
        }

        /// <summary>
        /// Рендер одной ячейки слота, текста количества и полоску урона
        /// </summary>
        private void _RenderItemStack(ItemStack itemStack, int x, int y)
        {
            if (itemStack != null)
            {
                _renderSlots.TextRenderCheck(itemStack, x, y);
                _renderSlots.DamageRender(itemStack, x, y);
            }
        }

        #endregion

        /// <summary>
        /// Метод для прорисовки кадра
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public override void Draw(float timeIndex)
        {
            base.Draw(timeIndex);

            _game.Render.DepthOff();
            // Фон
            _renderMvk.ShaderBindGuiColor();
            _renderMvk.BindTextureHud();
            _meshInventoryBg.Draw();

            // Заносим в шейдор
            _game.Render.ShsEntity.BindUniformBeginGui();
            // Всё 2d закончено, рисуем 3д элементы в Gui
            _game.Render.DepthOn();

            // Предметы
            int size = 36 * Gi.Si;
            int x = Gi.Width / 2 - 222 * Gi.Si + 24 * Gi.Si;
            int y = Gi.Height - 58 * Gi.Si + 23 * Gi.Si;
            ItemStack itemStack;
            // Перечень правой руки
            for (int i = 0; i < _limitPocket; i++)
            {
                // Прорисовка предмета в стаке если есть
                itemStack = _inventoryPlayer.GetStackInSlot(i);
                if (itemStack != null)
                {
                    _game.WorldRender.Entities.GetItemGuiRender(itemStack.Item.IndexItem).MeshDraw(x + i * size, y);
                }
            }
            // Левая рука
            itemStack = _inventoryPlayer.GetStackInSlot(InventoryPlayerMvk.PocketCount);
            if (itemStack != null)
            {
                _game.WorldRender.Entities.GetItemGuiRender(itemStack.Item.IndexItem).MeshDraw(x - 56 * Gi.Si, y);
            }

            _game.Render.DepthOff();

            _renderSlots.Draw(0, 0);

            // Выбранный
            int index = _game.Player.Inventory.GetCurrentIndex();
            _renderMvk.ShaderBindGuiColor(index * size, 0);
            _renderMvk.BindTextureHud();
            _meshInventorySelect.Draw();
            _renderMvk.ShaderBindGuiColor(0, 0);
            _game.Render.DepthOn();
        }

        public override void Dispose()
        {
            base.Dispose();
            _meshInventoryBg?.Dispose();
            _meshInventorySelect?.Dispose();
            _renderSlots.Dispose();
        }
    }
}
