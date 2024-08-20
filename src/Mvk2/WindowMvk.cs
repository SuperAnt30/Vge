using System;
using WinGL.Util;
using WinGL.OpenGL;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using WinGL.Actions;
using System.Reflection;
using Vge.Renderer.Font;
using Vge.Renderer;
using Vge;
using System.Numerics;
using Mvk2.Util;
using Mvk2.Audio;

namespace Mvk2
{
    public class WindowMvk : WindowMain
    {
        /// <summary>
        /// Объект сетки курсора, временно
        /// </summary>
        private Mesh cursorVBO;
        /// <summary>
        /// Виден ли курсор
        /// </summary>
        private bool cursorShow = false;

        private AudioMvk audio = new AudioMvk();

        public WindowMvk() : base()
        {
            version = "Test VBO by Ant " + Assembly.GetExecutingAssembly().GetName().Version.ToString();

            //FullScreen = true;
            //  CursorShow(true);

            //WinUser.CursorShow(false);
        }

        protected override void OnMouseDown(MouseButton button, int x, int y)
        {
            base.OnMouseDown(button, x, y);
            if (button == MouseButton.Left) cursorShow = true;
            client.IsRunGameLoop = true;
        }

        protected override void OnMouseUp(MouseButton button, int x, int y)
        {
            base.OnMouseUp(button, x, y);
            if (button == MouseButton.Left) cursorShow = false;
            client.IsRunGameLoop = false;
        }

        protected override void OnMouseEnter()
        {
            base.OnMouseEnter();
            //cursorShow = true;
            //CursorShow(false);
        }

        protected override void OnMouseLeave()
        {
            cursorShow = false;
            //CursorShow(true);
        }

        protected override void OnOpenGLInitialized()
        {
            new OptionsFileMvk().Load();
            new OptionsFileMvk().Save();

            // Загрузка опций должна быть до инициализации графики, 
            // так -как нужно знать откуда грузить шейдера
            base.OnOpenGLInitialized();

            // Инициализация звука и загрузка семплов
            audio.Initialize(2);
            audio.InitializeSample();


            textureMap = new TextureMap(gl, 4);
            Bitmap bitmap = Image.FromFile(OptionsMvk.PathTextures + "cursor.png") as Bitmap;

            textureMap.AddTexture((int)AssetsTexture.cursor,
                new BufferedImage(bitmap.Width, bitmap.Height, BitmapToByteArray(bitmap)));

            bitmap = Image.FromFile(OptionsMvk.PathTextures + "Font8.png") as Bitmap;
            BufferedImage font8 = new BufferedImage(bitmap.Width, bitmap.Height, BitmapToByteArray(bitmap));
            bitmap = Image.FromFile(OptionsMvk.PathTextures + "Font12.png") as Bitmap;
            BufferedImage font12 = new BufferedImage(bitmap.Width, bitmap.Height, BitmapToByteArray(bitmap));
            bitmap = Image.FromFile(OptionsMvk.PathTextures + "Font16.png") as Bitmap;
            BufferedImage font16 = new BufferedImage(bitmap.Width, bitmap.Height, BitmapToByteArray(bitmap));

            textureMap.AddTexture((int)AssetsTexture.Font8, font8);
            textureMap.AddTexture((int)AssetsTexture.Font12, font12);
            textureMap.AddTexture((int)AssetsTexture.Font16, font16);

            FontAdvance.Initialize(font8, font12, font16);

            cursorVBO = new Mesh(gl, RenderFigure.Rectangle2d(0, 0, 24, 24, 0, 0, 1, 1), new int[] { 2, 2 });

            gl.ShadeModel(GL.GL_SMOOTH);
            gl.ClearColor(0.0f, .5f, 0.0f, 1f);
            gl.Clear(GL.GL_COLOR_BUFFER_BIT | GL.GL_DEPTH_BUFFER_BIT);
            gl.ClearDepth(1.0f);
            gl.Enable(GL.GL_DEPTH_TEST);
            gl.DepthFunc(GL.GL_LEQUAL);
            gl.Hint(GL.GL_PERSPECTIVE_CORRECTION_HINT, GL.GL_NICEST);


        }

