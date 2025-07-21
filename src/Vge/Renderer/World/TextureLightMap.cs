using Vge.Util;
using Vge.World;
using WinGL.OpenGL;

namespace Vge.Renderer.World
{
    /// <summary>
    /// Объект работы текстуры для освещения
    /// </summary>
    public class TextureLightMap
    {
        /// <summary>
        /// Яркость фазы лун
        /// </summary>
        public readonly static float[] LightMoonPhase 
            = new float[] { 0, .16f, .32f, .48f, .8f, .48f, .32f, .16f };
        /// <summary>
        /// Яркость для текстур
        /// </summary>
        public readonly static float[] LightBrightnessTable = new float[16];

        /// <summary>
        /// Идентификатор текстуры GL
        /// </summary>
        private uint _locationLightMap = 0;
        
        /// <summary>
        /// Буфер карты 16 (x) * 16 (y) * 4 (RGBA)
        /// </summary>
        private readonly byte[] _buffer = new byte[1024];

        /// <summary>
        /// Параметр яркости неба прошлого обнавления
        /// </summary>
        private float _skyLightPrev = -1f;
        /// <summary>
        /// Неба нет (true), прошлого обновления
        /// </summary>
        private bool _hasNoSkyPrev;

        private readonly GL gl;

        public TextureLightMap(GL gl)
        {
            this.gl = gl;
            float fm = 0f;
            for (int i = 0; i < 16; i++)
            {
                float f = 1f - i / 15f;
                LightBrightnessTable[i] = (1f - f) / (f * 3f + 1f) * (1f - fm) + fm;
            }
        }

        /// <summary>
        /// В каждом такте корректировка
        /// </summary>
        /// <param name="worldSettings">Настройки мира</param>
        public void Update(WorldSettings worldSettings)
        {
            if (_hasNoSkyPrev != worldSettings.HasNoSky)
            {
                // Скорее всего обновился мир, ибо сменилось небо
                _hasNoSkyPrev = worldSettings.HasNoSky;
                if (_hasNoSkyPrev)
                {
                    _GenTextureNotSky();
                    _UpdateLightmap();
                }
                else
                {
                    _skyLightPrev = worldSettings.Calendar.GetSunLight();
                    _GenTextureSky(_skyLightPrev, worldSettings.Calendar.GetMoonLight());
                    _UpdateLightmap();
                }
            }
            else if (!_hasNoSkyPrev)
            {
                // Есть небо, проверяем смену яркости солнца
                float sunLight = worldSettings.Calendar.GetSunLight();
                if (_skyLightPrev != sunLight)
                {
                    _skyLightPrev = sunLight;
                    _GenTextureSky(_skyLightPrev, worldSettings.Calendar.GetMoonLight());
                    _UpdateLightmap();
                }
            }
        }

