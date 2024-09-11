using System.Numerics;
using Vge.Gui.Controls;
using Vge.Renderer;

namespace Vge.Gui.Screens
{
    /// <summary>
    /// Экран основного меню 
    /// </summary>
    public class ScreenNotification : ScreenBase
    {
        private readonly string notification;
        private MeshGuiColor mesh;
        private int si = -1;
        private int count;

        private Button button;

        public ScreenNotification(WindowMain window, string notification) : base(window)
        {
            this.notification = notification;
            button = new Button(window, 200, 40, "Кнопка супер Tag");
            button.Click += Button_Click;
        }

        private void Button_Click(object sender, System.EventArgs e)
        {
            window.ScreenMainMenu();
        }

        /// <summary>
        /// Запускается при создании объекта и при смене режима FullScreen
        /// </summary>
        protected override void OnInitialize()
        {
            base.OnInitialize();
            AddControls(button);
        }

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        protected override void OnResized()
        {
            button.SetPosition(Gi.Width / 2 - 200 * Gi.Si, Gi.Height / 4 + 44 * Gi.Si);
        }

        public override void OnTick(float deltaTime)
        {
            //if (count++ > 30)
            //{
            //    window.ScreenMainMenu();
            //}
        }

        public override void Draw(float timeIndex)
        {
            base.Draw(timeIndex);

            gl.ClearColor(.5f, .2f, .2f, 1f);

            RenderMain render = window.Render;

            render.FontMain.Clear();

            render.ShaderBindGuiColor();

            if (si != Gi.Si)
            {
                si = Gi.Si;
                if (mesh == null)
                {
                    mesh = new MeshGuiColor(gl);
                }
                render.FontMain.SetColor(new Vector3(.9f, .9f, .9f)).SetFontFX(Renderer.Font.EnumFontFX.Shadow);
                render.FontMain.RenderText(10 * si, 10 * si, notification);
                render.FontMain.RenderFX();
                render.FontMain.Reload(mesh);
            }

            render.BindTextureFontMain();
            mesh.Draw();
        }

        public override void Dispose()
        {
            mesh.Dispose();
        }
    }
}
