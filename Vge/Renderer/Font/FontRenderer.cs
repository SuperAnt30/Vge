using System;
using System.Collections.Generic;
using System.Numerics;
using WinGL.OpenGL;
using WinGL.Util;

namespace Vge.Renderer.Font
{
    public class FontRenderer
    {
        private static Mesh mesh;

        public static void Init(GL gl)
        {
            mesh = new Mesh(gl, new float[0], new int[] { 2, 2, 3 });
        }

        //public static void Draw(float[] buffer)
        //{
        //    mesh.Render(buffer);
        //    mesh.Draw();
        //}

        /// <summary>
        /// Узнать ширину символа
        /// </summary>
        private static int WidthChar(char letter, int size)
        {
            Symbol symbol = FontAdvance.Get(letter, size);
            return symbol.Width;
        }

        /// <summary>
        /// Прорисовка текста
        /// </summary>
        public static float[] RenderText(GL gl, float x, float y, Vector4 color, string text, FontSize size)
        {
            float[] buffer = null;
            string[] stringSeparators = new string[] { "\r\n" };
            string[] strs = text.Split(stringSeparators, StringSplitOptions.None);
            int h = (int)y;

            // WindowMain2.shaderText.SetUniform4(gl, "color", color.x, color.y, color.z, color.w);
            //WindowMain2.shader2D.SetUniform1(gl, "biasY", 0);
            //WindowMain2.shader2D.SetUniform1(gl, "biasX", 0);
            //float rgb = color.x;// + color.y * 100f + color.z * 10000;

            List<float> list = new List<float>();
            Symbol symbol;
            int sizeInt = (int)size;
            char[] vc;
            int w;
            foreach (string str in strs)
            {
                vc = str.ToCharArray();
                w = (int)x;
                for (int i = 0; i < vc.Length; i++)
                {
                    symbol = FontAdvance.Get(vc[i], sizeInt);
                    list.AddRange(RenderFigure.Rectangle2d(
                    w, h, w + FontAdvance.HoriAdvance[sizeInt], h + FontAdvance.VertAdvance[sizeInt],
                    symbol.U1, symbol.V1, symbol.U2, symbol.V2, color.X, color.Y, color.Z));
                    if (symbol.Width > 0) w += symbol.Width + StepFont(size);
                }
                h += FontAdvance.VertAdvance[(int)size] + 4;
            }
            if (list.Count > 0)
            {
                buffer = list.ToArray();
                //mesh.Render(buffer);
                //mesh.Draw();
            }
            return buffer;
        }

        /// <summary>
        /// Прорисовка строки
        /// </summary>
        public static int RenderString(GL gl, float x, float y, Vector4 color, string text, FontSize size)
            => RenderString(gl, x, y, color, text, size, true);

        private static int RenderString(GL gl, float x, float y, Vector4 color, string text, FontSize size, bool isColor)
        {
            char[] vc = text.ToCharArray();
            int h = (int)y;
            int w = (int)x;

            //if (isColor)
            //{
            // //   WindowMain2.shaderText.SetUniform4(gl, "color", color.x, color.y, color.z, color.w);
            ////    WindowMain2.shader2D.SetUniform1(gl, "biasY", 0);
            ////    WindowMain2.shader2D.SetUniform1(gl, "biasX", 0);
            //}
            float rgb = color.X + color.Y * 100f + color.Z * 10000;


            List<float> list = new List<float>();
            Symbol symbol;
            int sizeInt = (int)size;
            for (int i = 0; i < vc.Length; i++)
            {
                symbol = FontAdvance.Get(vc[i], sizeInt);
                
                //list.AddRange(RenderMeshColor.Rectangle2d(
                //    w + symbol.U1, h + symbol.V1,
                //    w + FontAdvanceVBO.HoriAdvance[sizeInt] + symbol.U2,
                //    h + FontAdvanceVBO.VertAdvance[sizeInt] + symbol.V2,
                //    rgb));
                //list.AddRange(RenderMeshColor.Rectangle2d(
                //    w + h * 10000,
                //    w + FontAdvanceVBO.HoriAdvance[sizeInt] + symbol.U2 +
                //    (h + FontAdvanceVBO.VertAdvance[sizeInt] + symbol.V2) * 10000,
                //    symbol.U1 + symbol.V1 * 16,
                //    symbol.U2 + symbol.V2 * 16,
                //    rgb));

                list.AddRange(RenderFigure.Rectangle2d(
                    w, h, w + FontAdvance.HoriAdvance[sizeInt], h + FontAdvance.VertAdvance[sizeInt],
                    symbol.U1, symbol.V1 , symbol.U2, symbol.V2, color.X, color.Y, color.Z));
                if (symbol.Width > 0) w += symbol.Width + StepFont(size);
            }
            if (list.Count > 0)
            {
                mesh.Reload(list.ToArray());
                //mesh.RenderEbo(list.ToArray());
                mesh.Draw();
            }
            return w - (int)x;
        }

        /// <summary>
        /// Узнать ширину текста
        /// </summary>
        public static int WidthString(string text, FontSize size)
        {
            char[] vc = text.ToCharArray();
            int w = 0;
            for (int i = 0; i < vc.Length; i++)
            {
                int w0 = WidthChar(vc[i], (int)size);
                if (w0 > 0) w += w0 + StepFont(size);
            }
            return w;
        }

        /// <summary>
        /// Растояние между буквами в пикселях
        /// </summary>
        public static int StepFont(FontSize size)
        {
            switch (size)
            {
                case FontSize.Font8: return 1;
                case FontSize.Font16: return 2;
                default: return 1;
            }
        }
        /// <summary>
        /// Получить текстуру по размеру шрифта
        /// </summary>
        public static AssetsTexture ConvertFontToTexture(FontSize size)
        {
            switch (size)
            {
                case FontSize.Font8: return AssetsTexture.Font8;
                case FontSize.Font16: return AssetsTexture.Font16;
                default: return AssetsTexture.Font12;
            }
        }
    }
}
