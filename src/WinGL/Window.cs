using System;
using System.Diagnostics;
using WinGL.Actions;
using WinGL.OpenGL;
using WinGL.Util;
using WinGL.Win32;
using WinGL.Win32.Gdi32;
using WinGL.Win32.User32;

namespace WinGL
{
    /// <summary>
    /// Объект окна
    /// https://pmg.org.ru/nehe/nehe01.htm
    /// </summary>
    public class Window
    {
        /// <summary>
        /// Ширина окна, по умолчанию размер HD
        /// </summary>
        public int Width { get; private set; } = 1280;
        /// <summary>
        /// Высота окна, по умолчанию размер HD
        /// </summary>
        public int Height { get; private set; } = 720;
        /// <summary>
        /// Позиция Х расположения окна
        /// </summary>
        public int LocationX { get; private set; } = 0;
        /// <summary>
        /// Позиция Y расположения окна
        /// </summary>
        public int LocationY { get; private set; } = 0;
        /// <summary>
        /// Заголовок окна
        /// </summary>
        public string Title { get; protected set; } = "FormOpenGL";
        /// <summary>
        /// Полноэкранные окно или нет
        /// </summary>
        public bool FullScreen { get; protected set; } = false;
        /// <summary>
        /// Название класса
        /// </summary>
        public string NameClass { get; private set; } = "FormOpenGL";
        /// <summary>
        /// Массив матрицы для проецирования двумерных координат на экран
        /// </summary>
        public readonly float[] Ortho2D = new float[16];
        /// <summary>
        /// Флаг, являет ли раскладка клавиатуры кириллицы
        /// </summary>
        public readonly bool KeyCyrillic;

        /// <summary>
        /// Объект OpenGL для элемента управления
        /// </summary>
        protected readonly GL gl = new GL();
        
        /// <summary>
        /// Версия 2.1, 3.3 или 4.6
        /// </summary>
        protected OpenGLVersion openGLVersion = OpenGLVersion.OpenGL4_6;
        
        /// <summary>
        /// Распаложена ли мышь на окне
        /// </summary>
        private bool mouseEntity = false;
        /// <summary>
        /// Ширина окна в оконном режимме
        /// </summary>
        private int windowWidth;
        /// <summary>
        /// Высота окна в оконном режимме
        /// </summary>
        private int windowHeigt;
        /// <summary>
        /// Позиция Х расположения окна в оконном режимме
        /// </summary>
        private int windowLocationX;
        /// <summary>
        /// Позиция Y расположения окна в оконном режимме
        /// </summary>
        private int windowLocationY;

        /// <summary>
        /// Здесь будет хранится дескриптор приложения
        /// </summary>
        private IntPtr hInstance;
        /// <summary>
        /// Здесь будет хранится дескриптор окна
        /// </summary>
        private IntPtr hWnd;
        /// <summary>
        /// Приватный контекст устройства GDI
        /// </summary>
        private IntPtr hDC;
        /// <summary>
        /// Хранится последний монитор, для кастыльного выхода
        /// </summary>
        private IntPtr hMonitorPrev;
        /// <summary>
        /// Объект делегата приёма ответов, делается глобальный, чтоб не удалялся из-за чистки мусора
        /// </summary>
        private WndProc lpfnWndProcMain;
        /// <summary>
        /// Работает ли глобальный loop
        /// </summary>
        private bool running = false;
                
        /// <summary>
        /// Использовать ли вертикальную сенхронизацию
        /// </summary>
        protected bool vSync = true;

        protected Window()
        {
            int keyboardLayoutId = System.Globalization.CultureInfo.CurrentCulture.KeyboardLayoutId;
            KeyCyrillic = keyboardLayoutId == 1049 // ru-RU
                || keyboardLayoutId == 8192; // ru-BY
        }

        /// <summary>
        /// Системное окно ошибки
        /// </summary>
        protected static void MessageBoxCrash(Exception ex)
        {
            Exception e;
            string stackTrace;
            if (ex.InnerException == null)
            {
                e = ex;
                stackTrace = e.StackTrace;
            }
            else
            {
                if (ex.InnerException.InnerException == null)
                {
                    e = ex.InnerException;
                    stackTrace = e.StackTrace + "\r\n" + ex.StackTrace;
                }
                else
                {
                    e = ex.InnerException;
                    stackTrace = e.InnerException.StackTrace + "\r\n" + e.StackTrace + "\r\n" + ex.StackTrace;
                }
            }
            if (stackTrace.Length > 800) stackTrace = stackTrace.Substring(0, 800) + "...";
            WinUser.MessageBox(string.Format("{0}: {1}\r\n---\r\n{2}", e.Source, e.Message, stackTrace),
                "Crash", WinUser.MB_ICONSTOP);
        }

