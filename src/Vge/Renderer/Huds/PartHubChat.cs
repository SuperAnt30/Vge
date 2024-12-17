using Vge.Games;
using Vge.Realms;
using Vge.Renderer.Font;

namespace Vge.Renderer.Huds
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
            if (_chat.FlagUpdate || _isRender)
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
                    font.Clear();
                    font.SetFontFX(EnumFontFX.Outline);// EnumFontFX.Shadow).SetColor(new Vector3(.9f, .9f, .9f));
                    int h = Gi.Height - 40 * Gi.Si;
                    count--;
                    for (int i = count; i >= 0; i--)
                    {
                        font.RenderString(10 * Gi.Si, h, _chat.ChatLines[i].message);
                        h -= font.GetVert() + 4 * Gi.Si;
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