        protected override void OnOpenGlDraw()
        {
            base.OnOpenGlDraw();
            
            gl.Clear(GL.GL_COLOR_BUFFER_BIT | GL.GL_DEPTH_BUFFER_BIT);
            gl.Enable(GL.GL_DEPTH_TEST);
            // группа для сглаживания, но может жутко тормазить
            gl.BlendFunc(GL.GL_SRC_ALPHA, GL.GL_ONE_MINUS_SRC_ALPHA);
            gl.ClearColor(.7f, .4f, .4f, 1f);
            gl.Enable(GL.GL_BLEND);

            DrawStatVBO();

            if (cursorShow)
            {

                textureMap.BindTexture((int)AssetsTexture.cursor);
                shader2D.Bind(gl);
                shader2D.SetUniformMatrix4(gl, "projview", Ortho2D);
                shader2D.SetUniform1(gl, "biasX", mouseX);
                shader2D.SetUniform1(gl, "biasY", mouseY);
                shader2D.SetUniform4(gl, "color", 1, 1, 1, 1);
                cursorVBO.Draw();

            }
        }

        /// <summary>
        /// Конвертация из Bitmap в объект BufferedImage
        /// </summary>
        private byte[] BitmapToByteArray(Bitmap bitmap)
        {
            BitmapData bmpdata = null;
            try
            {
                bmpdata = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                int numbytes = bmpdata.Stride * bitmap.Height;
                byte[] bytedata = new byte[numbytes];
                IntPtr ptr = bmpdata.Scan0;

                Marshal.Copy(ptr, bytedata, 0, numbytes);

                return bytedata;
            }
            finally
            {
                if (bmpdata != null)
                    bitmap.UnlockBits(bmpdata);
            }
        }

        protected override void OnResized(int width, int height)
        {
            base.OnResized(width, height);
            t1 = null;
        }

        /// <summary>
        /// Отладочный теккст
        /// VBO 450-460 fps, если biasY вывести на строку один то 540
        /// DL 650 fps
        /// </summary>
        public string textDebug =
            "Занесло меня на остров,\r\n" +
            "Ожидало много бед.\r\n" +
            "Жить на нём совсем не просто,\r\n" +
            "А прошло не мало лет.\r\n\r\n" +
            "Почти вымерли все звери,\r\n" +
            "Я остался лишь живой.\r\n" +
            "И ходил я всё и думал,\r\n" +
            "Как попасть же мне домой.\r\n\r\n" +
            "Занесло меня на остров,\r\n" +
            "Ожидало много бед.\r\n" +
            "Жить на нём совсем не просто,\r\n" +
            "А прошло не мало лет.\r\n\r\n" +
            "Почти вымерли все звери,\r\n" +
            "Я остался лишь живой.\r\n" +
            "И ходил я всё и думал,\r\n" +
            "Как попасть же мне домой.\r\n\r\n";

        private int xx = 0;

        Mesh mesh1;
        private float[] t1;