        /// <summary>
        /// Запустить окно самого же себя
        /// </summary>
        public static void Run(Window window)
        {
            Glm.Initialized();
            try
            {
                window.Begin();
            }
            catch (Exception ex)
            {
                MessageBoxCrash(ex);
                // При краше, принудительно останавливаем все потоки которые работали.
                Process.GetCurrentProcess().Kill();
            }
        }

        /// <summary>
        /// Запущено окно
        /// </summary>
        public virtual void Begined() { }

        #region BeginLoopCleanUp

        /// <summary>
        /// Запустить окно
        /// </summary>
        protected virtual void Begin()
        {
            bool timePeriodRun = false;
            try
            {
                CreateGLWindow();
                WinApi.TimeBeginPeriod(1);
                timePeriodRun = true;
                Begined();
                Loop();
            }
            catch (Exception ex)
            {
                throw new Exception(Sr.GetString(Sr.ErrorWhileStartingWindow, ex.Message), ex);
            }
            finally
            {
                OnStoping();
                CleanUp();
                if (timePeriodRun)
                {
                    WinApi.TimeEndPeriod(1);
                }
            }
        }

        /// <summary>
        /// Создание окна
        /// </summary>
        private void CreateGLWindow()
        {
            // Считаем дескриптор нашего приложения
            hInstance = WinApi.GetModuleHandle(IntPtr.Zero);
            lpfnWndProcMain = WndProcPrivate;
            // Структура класса окна
            WndClassEx wc = new WndClassEx()
            {
                // Перерисуем при перемещении и создаём скрытый DC
                style = ClassStyles.CS_HREDRAW | ClassStyles.CS_VREDRAW | ClassStyles.CS_OWNDC | ClassStyles.CS_BYTEALIGNCLIENT | ClassStyles.CS_BYTEALIGNWINDOW,
                // Процедура обработки сообщений
                lpfnWndProc = lpfnWndProcMain,
                // Устанавливаем дескриптор
                hInstance = hInstance,
                // Устанавливаем имя классу
                lpszClassName = NameClass
            };
            wc.hCursor = WinUser.CursorArrow();
            wc.Init();

            // Пытаемся зарегистрировать класс окна
            if (WinUser.RegisterClassEx(ref wc) == 0)
            {
                throw new Exception(Sr.FailedToRegisterTheWindowClass);
            }

            // Определяем монитор где будет запущено окно
            Vector2i pos = WinUser.GetCursorPos();
            IntPtr hMonitor = WinUser.MonitorFromPoint(pos, WinUser.MONITOR_DEFAULTTONEAREST);
            // Получаем информацию монитора
            MonitorInfoEx monitorInfo = new MonitorInfoEx();
            if (!WinUser.GetMonitorInfo(hMonitor, monitorInfo))
            {
                throw new Exception(Sr.FailedToGetMonitorInformation);
            }

            //TODO::2025-06-13 Продумать центровку!
            LocationX = windowLocationX = monitorInfo.Monitor.Left;
            LocationY = windowLocationY = monitorInfo.Monitor.Top;

            // Расширенный стиль окна
            WindowStylesEx dwExStyle = WindowStylesEx.WS_EX_APPWINDOW | WindowStylesEx.WS_EX_WINDOWEDGE;
            // Обычный стиль окна
            WindowStyles dwStyle = WindowStyles.WS_OVERLAPPEDWINDOW;

            // Создаём окно
            hWnd = WinUser.CreateWindowEx(
                dwExStyle,
                NameClass, Title,
                WindowStyles.WS_CLIPSIBLINGS | WindowStyles.WS_CLIPCHILDREN | dwStyle,
                LocationX, LocationY,
                Width, Height,
                IntPtr.Zero, IntPtr.Zero, hInstance, IntPtr.Zero);

            if (hWnd == IntPtr.Zero) 
            {
                throw new Exception(Sr.WindowCreationError);
            }

            int width = Width;
            int height = Height;
            // Корректировка масштаба экрана
            int dpi = (int)WinUser.GetDpiForWindow(hWnd);
            if (dpi != 96)
            {
                // Если dpi не 100%
                width = Width * dpi / 96;
                height = Height * dpi / 96;
            }
            // Определили разрешение рабочей области
            int widthWorkArea = monitorInfo.WorkArea.Right - monitorInfo.WorkArea.Left;
            int heightWorkArea = monitorInfo.WorkArea.Bottom - monitorInfo.WorkArea.Top;
            // Центрируем окно
            if (width > widthWorkArea)
            {
                width = widthWorkArea;
                LocationX = windowLocationX = monitorInfo.Monitor.Left;
            }
            else
            {
                LocationX = windowLocationX = monitorInfo.Monitor.Left + (widthWorkArea - width) / 2;
            }
            if (height > heightWorkArea)
            {
                height = heightWorkArea;
                LocationY = windowLocationY = monitorInfo.Monitor.Top;
            }
            else
            {
                LocationY = windowLocationY = monitorInfo.Monitor.Top + (heightWorkArea - height) / 2;
            }
            WinUser.SetWindowPos(hWnd, IntPtr.Zero, LocationX, LocationY, width, height, 
                WinUser.SWP_NOZORDER);

            // pfd сообщает Windows каким будет вывод на экран каждого пикселя
            PixelFormatDescriptor pfd = new PixelFormatDescriptor
            {
                nVersion = 1,
                dwFlags = WinGdi.PFD_DRAW_TO_WINDOW  // Формат для Окна
                | WinGdi.PFD_SUPPORT_OPENGL // Формат для OpenGL
                | WinGdi.PFD_DOUBLEBUFFER, // Формат для двойного буфера
                iPixelType = WinGdi.PFD_TYPE_RGBA, // Требуется RGBA формат
                cColorBits = 32, // Выбирается бит глубины цвета
                cDepthBits = 16, // 32 битный Z-буфер (буфер глубины)
                cStencilBits = 8, // Нет буфера трафарета
                iLayerType = WinGdi.PFD_MAIN_PLANE // Главный слой рисования
            };
            pfd.Init();

            // Получить Контекст Устройства
            hDC = WinUser.GetDC(hWnd);
            if (hDC == IntPtr.Zero)
            {
                throw new Exception(Sr.CantCreateAOpenGLDeviceContext);
            }

            // Подходящий формат пикселя
            int iPixelformat = WinGdi.ChoosePixelFormat(hDC, pfd);
            if (iPixelformat == 0)
            {
                throw new Exception(Sr.CantFindASuitablePixelFormat);
            }
            // Установить формат пикселя
            if (WinGdi.SetPixelFormat(hDC, iPixelformat, pfd) == 0)
            {
                throw new Exception(Sr.CantSetThePixelFormat);
            }
            // Установить контекст рендеринга
            gl.Create(hDC, openGLVersion);

            // Показать окно
            WinUser.ShowWindow(hWnd, WinUser.SW_SHOW);
            // Слегка повысим приоритет
            WinUser.SetForegroundWindow(hWnd);
            // Установить фокус клавиатуры на наше окно
            WinUser.SetFocus(hWnd);

            // Если полный экран по умолчанию, запускаем
            if (FullScreen)
            {
                SetFullScreen(true);
            }

            // Включать/выключать vsync
            gl.SwapIntervalEXT(vSync ? 1 : 0);

            // Тут графический метод первого запуска OpenGL
            OnOpenGLInitialized();
            // Обменивается передним и задним буферами
            WinGdi.SwapBuffers(hDC);

            running = true;
        }

