using Vge.Games;
using Vge.Realms;
using Vge.Renderer;
using Vge.Renderer.Font;

namespace Vge.Gui.Huds
{
    /// <summary>
    /// Чат, часть Heads-Up Display
    /// </summary>
    public class PartHubChat : WarpRenderer
    {
        /// <summary>
        /// Скрытый
        /// </summary>
        public bool Hidden = false;

        /// <summary>
        /// Максимальное количество строк в чате
        /// </summary>
        protected int _countMaxLine = 24;

        private MeshGuiColor _meshText;
        /// <summary>
        /// Объект кэш чата
        /// </summary>
        private readonly ChatList _chat;
        /// <summary>
        /// Принудительный рендер нужен
        /// </summary>
        private bool _isRender;
        /// <summary>
        /// Пустая прорисовка, текста на экране нет
        /// </summary>
        private bool _isDrawEmpty = true;

        public PartHubChat(GameBase game) : base(game)
        {
            _meshText = new MeshGuiColor(game.GetOpenGL());
            _chat = game.Player.Chat;
        }

        /// <summary>
        /// Нужен принудительный рендер
        /// </summary>
        public void Renderer() => _isRender = true;

        /// <summary>
        /// Метод для прорисовки кадра
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public override void Draw(float timeIndex)
        {
            if (!Hidden && !_isDrawEmpty)
            {
                Render.ShaderBindGuiColor();
                _chat.Font.BindTexture();
                _meshText.Draw();
            }
        }

        /// <summary>
        /// Игровой такт
        /// </summary>
        /// <param name="deltaTime">Дельта последнего тика в mc</param>
        public override void OnTick(float deltaTime)
        {
            _chat.OnTick();
            if (!Hidden && (_chat.FlagUpdate || _isRender))
            {
                int count = _chat.ChatLines.Count;
                if (count == 0)
                {
                    // Пусто
                    _isDrawEmpty = true;
                }
                else
                {
                    _isDrawEmpty = false;
                    FontBase font = _chat.Font;
                    font.SetFontFX(EnumFontFX.Outline);
                    int h = Gi.Height - 40 * Gi.Si;
                    if (count > _countMaxLine) count = _countMaxLine;
                    string[] list = new string[count];
                    int[] line = new int[count];
                    count--;
                    int width = Gi.Width / 2 / Gi.Si;
                    // Готовим массив строк, разбивая их если длинные
                    for (int i = count; i >= 0; i--)
                    {
                        font.Transfer.Run(_chat.ChatLines[i].Message, width, Gi.Si);
                        list[i] = font.Transfer.OutText;
                        line[i] = font.Transfer.NumberLines;
                    }
                    // Рисуем
                    font.Clear(true, true);
                    int number = 0;
                    for (int i = count; i >= 0; i--)
                    {
                        h -= (font.GetVert() + 4 * Gi.Si) * line[i];
                        font.RenderText(10 * Gi.Si, h, list[i]);
                        if (++number >= _countMaxLine)
                        {
                            break;
                        }
                    }
                    font.RenderFX();
                    font.Reload(_meshText);
                    font.Clear();
                }

                _chat.Update();
                _isRender = false;
            }
        }

        public override void Dispose()
        {
            _meshText.Dispose();
        }
    }
}
