using System;
using System.Collections.Generic;
using Vge.Json;
using Vge.Util;

namespace Vge.Audio
{
    /// <summary>
    /// Статический класс индексов семплов
    /// </summary>
    public static class AudioReg
    {
        /// <summary>
        /// Индекс семпла клика для GUI
        /// </summary>
        public static int Click;

        /// <summary>
        /// Инициализация интексов семплов. Нужен так же для 
        /// </summary>
        public static void Initialization(bool isClient)
        {
            string fileName = "Sounds.json";
            JsonRead jsonRead = new JsonRead(Options.PathSounds + fileName);
            if (jsonRead.IsThereFile)
            {
                try
                {
                    foreach (JsonKeyValue json in jsonRead.Compound.Items)
                    {
                        if (json.IsKey("Samples"))
                        {
                            List<SampleReg> samples = new List<SampleReg>();
                            string[] jsonSamples = json.GetArray().ToArrayString();
                            SampleReg sampleReg;
                            for (int i = 0; i < jsonSamples.Length; i++)
                            {
                                sampleReg = new SampleReg(jsonSamples[i]);
                                samples.Add(sampleReg);
                                if (sampleReg.Key == "Click")
                                {
                                    Click = i;
                                }
                            }
                            AudioIndexs.Initialization(isClient, samples);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(Sr.GetString(Sr.ErrorReadJsonSoundStat, fileName)
                        + " " + ex.Message, ex);
                }
            }
            else
            {
                throw new Exception(Sr.GetString(Sr.FileMissingSounds, fileName));
            }
        }
    }
}