        /// <summary>
        /// Основной цикл окна
        /// </summary>
        private void Loop()
        {
            MSG msg = new MSG();
            while (running)
            {
                if (WinUser.PeekMessage(ref msg, IntPtr.Zero, 0, 0, WinUser.PM_REMOVE))
                {
                    if (msg.message == WinUser.WM_QUIT)
                    {
                        Close();
                    }
                    else
                    {
                        // Переводим сообщение
                        WinUser.TranslateMessage(ref msg);
                        // Отсылаем сообщение
                        WinUser.DispatchMessage(ref msg);
                    }
                }
                else
                {
                    // Проверяем выход мыши за пределы окна
                    if (mouseEntity)
                    {
                        Vector2i pos = WinUser.GetCursorPos();
                        if (pos.X < LocationX || pos.Y < LocationY
                            || pos.X > LocationX + Width
                            || pos.Y > LocationY + Height)
                        {
                            mouseEntity = false;
                            OnMouseLeave();
                        }
                    }
                    LoopTick();
                }
            }
        }

        /// <summary>
        /// Очистить
        /// </summary>
        private void CleanUp()
        {
            CleanWindow();
            gl.Destroy();
            if (hDC != IntPtr.Zero)
            {
                WinUser.ReleaseDC(hWnd, hDC);
                hDC = IntPtr.Zero;
            }
            WinUser.DestroyWindow(hWnd);
            hWnd = IntPtr.Zero;
            WinUser.UnregisterClass(NameClass, hInstance);
            hInstance = IntPtr.Zero;
            lpfnWndProcMain = null;
        }

