using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Vge.Audio;

namespace Mvk2.Audio
{
    /// <summary>
    /// Статический класс индексов семплов
    /// </summary>
    public static class AudioReg
    {
        /// <summary>
        /// Индекс семпла клика
        /// </summary>
        public static int Click;

        /// <summary>
        /// Инициализация интексов семплов. Нужен так же для 
        /// </summary>
        public static void Initialization(bool isClient)
        {
            // TODO::2026-03-10 Сделать Sound.json
            List<SampleReg> samples = new List<SampleReg>();

            string dig = "Dig" + Path.DirectorySeparatorChar;
            string step = "Step" + Path.DirectorySeparatorChar;

            // Регистрация всех семплов, так же нужны для сервера. Но без пути
            
            // Для GUI клик
            samples.Add(new SampleReg("Click")); Click = 0;

            samples.Add(new SampleReg("DigGlass1", dig));
            samples.Add(new SampleReg("DigGlass2", dig));
            samples.Add(new SampleReg("DigGlass3", dig));
            samples.Add(new SampleReg("DigGrass1", dig));
            samples.Add(new SampleReg("DigGrass2", dig));
            samples.Add(new SampleReg("DigGrass3", dig));
            samples.Add(new SampleReg("DigGrass4", dig));
            samples.Add(new SampleReg("DigSand1", dig));
            samples.Add(new SampleReg("DigSand2", dig));
            samples.Add(new SampleReg("DigSand3", dig));
            samples.Add(new SampleReg("DigSand4", dig));
            samples.Add(new SampleReg("DigStone1", dig));
            samples.Add(new SampleReg("DigStone2", dig));
            samples.Add(new SampleReg("DigStone3", dig));
            samples.Add(new SampleReg("DigStone4", dig));
            samples.Add(new SampleReg("DigWood1", dig));
            samples.Add(new SampleReg("DigWood2", dig));
            samples.Add(new SampleReg("DigWood3", dig));
            samples.Add(new SampleReg("DigWood4", dig));
            
            samples.Add(new SampleReg("StepGrass1", step));
            samples.Add(new SampleReg("StepGrass2", step));
            samples.Add(new SampleReg("StepGrass3", step));
            samples.Add(new SampleReg("StepGrass4", step));
            samples.Add(new SampleReg("StepSand1", step));
            samples.Add(new SampleReg("StepSand2", step));
            samples.Add(new SampleReg("StepSand3", step));
            samples.Add(new SampleReg("StepSand4", step));
            samples.Add(new SampleReg("StepStone1", step));
            samples.Add(new SampleReg("StepStone2", step));
            samples.Add(new SampleReg("StepStone3", step));
            samples.Add(new SampleReg("StepStone4", step));
            samples.Add(new SampleReg("StepWood1", step));
            samples.Add(new SampleReg("StepWood2", step));
            samples.Add(new SampleReg("StepWood3", step));
            samples.Add(new SampleReg("StepWood4", step));


            AudioIndexs.Initialization(isClient, samples);
        }
    }
}