        /// <summary>
        /// Генерируем текстуру с небом
        /// </summary>
        /// <param name="sunLight">Яркость солнца, 0.0 - 1.0</param>
        /// <param name="moonLight">Значение яркости луны, можно использовать фазу луны 0.0 - 0.5</param>
        private void _GenTextureSky(float sunLight, float moonLight)
        {
            for (int i = 0; i < 256; ++i)
            {
                float ls = sunLight < moonLight && moonLight > 0
                    ? LightBrightnessTable[i / 16] * moonLight + .05f
                    : LightBrightnessTable[i / 16] * sunLight * .95f + .05f;

                //float lb = MvkStatic.LightBrightnessTable[i % 16] * 1.5f;
                float lb = (i % 16) / 14f;

                //if (молния)
                //{
                //    ls = MvkStatic.LightBrightnessTable[i / 16];
                //}

                float ls2 = ls * (sunLight * .65f + .35f);
                float ls3 = ls * (sunLight * .65f + .35f);
                float lb2 = lb * ((lb * .6f + .4f) * .6f + .4f);
                float lb3 = lb * (lb * lb * .6f + .4f);
                float cr = ls2 + lb;
                float cg = ls3 + lb2;
                float cb = ls + lb3;
                cr = cr * .96f + .03f;
                cg = cg * .96f + .03f;
                cb = cb * .96f + .03f;
                float light;

                //if (this.bossColorModifier > 0.0F)
                //{ // босс
                //light = 1f;// this.bossColorModifierPrev + (this.bossColorModifier - this.bossColorModifierPrev) * p_78472_1_;
                //cr = cr * (1.0F - light) + cr * 0.7F * light;
                //cg = cg * (1.0F - light) + cg * 0.6F * light;
                //cb = cb * (1.0F - light) + cb * 0.6F * light;
                //}

                //if (var2.provider.getDimensionId() == 1)
                //{ // другой мир, всегда темно (ад)
                //    cr = 0.22F + lb * 0.75F;
                //    cg = 0.28F + lb2 * 0.75F;
                //    cb = 0.25F + lb3 * 0.75F;
                //}

                //cr = 0.32F + lb * 0.75F;
                //cg = 0.28F + lb2 * 0.75F;
                //cb = 0.25F + lb3 * 0.75F;

                float light2;

                //if (this.mc.thePlayer.isPotionActive(Potion.nightVision))
                //{ // зелье ночного виденья
                //light = 1f;// this.func_180438_a(this.mc.thePlayer, p_78472_1_);
                //light2 = 1.0F / cr;

                //if (light2 > 1f / cg) light2 = 1f / cg;
                //if (light2 > 1f / cb) light2 = 1f / cb;

                //cr = cr * (1f - light) + cr * light2 * light;
                //cg = cg * (1f - light) + cg * light2 * light;
                //cb = cb * (1f - light) + cb * light2 * light;
                //}

                if (cr > 1f) cr = 1f;
                if (cg > 1f) cg = 1f;
                if (cb > 1f) cb = 1f;

                light = .5f;// this.mc.gameSettings.gammaSetting; // яркость экрана  .1 - .5
                float cr2 = 1f - cr;
                float cg2 = 1f - cg;
                float cb2 = 1f - cb;
                cr2 = 1f - cr2 * cr2 * cr2 * cr2;
                cg2 = 1f - cg2 * cg2 * cg2 * cg2;
                cb2 = 1f - cb2 * cb2 * cb2 * cb2;
                cr = cr * (1f - light) + cr2 * light;
                cg = cg * (1f - light) + cg2 * light;
                cb = cb * (1f - light) + cb2 * light;
                cr = cr * .96f + .03f;
                cg = cg * .96f + .03f;
                cb = cb * .96f + .03f;

                if (cr > 1f) cr = 1f;
                if (cg > 1f) cg = 1f;
                if (cb > 1f) cb = 1f;

                if (cr < 0f) cr = 0f;
                if (cg < 0f) cg = 0f;
                if (cb < 0f) cb = 0f;

                _buffer[i * 4] = (byte)(cb * 255);
                _buffer[i * 4 + 1] = (byte)(cg * 255);
                _buffer[i * 4 + 2] = (byte)(cr * 255);
                _buffer[i * 4 + 3] = 255;
            }
        }

        /// <summary>
        /// Генерируем текстуру без неба
        /// </summary>
        private void _GenTextureNotSky()
        {
            for (int i = 0; i < 256; ++i)
            {
                float lb = (i % 16) / 14f;
                float lb2 = lb * ((lb * .6f + .4f) * .6f + .4f);
                float lb3 = lb * (lb * lb * .6f + .4f);

                float light;

                //if (this.bossColorModifier > 0.0F)
                //{ // босс
                //light = 1f;// this.bossColorModifierPrev + (this.bossColorModifier - this.bossColorModifierPrev) * p_78472_1_;
                //cr = cr * (1.0F - light) + cr * 0.7F * light;
                //cg = cg * (1.0F - light) + cg * 0.6F * light;
                //cb = cb * (1.0F - light) + cb * 0.6F * light;
                //}

                //if (var2.provider.getDimensionId() == 1)
                //{ // другой мир, всегда темно (ад)
                float cr = 0.22F + lb * 0.75F;
                float cg = 0.28F + lb2 * 0.75F;
                float cb = 0.25F + lb3 * 0.75F;
                //}

                // На край смахивает
                //cr = 0.32F + lb * 0.75F;
                //cg = 0.28F + lb2 * 0.75F;
                //cb = 0.25F + lb3 * 0.75F;

                float light2;

                //if (this.mc.thePlayer.isPotionActive(Potion.nightVision))
                { // зелье ночного виденья
                    //light = .3f;// this.func_180438_a(this.mc.thePlayer, p_78472_1_);
                    //light2 = 1.0F / cr;

                    //if (light2 > 1f / cg) light2 = 1f / cg;
                    //if (light2 > 1f / cb) light2 = 1f / cb;

                    //cr = cr * (1f - light) + cr * light2 * light;
                    //cg = cg * (1f - light) + cg * light2 * light;
                    //cb = cb * (1f - light) + cb * light2 * light;
                }

                if (cr > 1f) cr = 1f;
                if (cg > 1f) cg = 1f;
                if (cb > 1f) cb = 1f;

                light = .1f;// this.mc.gameSettings.gammaSetting; // яркость экрана  .1 - .5
                float cr2 = 1f - cr;
                float cg2 = 1f - cg;
                float cb2 = 1f - cb;
                cr2 = 1f - cr2 * cr2 * cr2 * cr2;
                cg2 = 1f - cg2 * cg2 * cg2 * cg2;
                cb2 = 1f - cb2 * cb2 * cb2 * cb2;
                cr = cr * (1f - light) + cr2 * light;
                cg = cg * (1f - light) + cg2 * light;
                cb = cb * (1f - light) + cb2 * light;
                cr = cr * .96f + .03f;
                cg = cg * .96f + .03f;
                cb = cb * .96f + .03f;

                if (cr > 1f) cr = 1f;
                if (cg > 1f) cg = 1f;
                if (cb > 1f) cb = 1f;

                if (cr < 0f) cr = 0f;
                if (cg < 0f) cg = 0f;
                if (cb < 0f) cb = 0f;

                _buffer[i * 4] = (byte)(cb * 255);
                _buffer[i * 4 + 1] = (byte)(cg * 255);
                _buffer[i * 4 + 2] = (byte)(cr * 255);
                _buffer[i * 4 + 3] = 255;
            }
        }