        /// <summary>
        /// Вызывается перед очисткой окна
        /// </summary>
        protected virtual void CleanWindow() { }

        /// <summary>
        /// Тик в лупе
        /// </summary>
        protected virtual void LoopTick() { }

        /// <summary>
        /// Прорисовать кадр, с заменой буфера
        /// </summary>
        public void DrawFrame()
        {
            // Тут прорисовка OnDraw
            OnOpenGlDraw();
            // Обменивается передним и задним буферами
            WinGdi.SwapBuffers(hDC);
        }
        
        /// <summary>
        /// Закрыть приложение
        /// </summary>
        protected virtual void Close() => running = false;
        /// <summary>
        /// Работает ли луп
        /// </summary>
        protected bool IsRunning() => running;

        /// <summary>
        /// Задать режим экрана
        /// </summary>
        public virtual void SetFullScreen(bool fullScreen)
        {
            FullScreen = fullScreen;

            // В помощь https://devblogs.microsoft.com/oldnewthing/20100412-00/?p=14353

            // Меняем стиль
            // Расширенный стиль окна
            WindowStylesEx dwExStyle = (WindowStylesEx)WinUser.GetWindowLong(hWnd, WinUser.GWL_EXSTYLE);
            // Обычный стиль окна
            WindowStyles dwStyle = (WindowStyles)WinUser.GetWindowLong(hWnd, WinUser.GWL_STYLE);

            int width = windowWidth;
            int height = windowHeigt;

            if (FullScreen)
            {
                // Определяем экран, по стартовой точки
                Vector2i pos = new Vector2i(LocationX, LocationY);
                IntPtr hMonitor = WinUser.MonitorFromPoint(pos, WinUser.MONITOR_DEFAULTTONEAREST);
                // Получаем информацию монитора
                MonitorInfoEx monitorInfo = new MonitorInfoEx();
                if (!WinUser.GetMonitorInfo(hMonitor, monitorInfo))
                {
                    throw new Exception(Sr.FailedToGetMonitorInformation);
                }
                // Получаем размер и расположение текущего окна
                RectStruct rect = new RectStruct();
                if (!WinUser.GetWindowRect(hWnd, ref rect))
                {
                    throw new Exception(Sr.FailedToGetWindowRect);
                }

                windowLocationX = rect.Left;
                windowLocationY = rect.Top;
                windowWidth = rect.Right - rect.Left;
                windowHeigt = rect.Bottom - rect.Top;

                LocationX = monitorInfo.Monitor.Left;
                LocationY = monitorInfo.Monitor.Top;
                width = monitorInfo.Monitor.Right - monitorInfo.Monitor.Left;
                height = monitorInfo.Monitor.Bottom - monitorInfo.Monitor.Top;

                dwStyle = dwStyle & ~WindowStyles.WS_OVERLAPPEDWINDOW;
                dwExStyle = dwExStyle & ~WindowStylesEx.WS_EX_WINDOWEDGE;

                // Кастыльное решение, при старте или смене экрана, первый раз F11, открывается не с 0:0
                if (!hMonitorPrev.Equals(hMonitor))
                {
                    hMonitorPrev = hMonitor;
                }
                else
                {
                    WinUser.SetWindowPos(hWnd, IntPtr.Zero, LocationX, LocationY, width, height,
                        WinUser.SWP_NOOWNERZORDER | WinUser.SWP_FRAMECHANGED);
                }
            }
            else
            {
                LocationX = windowLocationX;
                LocationY = windowLocationY;
                dwExStyle = dwExStyle | WindowStylesEx.WS_EX_WINDOWEDGE;
                dwStyle = dwStyle | WindowStyles.WS_OVERLAPPEDWINDOW;
            }
            
            WinUser.SetWindowLong(hWnd, WinUser.GWL_STYLE, (int)dwStyle);
            WinUser.SetWindowLong(hWnd, WinUser.GWL_EXSTYLE, (int)dwExStyle);
            WinUser.SetWindowPos(hWnd, IntPtr.Zero, LocationX, LocationY, width, height,
                WinUser.SWP_NOZORDER | WinUser.SWP_NOOWNERZORDER | WinUser.SWP_FRAMECHANGED);
        }