        /// <summary>
        /// Статистика на экране
        /// </summary>
        private void DrawStatVBO()
        {
            // Tessellation.Texture2DEnable();
            textureMap.BindTexture((int)AssetsTexture.Font12);
            shaderText.Bind(gl);
            shaderText.SetUniformMatrix4(gl, "projview", Ortho2D);

            Vector4 bg = new Vector4(.2f, .2f, .2f, 1f);
            Vector4 cw = new Vector4(.9f, .9f, .9f, 1f);

            FontRenderer.RenderString(gl, xx + 1, 401, bg, "-O-", FontSize.Font12);
            FontRenderer.RenderString(gl, xx, 400, cw, "-O-", FontSize.Font12);
            
            if (++xx > 900) xx = 0;

            // textDebug
            if (t1 == null)
            {
                
                t1 = FontRenderer.RenderText(gl, 11, 11, bg, textDebug, FontSize.Font12);
                
                //mesh1.Render(t1);
                List<float> list = new List<float>(t1);
                t1 = FontRenderer.RenderText(gl, 10, 10, cw, textDebug, FontSize.Font12);
                list.AddRange(t1);
                t1 = FontRenderer.RenderText(gl, 211, 11, bg, textDebug, FontSize.Font12);
                list.AddRange(t1);
                t1 = FontRenderer.RenderText(gl, 210, 10, cw, textDebug, FontSize.Font12);
                list.AddRange(t1);
                t1 = FontRenderer.RenderText(gl, 411, 11, bg, textDebug, FontSize.Font12);
                list.AddRange(t1);
                t1 = FontRenderer.RenderText(gl, 410, 10, cw, textDebug, FontSize.Font12);
                list.AddRange(t1);
                t1 = FontRenderer.RenderText(gl, 611, 11, bg, textDebug, FontSize.Font12);
                list.AddRange(t1);
                t1 = FontRenderer.RenderText(gl, 610, 10, cw, textDebug, FontSize.Font12);
                list.AddRange(t1);

                mesh1 = new Mesh(gl, list.ToArray(), new int[] { 2, 2, 3 });
                //mesh1.Render(list.ToArray());
                //mesh1.RenderEbo(list.ToArray());

            }
            //if (t2 == null)
            //{
            //    t2 = FontRendererVBO2.RenderText(gl, 10, 10, cw, textDebug, FontSize.Font12);
            //    mesh2 = new RenderMeshColor(gl);
                
            //    mesh2.Render(t2);
            //}
            //shaderText.SetUniform4(gl, "color", bg.x, bg.y, bg.z, bg.w);
            mesh1.Draw();
            //shaderText.SetUniform4(gl, "color", cw.x, cw.y, cw.z, cw.w);
            //mesh2.Draw();

            // stringInfo
            //FontRendererVBO.RenderString(gl, 11, Height - 38f, bg, stringInfo, FontSize.Font12);
            //FontRendererVBO.RenderString(gl, 11, Height - 39f, cw, stringInfo, FontSize.Font12);

            

            // Version
            textureMap.BindTexture((int)AssetsTexture.Font16);
            int w = FontRenderer.WidthString(version, FontSize.Font16);
            FontRenderer.RenderString(gl, Width - w - 9f, Height - 18f, bg, version, FontSize.Font16);
            FontRenderer.RenderString(gl, Width - w - 10f, Height - 19f, new Vector4(0.6f, 0.9f, .9f, 1f), version, FontSize.Font16);

            textureMap.BindTexture((int)AssetsTexture.Font12);
            // fps
            string str = "FPS " + fps.ToString() + " TPS " + tps.ToString();
            FontRenderer.RenderString(gl, 11f, Height - 18f, bg, str, FontSize.Font12);
            FontRenderer.RenderString(gl, 10f, Height - 19f, cw, str, FontSize.Font12);

            // XYZ
            w = 190;
            str = Width + " " + Height;
            if (VSync) str += " VSync";
            FontRenderer.RenderString(gl, w + 1, Height - 18f, bg, str, FontSize.Font12);
            w += FontRenderer.RenderString(gl, w, Height - 19f, cw, str, FontSize.Font12) + 10;

            // XY
            w = 400;
            str = "XY";
            FontRenderer.RenderString(gl, w + 1, Height - 18f, bg, str, FontSize.Font12);
            w += FontRenderer.RenderString(gl, w, Height - 19f, cw, str, FontSize.Font12) + 10;
            str = mouseX.ToString("0.0");
            FontRenderer.RenderString(gl, w + 1, Height - 18f, bg, str, FontSize.Font12);
            w += FontRenderer.RenderString(gl, w, Height - 19f, cw, str, FontSize.Font12) + 10;
            str = mouseY.ToString("0.0");
            FontRenderer.RenderString(gl, w + 1, Height - 18f, bg, str, FontSize.Font12);
            FontRenderer.RenderString(gl, w, Height - 19f, cw, str, FontSize.Font12);

            //textDb
            if (textDb != "")
            {
                FontRenderer.RenderString(gl, 11f, Height - 38f, bg, textDb, FontSize.Font12);
                FontRenderer.RenderString(gl, 10f, Height - 39f, cw, textDb, FontSize.Font12);
            }

            //  shaderText.Unbind(gl);
        }

        protected override void Client_Frame(object sender, EventArgs e)
        {
            DrawFrame();
            //Thread.Sleep(25);
        }

        protected override void Client_Tick(object sender, EventArgs e)
        {
            base.Client_Tick(sender, e);
            xx -= 100;
            if (xx < 0) xx = 0;
        }

        private string textDb = "";

        protected override void OnKeyDown(Keys keys)
        {
            base.OnKeyDown(keys);
            if (keys == Keys.Space)
            {
                audio.PlaySound(0, 0, 0, 0, 1, 1);
            }
            else if (keys == Keys.Enter)
            {
                audio.PlaySound(1, 0, 0, 0, 1, 1);
            }
            //map.ContainsKey(keys);
            textDb = "d* " + keys.ToString();// + " " + Convert.ToString(lParam.ToInt32(), 2);
        }

        protected override void OnKeyUp(Keys keys)
        {
            base.OnKeyUp(keys);
            textDb = "up " + keys.ToString();
        }

        protected override void OnKeyPress(char key)
        {
            try
            {
                //textDb += key;
                return;
            }
            catch (Exception ex)
            {
                return;
            }
        }

        //protected override void Server_Tick(object sender, EventArgs e)
        //{
        //    xx -= 100;
        //    if (xx < 0) xx = 0;
        //}
    }
}
