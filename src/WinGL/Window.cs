using System;
using WinGL.Util;
using WinGL.Win32;
using WinGL.Win32.Gdi32;
using WinGL.OpenGL;
using WinGL.Win32.User32;
using WinGL.Actions;
using System.Diagnostics;

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
        public string Title { get; private set; } = "FormOpenGL";
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
        public float[] Ortho2D { get; private set; }

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
        /// Рамка ширина окна в оконном режимме
        /// </summary>
        private int windowWidthBorder;
        /// <summary>
        /// Рамка высота окна в оконном режимме
        /// </summary>
        private int windowHeigtBorder;
        /// <summary>
        /// Был ли первый запуск, чтоб определеить размер рамки
        /// </summary>
        private bool flagWindowSizeBorder = false;
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
        /// Объект делегата приёма ответов, делается глобальный, чтоб не удалялся из-за чистки мусора
        /// </summary>
        private WndProc lpfnWndProcMain;
        /// <summary>
        /// Работает ли глобальный loop
        /// </summary>
        private bool running = false;
        /// <summary>
        /// Флаг перезапуска программы
        /// </summary>
        private bool flagRestart = false;
        /// <summary>
        /// Использовать ли вертикальную сенхронизацию
        /// </summary>
        protected bool vSync = true;

        protected Window()
        {
            windowWidth = Width;
            windowHeigt = Height;
            windowLocationX = LocationX;
            windowLocationY = LocationY;
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
                e = ex.InnerException;
                stackTrace = e.StackTrace + "\r\n" + ex.StackTrace;
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
            // Строчка для масштаба
            WinUser.SetProcessDPIAware();

            Glm.Initialized();
            try
            {
                window.Begin();
            }
            catch (Exception ex)
            {
                MessageBoxCrash(ex);
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
                Close();
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
                if (flagRestart)
                {
                    // Если имеется флаг перезапуска, запускаем новый процесс
                    string name = Process.GetCurrentProcess().MainModule.FileName;
                    Process.Start(name);
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
            int width = windowWidth;
            int height = windowHeigt;

            // Полноэкранный режим?
            if (FullScreen)
            {
                // Режим устройства
                DevMode dm = new DevMode
                {
                    dmDeviceName = new string(new char[32]),
                    dmFormName = new string(new char[32])
                };
                dm.Init();
                // Извлекает информацию об одном из графических режимов устройства отображения
                WinUser.EnumDisplaySettings(null, WinUser.ENUM_CURRENT_SETTINGS, ref dm);
                // Пытаемся установить выбранный режим и получить результат
                // Примечание: CDS_FULLSCREEN убирает панель управления
                if (WinUser.ChangeDisplaySettings(ref dm, WinUser.CDS_FULLSCREEN) != WinUser.DISP_CHANGE_SUCCESSFUL)
                {
                    // Выбор оконного режима
                    FullScreen = false;
                }
                else
                {
                    width = dm.dmPelsWidth;
                    height = dm.dmPelsHeight;
                }
            }

            // Расширенный стиль окна
            WindowStylesEx dwExStyle;
            // Обычный стиль окна
            WindowStyles dwStyle;

            if (FullScreen)
            {
                // Мы остались в полноэкранном режиме
                dwExStyle = WindowStylesEx.WS_EX_APPWINDOW;
                dwStyle = WindowStyles.WS_POPUP | WindowStyles.WS_VISIBLE | WindowStyles.WS_MINIMIZEBOX;
                LocationX = LocationY = 0;
            }
            else
            {
                //width = windowWidth;
                //height = windowHeigt;
                LocationX = windowLocationX;
                LocationY = windowLocationY;
                dwExStyle = WindowStylesEx.WS_EX_APPWINDOW | WindowStylesEx.WS_EX_WINDOWEDGE;
                dwStyle = WindowStyles.WS_OVERLAPPEDWINDOW;
            }

            hWnd = WinUser.CreateWindowEx(
                dwExStyle,
                NameClass, Title,
                WindowStyles.WS_CLIPSIBLINGS | WindowStyles.WS_CLIPCHILDREN | dwStyle,
                LocationX, LocationY,
                width, height,
                IntPtr.Zero, IntPtr.Zero, hInstance, IntPtr.Zero);

            if (hWnd == IntPtr.Zero) 
            {
                throw new Exception(Sr.WindowCreationError);
            }

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
            // // Установить фокус клавиатуры на наше окно
            WinUser.SetFocus(hWnd);

            // Включать/выключать vsync
            if (!vSync)
            {
                gl.SwapIntervalEXT(0);
            }

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
        protected void DrawFrame()
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
        /// Перезапустить приложение
        /// </summary>
        public void Restart()
        {
            flagRestart = true;
            Close();
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
            if (!flagWindowSizeBorder)
            {
                flagWindowSizeBorder = true;
                windowWidthBorder = Width - width;
                windowHeigtBorder = Height - height;
            }
            Width = width;
            Height = height;
            if (!FullScreen)
            {
                windowWidth = width + windowWidthBorder;
                windowHeigt = height + windowHeigtBorder;
            }

            Ortho2D = Glm.Ortho(0, Width, Height, 0, 0, 1).ToArray();

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
