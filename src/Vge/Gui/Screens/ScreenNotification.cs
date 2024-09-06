using System.Numerics;
using Vge.Renderer;

namespace Vge.Gui.Screens
{
    /// <summary>
    /// Экран основного меню 
    /// </summary>
    public class ScreenNotification : ScreenBase
    {
        private readonly string notification;
        private Mesh2d mesh;
        private int si = -1;
        private int count;

        public ScreenNotification(WindowMain window, string notification) : base(window)
            => this.notification = notification;

        public override void OnTick(float deltaTime)
        {
            if (count++ > 30)
            {
                window.ScreenMainMenu();
            }
        }

        public override void Draw(float timeIndex)
        {
            gl.ClearColor(.5f, .2f, .2f, 1f);

            RenderMain render = window.Render;
            render.FontMain.BufferClear();

            render.shaderText.Bind(gl);
            render.shaderText.SetUniformMatrix4(gl, "projview", window.Ortho2D);

            if (si != Gi.Si)
            {
                si = Gi.Si;
                if (mesh == null)
                {
                    mesh = new Mesh2d(gl);
                }
                render.FontMain.RenderText(11 * si, 11 * si, notification, new Vector3(.2f, .2f, .2f));
                render.FontMain.RenderText(10 * si, 10 * si, notification, new Vector3(.9f, .9f, .9f));
                render.FontMain.Reload(mesh);
            }

            render.BindTexutreFontMain();
            mesh.Draw();
        }

        public override void Dispose()
        {
            mesh.Dispose();
        }
    }
}
