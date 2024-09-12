using Vge.Gui.Controls;
using Vge.Renderer.Font;

namespace Vge.Gui.Screens
{
    /// <summary>
    /// Экран основного меню 
    /// </summary>
    public class ScreenNotification : ScreenBase
    {
        private readonly Label label;
        private readonly Button button;

        public ScreenNotification(WindowMain window, string notification) : base(window)
        {
            FontBase font = window.Render.FontMain;
            label = new Label(window, font, window.Width - 100, 200, notification, true);
            label.Multiline().SetTextAlight(EnumAlight.Center, EnumAlightVert.Bottom);
            button = new Button(window, font, 200, 40, "Кнопка супер Tag");
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
            AddControls(label);
        }

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        protected override void OnResized()
        {
            button.SetPosition((window.Width / si - button.Width) / 2, window.Height / (si * 2) + 20);
            label.SetSize(window.Width / si - 100, label.Height);
            label.SetPosition(50, window.Height / (si * 2) - label.Height);
        }

        public override void Draw(float timeIndex)
        {
            gl.ClearColor(.5f, .2f, .2f, 1f);
            base.Draw(timeIndex);

            //RenderMain render = window.Render;

            //render.FontMain.Clear();

            //render.ShaderBindGuiColor();

            //if (si != Gi.Si)
            //{
            //    si = Gi.Si;
            //    if (mesh == null)
            //    {
            //        mesh = new MeshGuiColor(gl);
            //    }
            //    render.FontMain.SetColor(new Vector3(.9f, .9f, .9f)).SetFontFX(Renderer.Font.EnumFontFX.Shadow);
            //    render.FontMain.RenderText(10 * si, 10 * si, notification);
            //    render.FontMain.RenderFX();
            //    render.FontMain.Reload(mesh);
            //}

            //render.BindTextureFontMain();
            //mesh.Draw();
        }

        //public override void Dispose()
        //{
        //    mesh.Dispose();
        //}
    }
}