        #endregion

        #region OnWindow

        /// <summary>
        /// Происходит перед остановкой приложения
        /// </summary>
        protected virtual void OnStoping() { }

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        protected virtual void OnResized(int width, int height)
        {
            Width = width;
            Height = height;

            Glm.Ortho(0, Width, Height, 0, 0, 1).ConvArray(Ortho2D);
            gl.Viewport(0, 0, width, height);
        }

        /// <summary>
        /// Перемещено окно
        /// </summary>
        protected virtual void OnMove(int locationX, int locationY)
        {
            LocationX = locationX;
            LocationY = locationY;
            if (!FullScreen)
            {
                windowLocationX = LocationX;
                windowLocationY = LocationY;
            }
        }

        /// <summary>
        /// Активация или деакциваия окна
        /// </summary>
        protected virtual void OnActivate(bool active) { }

        #endregion 

        #region OnOpenGl

        /// <summary>
        /// Прорисовка кадра
        /// </summary>
        protected virtual void OnOpenGlDraw() { }

        /// <summary>
        /// Инициализировать, первый запуск OpenGL
        /// </summary>
        protected virtual void OnOpenGLInitialized() { }

        #endregion

        #region OnMouse

        /// <summary>
        /// Перемещение мыши по окну
        /// </summary>
        protected virtual void OnMouseMove(int x, int y) { }
        /// <summary>
        /// Нажатие курсора мыши
        /// </summary>
        protected virtual void OnMouseDown(MouseButton button, int x, int y) { }
        /// <summary>
        /// Отпустил курсор мыши
        /// </summary>
        protected virtual void OnMouseUp(MouseButton button, int x, int y) { }
        /// <summary>
        /// Вращение колёсика мыши
        /// </summary>
        protected virtual void OnMouseWheel(int delta, int x, int y) { }
        /// <summary>
        /// Курсор вышел за пределы окна
        /// </summary>
        protected virtual void OnMouseLeave() { }
        /// <summary>
        /// Курсор вошёл в пределы окна
        /// </summary>
        protected virtual void OnMouseEnter() { }

        #endregion

        #region OnKey

        /// <summary>
        /// Клавиша нажата
        /// </summary>
        protected virtual void OnKeyDown(Keys keys) { }

        /// <summary>
        /// Клавиша отпущена
        /// </summary>
        protected virtual void OnKeyUp(Keys keys) { }

        /// <summary>
        /// Нажата клавиша в char формате
        /// </summary>
        protected virtual void OnKeyPress(char key) { }

        #endregion

