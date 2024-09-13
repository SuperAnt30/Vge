
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Security;

namespace WinGL.Win32
{
    /// <summary>
    /// Полезные функции, импортированные из Win32 SDK.
    /// </summary>
    internal static class WinApi
    {
        /// <summary>
        /// Инициализирует класс <see cref="Win32"/>
        /// </summary>
        //static WinApi()
        //{
        //    // Загрузите библиотеку openGL — без этого вызовы wgl завершится неудачно
        //   // IntPtr glLibrary = LoadLibrary(OpenGL32);
        //}

        // Имена библиотек, которые мы импортируем
        public const string Kernel32 = "kernel32.dll";
        public const string Winmm = "winmm.dll";

        #region Kernel32 Functions

        //[DllImport(Kernel32, SetLastError = true)]
        //public static extern IntPtr LoadLibrary(string lpFileName);

        /// <summary>
        /// Извлекает дескриптор модуля для указанного модуля.
        /// Модуль должен быть загружен вызывающим процессом.
        /// </summary>
        [DllImport(Kernel32, SetLastError = true)]
        public static extern IntPtr GetModuleHandle(IntPtr hWnd);

        [DllImport(Kernel32, SetLastError = true)]
        public static extern IntPtr GlobalLock(IntPtr hMem);

        [DllImport(Kernel32, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GlobalUnlock(IntPtr hMem);

        #endregion

        #region TimePeriod

        /// В документации написано, что timeBeginPeriod следует вызывать непосредственно перед использованием таймера,
        /// а timeEndPeriod - сразу же после него. Иными словами, рекомендуется чтобы таймер как можно меньше времени 
        /// работал при повышенном разрешении (это снижает общую производительность системы). 
        /// А значит вариант "задать период при старте программы, и потом вырубить при выходе" лучше не использовать.

        /// Функция timeGetDevCaps запрашивает устройство таймера для определения его разрешения.

        /// <summary>TimeBeginPeriod(). See the Windows API documentation for details.</summary>

        [SuppressMessage("Microsoft.Interoperability", "CA1401:PInvokesShouldNotBeVisible"), SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage"), SuppressUnmanagedCodeSecurity]
        [DllImport(Winmm, EntryPoint = "timeBeginPeriod", SetLastError = true)]

        /// <summary>
        /// Функция TimeEndPeriod запрашивает минимальное разрешение для периодических таймеров.
        /// </summary>
        public static extern uint TimeBeginPeriod(uint uMilliseconds);

        /// <summary>Функция TimeEndPeriod очищает ранее установленную минимальную разрешение таймера.</summary>

        [SuppressMessage("Microsoft.Interoperability", "CA1401:PInvokesShouldNotBeVisible"), SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage"), SuppressUnmanagedCodeSecurity]
        [DllImport(Winmm, EntryPoint = "timeEndPeriod", SetLastError = true)]

        /// <summary>
        /// Функция TimeEndPeriod очищает ранее установленную минимальную разрешение таймера.
        /// </summary>
        public static extern uint TimeEndPeriod(uint uMilliseconds);

        #endregion


    }
}
