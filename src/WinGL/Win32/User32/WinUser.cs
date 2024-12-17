using System;
using System.Runtime.InteropServices;
using WinGL.Util;

namespace WinGL.Win32.User32
{
    /// <summary>
    /// Функция обратного вызова, определяемая в приложении, которая обрабатывает сообщения,
    /// отправляемые в окно. 
    /// </summary>
    /// <param name="hWnd">Дескриптор окна</param>
    /// <param name="msg">Сообщение</param>
    /// <param name="wParam">Дополнительные сведения о сообщении</param>
    /// <param name="lParam">Дополнительные сведения о сообщении</param>
    /// <returns>Возвращаемое значение является результатом обработки сообщения и зависит от отправленного сообщения</returns>
    internal delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    internal static class WinUser
    {
        public const string User32 = "user32.dll";

        /// <summary>
        /// Вызывает процедуру окна по умолчанию, чтобы обеспечить обработку по умолчанию 
        /// для всех оконных сообщений, которые не обрабатываются приложением.
        /// Эта функция обеспечивает обработку каждого сообщения.
        /// </summary>
        /// <param name="hWnd">Дескриптор окна</param>
        /// <param name="msg">Сообщение</param>
        /// <param name="wParam">Дополнительные сведения о сообщении</param>
        /// <param name="lParam">Дополнительные сведения о сообщении</param>
        /// <returns>Возвращаемое значение является результатом обработки сообщения и зависит от сообщения</returns>
        [DllImport(User32, SetLastError = true)]
        public static extern IntPtr DefWindowProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// Регистрирует класс окна для последующего использования в вызовах функции CreateWindow или CreateWindowEx
        /// </summary>
        //[DllImport(User32, SetLastError = true)]
        //[return: MarshalAs(UnmanagedType.Bool)]
        //public static extern bool RegisterClass(WndClassEx lpwcx);
        /// <summary>
        /// Регистрирует класс окна для последующего использования в вызовах функции CreateWindow или CreateWindowEx
        /// </summary>
        [DllImport(User32, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.U2)]
        public static extern short RegisterClassEx([In] ref WndClassEx lpwcx);

        /// <summary>
        /// Извлекает информацию об одном из графических режимов устройства отображения.
        /// Чтобы получить информацию для всех графических режимов устройства отображения,
        /// выполните серию вызовов этой функции.
        /// </summary>
        /// <param name="lpszDeviceName">Указатель на строку с нулевым завершением, указывающую устройство отображения, о графическом режиме которого функция будет получать информацию.</param>
        /// <param name="iModeNum">Тип информации, которую необходимо получить. Это значение может быть индексом графического режима или одним из следующих значений.</param>
        /// <param name="lpDevMode">Указатель на структуру</param>
        [DllImport(User32, SetLastError = true)]
        public static extern int EnumDisplaySettings(string lpszDeviceName, int iModeNum, ref DevMode lpDevMode);

        /// <summary>
        /// Изменяет настройки устройства отображения по умолчанию на указанный графический режим.
        /// https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-changedisplaysettingsa
        /// </summary>
        /// <param name="lpDevMode">Указатель на структуру</param>
        /// <param name="dwFlags">Указывает, как следует изменить графический режим. Этот параметр может иметь одно из следующих значений.</param>
        [DllImport(User32, SetLastError = true)]
        public static extern int ChangeDisplaySettings(ref DevMode lpDevMode, int dwFlags);

        /// <summary>
        /// Создает перекрывающееся, всплывающее или дочернее окно с расширенным стилем окна;
        /// в противном случае эта функция идентична функции CreateWindow. 
        /// </summary>
        [DllImport(User32, SetLastError = true)]
        public static extern IntPtr CreateWindowEx(
           WindowStylesEx dwExStyle,
           string lpClassName,
           string lpWindowName,
           WindowStyles dwStyle,
           int x,
           int y,
           int nWidth,
           int nHeight,
           IntPtr hWndParent,
           IntPtr hMenu,
           IntPtr hInstance,
           IntPtr lpParam);

        /// <summary>
        /// Функция GetDC извлекает дескриптор контекста устройства (DC) 
        /// для клиентской области указанного окна или всего экрана. 
        /// Возвращаемый дескриптор можно использовать в последующих 
        /// функциях GDI для рисования в контроллере домена. 
        /// Контекст устройства — это непрозрачная структура данных, 
        /// значения которой используются внутри GDI.
        /// </summary>
        [DllImport(User32, SetLastError = true)]
        public static extern IntPtr GetDC(IntPtr hWnd);

        /// <summary>
        /// Задает состояние отображения указанного окна
        /// https://learn.microsoft.com/ru-ru/windows/win32/api/winuser/nf-winuser-showwindow
        /// </summary>
        /// <returns>Если окно было ранее видимым, возвращаемое значение будет ненулевым true. 
        /// Если окно ранее было скрыто, возвращаемое значение равно false.</returns>
        [DllImport(User32, SetLastError = true)]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        /// <summary>
        /// Выводит поток, создавший указанное окно, на передний план и активирует окно.
        /// Ввод с клавиатуры направляется в окно, и для пользователя изменяются различные визуальные подсказки.
        /// Система назначает немного более высокий приоритет потоку, создавшему окно переднего плана, чем другим потокам.
        /// </summary>
        [DllImport(User32, SetLastError = true)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        /// <summary>
        /// Устанавливает фокус клавиатуры в указанное окно.
        /// Окно должно быть присоединено к очереди сообщений вызывающего потока.
        /// </summary>
        [DllImport(User32, SetLastError = true)]
        public static extern IntPtr SetFocus(IntPtr hWnd);

        /// <summary>
        /// Изменяет текст строки заголовка указанного окна
        /// </summary>
        [DllImport(User32, SetLastError = true)]
        public static extern bool SetWindowText(IntPtr hWnd, string lpString);

        /// <summary>
        /// Освобождает контекст устройства (DC), освобождая его для использования 
        /// другими приложениями. Эффект функции ReleaseDC зависит от типа контроллера
        /// домена. Он освобождает только общие контроллеры домена и контроллеры 
        /// домена окон. Он не влияет на класс или частные контроллеры домена.
        /// </summary>
        [DllImport(User32, SetLastError = true)]
        public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        /// <summary>
        /// Уничтожает указанное окно
        /// </summary>
        [DllImport(User32, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DestroyWindow(IntPtr hWnd);

        /// <summary>
        /// Отменяет регистрацию класса окна, освобождая память, 
        /// необходимую для этого класса.
        /// </summary>
        [DllImport(User32, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnregisterClass(string lpClassName, IntPtr hInstance);

        /// <summary>
        /// Отправляет входящие сообщения без очереди, проверяет очередь сообщений
        /// потока на наличие отправленного сообщения и извлекает сообщение 
        /// (если таковые существуют).
        /// https://learn.microsoft.com/ru-ru/windows/win32/api/winuser/nf-winuser-peekmessagea
        /// </summary>
        [DllImport(User32, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool PeekMessage(ref MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);

        /// <summary>
        /// Преобразует сообщения с виртуальным ключом в символьные сообщения.
        /// </summary>
        [DllImport(User32, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool TranslateMessage(ref MSG lpMsg);

        /// <summary>
        /// Отправляет сообщение оконной процедуре
        /// </summary>
        [DllImport(User32, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DispatchMessage(ref MSG lpMsg);

        /// <summary>
        /// Метод для масштаба
        /// </summary>
        [DllImport(User32, SetLastError = true)]
        public static extern bool SetProcessDPIAware();

        /// <summary>
        /// Must initialize cbSize
        /// </summary>
        [DllImport(User32, SetLastError = true)]
        static extern bool GetCursorInfo(ref CursorInfoStruct pci);

        [DllImport(User32, SetLastError = true)]
        static extern int ShowCursor(bool bShow);

        [DllImport(User32, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetCursorPos(out Vector2i point);

        [DllImport(User32, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetCursorPos(int x, int y);

        [DllImport(User32, SetLastError = true)]
        static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);

        [DllImport(User32, SetLastError = true)]
        static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);


        #region Cursor

        /// <summary>
        /// Получить позицию курсора мыши
        /// </summary>
        public static Vector2i GetCursorPos()
        {
            Vector2i pos = new Vector2i();
            GetCursorPos(out pos);
            return pos;
        }

        /// <summary>
        /// Задать позицию курсора
        /// </summary>
        public static void SetCursorPosition(int x, int y)
            => SetCursorPos(x, y);

        /// <summary>
        /// Виден ли курсор мыши
        /// </summary>
        public static bool IsCursorVisible()
        {
            CursorInfoStruct pci = new CursorInfoStruct
            {
                cbSize = Marshal.SizeOf(typeof(CursorInfoStruct))
            };
            GetCursorInfo(ref pci);
            return (pci.flags & SHOWING) != 0;
        }

        /// <summary>
        /// Указать виден ли курсор
        /// </summary>
        public static void CursorShow(bool bShow)
        {
            if (bShow)
            {
                if (!IsCursorVisible()) ShowCursor(bShow);
            }
            else
            {
                if (IsCursorVisible()) ShowCursor(bShow);
            }
        }

        /// <summary>
        /// Установить курсор стрелки
        /// </summary>
        public static IntPtr CursorArrow()
        {
            return LoadCursor(IntPtr.Zero, IDC_ARROW);
        }

        #endregion

        /// <summary>
        /// Вызвать системное окно
        /// https://learn.microsoft.com/ru-ru/windows/win32/api/winuser/nf-winuser-messagebox
        /// </summary>
        public static int MessageBox(string text, string caption, uint type)
        {
            return MessageBox(IntPtr.Zero, text, caption, type);
        }

        #region Clipboard

        [DllImport(User32, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool OpenClipboard(IntPtr hWndNewOwner);

        [DllImport(User32, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseClipboard();

        [DllImport(User32, SetLastError = true)]
        public static extern IntPtr SetClipboardData(uint uFormat, IntPtr data);

        [DllImport(User32)]
        public static extern bool EmptyClipboard();

        [DllImport(User32, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsClipboardFormatAvailable(uint format);

        [DllImport(User32, SetLastError = true)]
        public static extern IntPtr GetClipboardData(uint uFormat);

        #endregion

        #region Windows Messages

        public const int MB_OK = 0x0000;
        public const int MB_OKCANCEL = 0x0001;
        public const int MB_YESNO = 0x0004;

        public const int MB_ICONSTOP = 0x0010;
        public const int MB_ICONWARNING = 0x0030;
        public const int MB_ICONASTERISK = 0x0040;

        public const int WM_ACTIVATE = 0x0006;
        public const int WM_ACTIVATEAPP = 0x001C;
        public const int WM_AFXFIRST = 0x0360;
        public const int WM_AFXLAST = 0x037F;
        public const int WM_APP = 0x8000;
        public const int WM_ASKCBFORMATNAME = 0x030C;
        public const int WM_CANCELJOURNAL = 0x004B;
        public const int WM_CANCELMODE = 0x001F;
        public const int WM_CAPTURECHANGED = 0x0215;
        public const int WM_CHANGECBCHAIN = 0x030D;
        public const int WM_CHANGEUISTATE = 0x0127;
        public const int WM_CHAR = 0x0102;
        public const int WM_CHARTOITEM = 0x002F;
        public const int WM_CHILDACTIVATE = 0x0022;
        public const int WM_CLEAR = 0x0303;
        public const int WM_CLOSE = 0x0010;
        public const int WM_COMMAND = 0x0111;
        public const int WM_COMPACTING = 0x0041;
        public const int WM_COMPAREITEM = 0x0039;
        public const int WM_CONTEXTMENU = 0x007B;
        public const int WM_COPY = 0x0301;
        public const int WM_COPYDATA = 0x004A;
        public const int WM_CREATE = 0x0001;
        public const int WM_CTLCOLORBTN = 0x0135;
        public const int WM_CTLCOLORDLG = 0x0136;
        public const int WM_CTLCOLOREDIT = 0x0133;
        public const int WM_CTLCOLORLISTBOX = 0x0134;
        public const int WM_CTLCOLORMSGBOX = 0x0132;
        public const int WM_CTLCOLORSCROLLBAR = 0x0137;
        public const int WM_CTLCOLORSTATIC = 0x0138;
        public const int WM_CUT = 0x0300;
        public const int WM_DEADCHAR = 0x0103;
        public const int WM_DELETEITEM = 0x002D;
        public const int WM_DESTROY = 0x0002;
        public const int WM_DESTROYCLIPBOARD = 0x0307;
        public const int WM_DEVICECHANGE = 0x0219;
        public const int WM_DEVMODECHANGE = 0x001B;
        public const int WM_DISPLAYCHANGE = 0x007E;
        public const int WM_DRAWCLIPBOARD = 0x0308;
        public const int WM_DRAWITEM = 0x002B;
        public const int WM_DROPFILES = 0x0233;
        public const int WM_ENABLE = 0x000A;
        public const int WM_ENDSESSION = 0x0016;
        public const int WM_ENTERIDLE = 0x0121;
        public const int WM_ENTERMENULOOP = 0x0211;
        public const int WM_ENTERSIZEMOVE = 0x0231;
        public const int WM_ERASEBKGND = 0x0014;
        public const int WM_EXITMENULOOP = 0x0212;
        public const int WM_EXITSIZEMOVE = 0x0232;
        public const int WM_FONTCHANGE = 0x001D;
        public const int WM_GETDLGCODE = 0x0087;
        public const int WM_GETFONT = 0x0031;
        public const int WM_GETHOTKEY = 0x0033;
        public const int WM_GETICON = 0x007F;
        public const int WM_GETMINMAXINFO = 0x0024;
        public const int WM_GETOBJECT = 0x003D;
        public const int WM_GETTEXT = 0x000D;
        public const int WM_GETTEXTLENGTH = 0x000E;
        public const int WM_HANDHELDFIRST = 0x0358;
        public const int WM_HANDHELDLAST = 0x035F;
        public const int WM_HELP = 0x0053;
        public const int WM_HOTKEY = 0x0312;
        public const int WM_HSCROLL = 0x0114;
        public const int WM_HSCROLLCLIPBOARD = 0x030E;
        public const int WM_ICONERASEBKGND = 0x0027;
        public const int WM_IME_CHAR = 0x0286;
        public const int WM_IME_COMPOSITION = 0x010F;
        public const int WM_IME_COMPOSITIONFULL = 0x0284;
        public const int WM_IME_CONTROL = 0x0283;
        public const int WM_IME_ENDCOMPOSITION = 0x010E;
        public const int WM_IME_KEYDOWN = 0x0290;
        public const int WM_IME_KEYLAST = 0x010F;
        public const int WM_IME_KEYUP = 0x0291;
        public const int WM_IME_NOTIFY = 0x0282;
        public const int WM_IME_REQUEST = 0x0288;
        public const int WM_IME_SELECT = 0x0285;
        public const int WM_IME_SETCONTEXT = 0x0281;
        public const int WM_IME_STARTCOMPOSITION = 0x010D;
        public const int WM_INITDIALOG = 0x0110;
        public const int WM_INITMENU = 0x0116;
        public const int WM_INITMENUPOPUP = 0x0117;
        public const int WM_INPUTLANGCHANGE = 0x0051;
        public const int WM_INPUTLANGCHANGEREQUEST = 0x0050;
        public const int WM_KEYDOWN = 0x0100;
        public const int WM_KEYFIRST = 0x0100;
        public const int WM_KEYLAST = 0x0108;
        public const int WM_KEYUP = 0x0101;
        public const int WM_KILLFOCUS = 0x0008;
        public const int WM_LBUTTONDBLCLK = 0x0203;
        public const int WM_LBUTTONDOWN = 0x0201;
        public const int WM_LBUTTONUP = 0x0202;
        public const int WM_MBUTTONDBLCLK = 0x0209;
        public const int WM_MBUTTONDOWN = 0x0207;
        public const int WM_MBUTTONUP = 0x0208;
        public const int WM_MDIACTIVATE = 0x0222;
        public const int WM_MDICASCADE = 0x0227;
        public const int WM_MDICREATE = 0x0220;
        public const int WM_MDIDESTROY = 0x0221;
        public const int WM_MDIGETACTIVE = 0x0229;
        public const int WM_MDIICONARRANGE = 0x0228;
        public const int WM_MDIMAXIMIZE = 0x0225;
        public const int WM_MDINEXT = 0x0224;
        public const int WM_MDIREFRESHMENU = 0x0234;
        public const int WM_MDIRESTORE = 0x0223;
        public const int WM_MDISETMENU = 0x0230;
        public const int WM_MDITILE = 0x0226;
        public const int WM_MEASUREITEM = 0x002C;
        public const int WM_MENUCHAR = 0x0120;
        public const int WM_MENUCOMMAND = 0x0126;
        public const int WM_MENUDRAG = 0x0123;
        public const int WM_MENUGETOBJECT = 0x0124;
        public const int WM_MENURBUTTONUP = 0x0122;
        public const int WM_MENUSELECT = 0x011F;
        public const int WM_MOUSEACTIVATE = 0x0021;
        public const int WM_MOUSEHOVER = 0x02A1;
        public const int WM_MOUSELAST = 0x020D;
        public const int WM_MOUSELEAVE = 0x02A3;
        public const int WM_MOUSEMOVE = 0x0200;
        public const int WM_MOUSEWHEEL = 0x020A;
        public const int WM_MOUSEHWHEEL = 0x020E;
        public const int WM_MOVE = 0x0003;
        public const int WM_MOVING = 0x0216;
        public const int WM_NCACTIVATE = 0x0086;
        public const int WM_NCCALCSIZE = 0x0083;
        public const int WM_NCCREATE = 0x0081;
        public const int WM_NCDESTROY = 0x0082;
        public const int WM_NCHITTEST = 0x0084;
        public const int WM_NCLBUTTONDBLCLK = 0x00A3;
        public const int WM_NCLBUTTONDOWN = 0x00A1;
        public const int WM_NCLBUTTONUP = 0x00A2;
        public const int WM_NCMBUTTONDBLCLK = 0x00A9;
        public const int WM_NCMBUTTONDOWN = 0x00A7;
        public const int WM_NCMBUTTONUP = 0x00A8;
        public const int WM_NCMOUSELEAVE = 0x02A2;
        public const int WM_NCMOUSEMOVE = 0x00A0;
        public const int WM_NCPAINT = 0x0085;
        public const int WM_NCRBUTTONDBLCLK = 0x00A6;
        public const int WM_NCRBUTTONDOWN = 0x00A4;
        public const int WM_NCRBUTTONUP = 0x00A5;
        public const int WM_NEXTDLGCTL = 0x0028;
        public const int WM_NEXTMENU = 0x0213;
        public const int WM_NOTIFY = 0x004E;
        public const int WM_NOTIFYFORMAT = 0x0055;
        public const int WM_NULL = 0x0000;
        public const int WM_PAINT = 0x000F;
        public const int WM_PAINTCLIPBOARD = 0x0309;
        public const int WM_PAINTICON = 0x0026;
        public const int WM_PALETTECHANGED = 0x0311;
        public const int WM_PALETTEISCHANGING = 0x0310;
        public const int WM_PARENTNOTIFY = 0x0210;
        public const int WM_PASTE = 0x0302;
        public const int WM_PENWINFIRST = 0x0380;
        public const int WM_PENWINLAST = 0x038F;
        public const int WM_POWER = 0x0048;
        public const int WM_POWERBROADCAST = 0x0218;
        public const int WM_PRINT = 0x0317;
        public const int WM_PRINTCLIENT = 0x0318;
        public const int WM_QUERYDRAGICON = 0x0037;
        public const int WM_QUERYENDSESSION = 0x0011;
        public const int WM_QUERYNEWPALETTE = 0x030F;
        public const int WM_QUERYOPEN = 0x0013;
        public const int WM_QUEUESYNC = 0x0023;
        public const int WM_QUIT = 0x0012;
        public const int WM_RBUTTONDBLCLK = 0x0206;
        public const int WM_RBUTTONDOWN = 0x0204;
        public const int WM_RBUTTONUP = 0x0205;
        public const int WM_RENDERALLFORMATS = 0x0306;
        public const int WM_RENDERFORMAT = 0x0305;
        public const int WM_SETCURSOR = 0x0020;
        public const int WM_SETFOCUS = 0x0007;
        public const int WM_SETFONT = 0x0030;
        public const int WM_SETHOTKEY = 0x0032;
        public const int WM_SETICON = 0x0080;
        public const int WM_SETREDRAW = 0x000B;
        public const int WM_SETTEXT = 0x000C;
        public const int WM_SETTINGCHANGE = 0x001A;
        public const int WM_SHOWWINDOW = 0x0018;
        public const int WM_SIZE = 0x0005;
        public const int WM_SIZECLIPBOARD = 0x030B;
        public const int WM_SIZING = 0x0214;
        public const int WM_SPOOLERSTATUS = 0x002A;
        public const int WM_STYLECHANGED = 0x007D;
        public const int WM_STYLECHANGING = 0x007C;
        public const int WM_SYNCPAINT = 0x0088;
        public const int WM_SYSCHAR = 0x0106;
        public const int WM_SYSCOLORCHANGE = 0x0015;
        public const int WM_SYSCOMMAND = 0x0112;
        public const int WM_SYSDEADCHAR = 0x0107;
        public const int WM_SYSKEYDOWN = 0x0104;
        public const int WM_SYSKEYUP = 0x0105;
        public const int WM_TCARD = 0x0052;
        public const int WM_TIMECHANGE = 0x001E;
        public const int WM_TIMER = 0x0113;
        public const int WM_UNDO = 0x0304;
        public const int WM_UNINITMENUPOPUP = 0x0125;
        public const int WM_USER = 0x0400;
        public const int WM_USERCHANGED = 0x0054;
        public const int WM_VKEYTOITEM = 0x002E;
        public const int WM_VSCROLL = 0x0115;
        public const int WM_VSCROLLCLIPBOARD = 0x030A;
        public const int WM_WINDOWPOSCHANGED = 0x0047;
        public const int WM_WINDOWPOSCHANGING = 0x0046;
        public const int WM_WININICHANGE = 0x001A;
        public const int WM_XBUTTONDBLCLK = 0x020D;
        public const int WM_XBUTTONDOWN = 0x020B;
        public const int WM_XBUTTONUP = 0x020C;

        #endregion

        public const int ENUM_CURRENT_SETTINGS = -1;

        public const int CDS_UPDATEREGISTRY = 0x01;
        public const int CDS_TEST = 0x02;
        public const int CDS_FULLSCREEN = 0x04;
        public const int CDS_GLOBAL = 0x08;
        public const int CDS_SET_PRIMARY = 0x10;
        public const int CDS_RESET = 0x40000000;
        public const int CDS_NORESET = 0x10000000;

        public const int DISP_CHANGE_SUCCESSFUL = 0;
        public const int DISP_CHANGE_RESTART = 1;
        public const int DISP_CHANGE_FAILED = -1;

        public const int SW_SHOW = 5;

        private const int SHOWING = 0x01;

        public const int IDC_ARROW = 32512;

        /// <summary>
        /// Сообщения удаляются из очереди после обработки с помощью PeekMessage
        /// </summary>
        public const int PM_REMOVE = 0x0001;
        


    }
}