        /// <summary>
        /// Обновить текстуру
        /// </summary>
        private void _UpdateLightmap()
        {
            bool isCreate = _locationLightMap == 0;
            if (isCreate)
            {
                uint[] texture = new uint[1];
                gl.GenTextures(1, texture);
                _locationLightMap = texture[0];
            }

            gl.ActiveTexture(GL.GL_TEXTURE0 + (uint)Gi.ActiveTextureLightMap);
            gl.BindTexture(GL.GL_TEXTURE_2D, _locationLightMap);

            if (isCreate)
            {
                gl.TexImage2D(GL.GL_TEXTURE_2D, 0, GL.GL_RGBA, 16, 16,
                0, GL.GL_BGRA, GL.GL_UNSIGNED_BYTE, _buffer);

                gl.TexParameter(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MIN_FILTER, GL.GL_LINEAR);
                gl.TexParameter(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MAG_FILTER, GL.GL_LINEAR);
                gl.TexParameter(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_WRAP_S, GL.GL_CLAMP);
                //er = gl.GetError(); // InvalidEnum на Intel HD Graphics 5500
                gl.TexParameter(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_WRAP_T, GL.GL_CLAMP);
                //er = gl.GetError(); // InvalidEnum на Intel HD Graphics 5500
            }
            else
            {
                gl.TexSubImage2D(GL.GL_TEXTURE_2D, 0, 0, 0, 16, 16,
                        GL.GL_BGRA, GL.GL_UNSIGNED_BYTE, _buffer);
            }

#if DEBUG
            _UpdateLightmapDebug();
#endif
        }

        #region Debug

#if DEBUG

        private uint _locationLightMapDebug = 0;

        /// <summary>
        /// Обновить текстуру для визуальной отладки
        /// </summary>
        private void _UpdateLightmapDebug()
        {
            bool isCreate = _locationLightMapDebug == 0;
            if (isCreate)
            {
                uint[] texture = new uint[1];
                gl.GenTextures(1, texture);
                _locationLightMapDebug = texture[0];
            }

            gl.ActiveTexture(GL.GL_TEXTURE0);
            gl.BindTexture(GL.GL_TEXTURE_2D, _locationLightMapDebug);

            if (isCreate)
            {
                gl.TexImage2D(GL.GL_TEXTURE_2D, 0, GL.GL_RGBA, 16, 16,
                0, GL.GL_BGRA, GL.GL_UNSIGNED_BYTE, _buffer);
                gl.TexParameter(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MIN_FILTER, GL.GL_NEAREST);
                gl.TexParameter(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MAG_FILTER, GL.GL_NEAREST);
                gl.TexParameter(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_WRAP_S, GL.GL_CLAMP);
                gl.TexParameter(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_WRAP_T, GL.GL_CLAMP);
            }
            else
            {
                gl.TexSubImage2D(GL.GL_TEXTURE_2D, 0, 0, 0, 16, 16,
                        GL.GL_BGRA, GL.GL_UNSIGNED_BYTE, _buffer);
            }
        }

        public void BindTexture2dGui()
        {
            gl.ActiveTexture(GL.GL_TEXTURE0);
            gl.BindTexture(GL.GL_TEXTURE_2D, _locationLightMapDebug);
        }

#endif

    #endregion

    }
}