        /// <summary>
        /// Функция обратного вызова, определяемая в приложении, 
        /// которая обрабатывает сообщения, отправляемые в окно. 
        /// </summary>
        /// <param name="hWnd">Дескриптор окна</param>
        /// <param name="msg">Сообщение</param>
        /// <param name="wParam">Дополнительные сведения о сообщении</param>
        /// <param name="lParam">Дополнительные сведения о сообщении</param>
        private IntPtr WndProcPrivate(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam)
        {
            int param;

            // Тут надо подготовить все возможные события
            switch (uMsg)
            {
                #region Window

                case WinUser.WM_NCCREATE:
                    WinUser.EnableNonClientDpiScaling(hWnd);
                    break;
                // Закрыто окно
                case WinUser.WM_CLOSE:
                    Close();
                    return IntPtr.Zero;
                // Изменён размер окна
                case WinUser.WM_SIZE:
                    param = lParam.ToInt32();
                    // param = Conv.IntPtrToUint(lParam);
                    OnResized(param & 0xFFFF, param >> 16);
                    return IntPtr.Zero;
                // Перемещение окна
                case WinUser.WM_MOVE:
                    if (lParam.ToInt64() == 2197848832)
                    {
                        // Свернули окно
                    }
                    else if (lParam.ToInt64() > 2147483647)
                    {
                        // Вроде как на весь экран. Методом подбора
                    }
                    else
                    {
                        param = lParam.ToInt32();
                        OnMove((short)(param & 0xFFFF), (short)(param >> 16));
                    }
                    return IntPtr.Zero;
                case WinUser.WM_ACTIVATE:
                    param = wParam.ToInt32();
                    OnActivate(param != 0);
                    return IntPtr.Zero;
                case WinUser.WM_DPICHANGED:
                    // Чтоб это отлавливать, надо в манифесте установить
                    // https://github.com/microsoft/WPF-Samples/blob/main/PerMonitorDPI/readme.md#1turn-on-windows-level-per-monitor-dpi-awareness-in-appmanifest
                    param = wParam.ToInt32();
                    //OnDpiChanged();
                    return IntPtr.Zero;

                #endregion

                #region Mouse

                // Движение мышью
                case WinUser.WM_MOUSEMOVE:
                    param = lParam.ToInt32();
                    OnMouseMove(param & 0xFFFF, param >> 16);
                    if (!mouseEntity)
                    {
                        mouseEntity = true;
                        OnMouseEnter();
                    }
                    return IntPtr.Zero;
                // Нажата левая клавиша мыши
                case WinUser.WM_LBUTTONDOWN:
                    param = lParam.ToInt32();
                    OnMouseDown(MouseButton.Left, param & 0xFFFF, param >> 16);
                    return IntPtr.Zero;
                // Нажата правая клавиша мыши
                case WinUser.WM_RBUTTONDOWN:
                    param = lParam.ToInt32();
                    OnMouseDown(MouseButton.Right, param & 0xFFFF, param >> 16);
                    return IntPtr.Zero;
                // Нажата средняя клавиша мыши
                case WinUser.WM_MBUTTONDOWN:
                    param = lParam.ToInt32();
                    OnMouseDown(MouseButton.Middle, param & 0xFFFF, param >> 16);
                    return IntPtr.Zero;
                // Отпущена левая клавиша мыши
                case WinUser.WM_LBUTTONUP:
                    param = lParam.ToInt32();
                    OnMouseUp(MouseButton.Left, param & 0xFFFF, param >> 16);
                    return IntPtr.Zero;
                // Отпущена правая клавиша мыши
                case WinUser.WM_RBUTTONUP:
                    param = lParam.ToInt32();
                    OnMouseUp(MouseButton.Right, param & 0xFFFF, param >> 16);
                    return IntPtr.Zero;
                // Отпущена средняя клавиша мыши
                case WinUser.WM_MBUTTONUP:
                    param = lParam.ToInt32();
                    OnMouseUp(MouseButton.Middle, param & 0xFFFF, param >> 16);
                    return IntPtr.Zero;
                // Вращение колёсика мыши
                case WinUser.WM_MOUSEWHEEL:
                    param = lParam.ToInt32();
                    OnMouseWheel((short)(wParam.ToInt64() >> 16), param & 0xFFFF, param >> 16);
                    return IntPtr.Zero;

                #endregion

                #region Key

                // Отпущена клавиша клавиатуры
                case WinUser.WM_SYSKEYUP:
                case WinUser.WM_KEYUP:
                    OnKeyUp((Keys)wParam.ToInt32());
                    return IntPtr.Zero;
                // Нажата клавиша клавиатуры
                case WinUser.WM_SYSKEYDOWN:
                case WinUser.WM_KEYDOWN:
                    OnKeyDown((Keys)wParam.ToInt32());
                    return IntPtr.Zero;
                // Нажата системная клавиша, нужна заглушка, чтоб не было звука
                case WinUser.WM_SYSCHAR:
                    return IntPtr.Zero;
                // Нажата клавиша клавиатуры для написания текста
                case WinUser.WM_CHAR:
                    OnKeyPress((char)wParam);
                    return IntPtr.Zero;

                #endregion
                
            }

            return WinUser.DefWindowProc(hWnd, uMsg, wParam, lParam);
        }

        /// <summary>
        /// Заменить название заголовка
        /// </summary>
        protected void SetTitle(string title)
        {
            Title = title;
            if (hWnd != IntPtr.Zero)
            {
                WinUser.SetWindowText(hWnd, title);
            }
        }

        /// <summary>
        /// Указать виден ли курсор
        /// </summary>
        public void CursorShow(bool bShow) => WinUser.CursorShow(bShow);

        /// <summary>
        /// Задать позицию курсора в окне
        /// </summary>
        public void SetCursorPosition(int x, int y) 
            => WinUser.SetCursorPosition(x + LocationX, y + LocationY);

        /// <summary>
        /// Включить или выключить вертикальную сенхронизацию
        /// </summary>
        public virtual void SetVSync(bool on)
        {
            if (vSync != on)
            {
                vSync = on;
                gl.SwapIntervalEXT(vSync ? 1 : 0);
            }
        }
    }
}
