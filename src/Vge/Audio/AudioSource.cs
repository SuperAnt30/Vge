namespace Vge.Audio
{
    /// <summary>
    /// Объект источника звука
    /// </summary>
    public class AudioSource
    {
        /// <summary>
        /// Проигрывает ли сейчас звук
        /// </summary>
        public bool Processing { get; private set; } = false;

        /// <summary>
        /// Id источника
        /// </summary>
        private uint sourceId = 0;
        /// <summary>
        /// Id буфера
        /// </summary>
        private uint bufferId = 0;

        public bool Initialized()
        {
            Al.alGenSources(1, out uint sid);
            int errorCode = Al.alGetError();
            // По ошибке определяем источник и буфер обмена
            if (errorCode == 0)
            {
                Al.alGenBuffers(1, out uint bid);
                errorCode = Al.alGetError();
                if (errorCode == 0)
                {
                    // Всё норм
                    bufferId = bid;
                    sourceId = sid;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Проиграть звук
        /// </summary>
        public void Play(float posX, float posY, float posZ, float volume, float pitch)
        {
            Processing = true;
            Al.alSourcef(sourceId, Al.AL_PITCH, pitch);
            Al.alSourcef(sourceId, Al.AL_GAIN, volume);
            Al.alSource3f(sourceId, Al.AL_POSITION, posX, posY, posZ);
            Al.alSource3f(sourceId, Al.AL_ORIENTATION, 0, 0, -1);
            Al.alSourcePlay(sourceId);
        }
        /// <summary>
        /// Проиграть звук, указав расположение и громкость
        /// </summary>
        public void Play(float posX, float posY, float posZ, float volume) => Play(posX, posY, posZ, volume, 1f);
        /// <summary>
        /// Проиграть звук, указав громкость и тональность
        /// </summary>
        public void Play(float volume, float pitch) => Play(0, 0, 0, volume, pitch);
        /// <summary>
        /// Проиграть звук, указав только громкость
        /// </summary>
        public void Play(float volume) => Play(0, 0, 0, volume, 1f);

        /// <summary>
        /// Проверить процесс звучания
        /// </summary>
        public bool CheckProcessing()
        {
            if (Processing)
            {
                Al.alGetSourcei(sourceId, Al.AL_SOURCE_STATE, out int value);
                if (value != Al.AL_PLAYING)
                {
                    Al.alDeleteSources(1, ref sourceId);
                    Al.alDeleteBuffers(1, ref bufferId);
                    Al.alGenSources(1, out uint sid);
                    int errorS = Al.alGetError();
                    Al.alGenBuffers(1, out uint bid);
                    int errorB = Al.alGetError();
                    // Всё норм
                    bufferId = bid;
                    sourceId = sid;
                    Processing = false;
                }
            }
            return Processing;
        }

        /// <summary>
        /// Задать сэмпл
        /// </summary>
        /// <param name="audio">Объект сэмпла</param>
        public void Sample(AudioSample audio)
        {
            Al.alBufferData(bufferId, audio.AlFormat, audio.Buffer, audio.Size, audio.SamplesPerSecond);
            Al.alSourcei(sourceId, Al.AL_BUFFER, (int)bufferId);
        }
    }
}