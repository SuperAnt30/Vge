using Mvk2.Util;
using Vge.Audio;

namespace Mvk2.Audio
{
    /// <summary>
    /// Класс звуков для Малювек 2
    /// </summary>
    public class AudioMvk : AudioBase
    {
        /// <summary>
        /// Загрузка сэмпла
        /// </summary>
        public override void InitializeSample()
        {
            AudioSample sample = new AudioSample();
            sample.LoadWave(OptionsMvk.PathSounds + "say1.wav");
            items[0] = sample;
            sample = new AudioSample();
            sample.LoadOgg(OptionsMvk.PathSounds + "Click.ogg");
            items[1] = sample;
        }
    }
}
