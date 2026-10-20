using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Forms = System.Windows.Forms;

namespace DesktopPet
{
    public class PetWindow : Window
    {
        // =========================================================================
        // CUTE OWL — LOW-RES PIXEL ART EDITION (48x48)
        // =========================================================================

        // Configurable Low-Res Pixel Art Parameters
        public const int NativeWidth = 48;       // Native canvas width (32, 48, or 64)
        public const int NativeHeight = 48;      // Native canvas height
        public static int ScaleFactor = 3;       // Integer scale multiplier (3x = 144x144, 4x = 192x192)

        private Canvas rootCanvas;
        private Canvas owlContainer;
        private Image owlSprite;
        private Canvas placeholderCanvas;
        private ScaleTransform owlScale;
        private Forms.NotifyIcon trayIcon;

        // Discrete Sprite Strips
        private BitmapImage[] idleFrames;
        private BitmapImage[] walkFrames;
        private BitmapImage[] sleepFrames;
        private BitmapImage[] flyFrames;
        private BitmapImage idleBlinkFrame;
        private bool hasLoadedSprites = false;

        // Animation State: "IDLE", "WALKING", "SLEEPING", "FLYING"
        private string currentState = "IDLE";
        private int currentFrameIndex = 0;
        private bool isBlinking = false;
        private double walkDirection = 1.0; // 1.0 = Right, -1.0 = Left
        private double floorY;

        // Frame Timing (ms)
        private readonly int[] idleTiming = new int[] { 200, 180, 180, 200 };
        private readonly int[] walkTiming = new int[] { 120, 160, 120, 160 };
        private readonly int[] sleepTiming = new int[] { 500, 500, 500, 500 };
        private readonly int[] flyTiming = new int[] { 85, 85, 85, 85, 85, 85 };
        private const double WalkStepSize = 6.0; // Movement per step in screen pixels

        // Timers & AFK
        private DispatcherTimer animTimer;
        private DispatcherTimer blinkTimer;
        private DispatcherTimer behaviorTimer;
        private DispatcherTimer afkTimer;
        private DateTime lastInteractionTime = DateTime.UtcNow;
        private const double AfkSleepThresholdSeconds = 10.0;
        private Random rnd = new Random();

        // Mouse Dragging & Gliding
        private bool isMouseDown = false;
        private bool isDraggingActive = false;
        private Point dragCursorOffset;
        private Point dragStartScreen;
        private Point lastDragPoint;
        private DispatcherTimer glideTimer;
        private double glideTime = 0.0;

        [STAThread]
        public static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                Forms.MessageBox.Show(e.ExceptionObject.ToString(), "Pet Error");
            };

            Application app = new Application();
            PetWindow win = new PetWindow();
            app.Run(win);
        }

        public PetWindow()
        {
            this.Title = "Cute Owl (Low-Res)";
            this.WindowStyle = WindowStyle.None;
            this.AllowsTransparency = true;
            this.Background = Brushes.Transparent;
            this.Topmost = true;
            this.ShowInTaskbar = false;
            this.SnapsToDevicePixels = true;
            this.UseLayoutRounding = true;

            // Pixel-art rendering: nearest-neighbor scaling, no bilinear blur
            RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.NearestNeighbor);
            RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);

            UpdateWindowDimensions();

            LoadSprites();
            BuildUI();
            SetupTray();
            SetupTimers();
            SetupMouseInteraction();
        }

        private void UpdateWindowDimensions()
        {
            int displayW = NativeWidth * ScaleFactor;
            int displayH = NativeHeight * ScaleFactor;

            this.Width = displayW + 30;
            this.Height = displayH + 30;

            Rect workArea = SystemParameters.WorkArea;
            this.floorY = workArea.Bottom - this.Height;
            if (this.Left == 0 || double.IsNaN(this.Left))
            {
                this.Left = workArea.Right - this.Width - 100;
            }
            this.Top = this.floorY;
        }

        // =========================================================================
        // 1. ASSET LOADING
        // =========================================================================

        private void LoadSprites()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            idleFrames = LoadSpriteStrip(System.IO.Path.Combine(baseDir, @"assets\sprites\idle"));
            walkFrames = LoadSpriteStrip(System.IO.Path.Combine(baseDir, @"assets\sprites\walk"));
            sleepFrames = LoadSpriteStrip(System.IO.Path.Combine(baseDir, @"assets\sprites\sleep"));

            var rawFly = LoadSpriteStrip(System.IO.Path.Combine(baseDir, @"assets\sprites\fly"));
            if (rawFly != null && rawFly.Length == 4)
            {
                // Smooth 6-beat ping-pong flap cycle: Up -> MidUp -> MidDown -> Down -> MidDown -> MidUp
                flyFrames = new BitmapImage[] { rawFly[0], rawFly[1], rawFly[2], rawFly[3], rawFly[2], rawFly[1] };
            }
            else
            {
                flyFrames = rawFly;
            }

            string blinkPath = System.IO.Path.Combine(baseDir, @"assets\sprites\idle\idle_blink.png");
            if (File.Exists(blinkPath))
            {
                idleBlinkFrame = LoadSingleBitmap(blinkPath);
            }

            hasLoadedSprites = (idleFrames != null && idleFrames.Length > 0 && idleFrames[0] != null);
        }

        private BitmapImage[] LoadSpriteStrip(string dir)
        {
            if (!Directory.Exists(dir)) return new BitmapImage[0];

            var files = Directory.GetFiles(dir, "frame_*.png");
            if (files.Length == 0) return new BitmapImage[0];

            Array.Sort(files, (a, b) => ExtractFrameNumber(a).CompareTo(ExtractFrameNumber(b)));

            BitmapImage[] frames = new BitmapImage[files.Length];
            for (int i = 0; i < files.Length; i++)
            {
                frames[i] = LoadSingleBitmap(files[i]);
            }
            return frames;
        }

        private int ExtractFrameNumber(string path)
        {
            string fname = System.IO.Path.GetFileNameWithoutExtension(path);
            int idx = fname.LastIndexOf('_');
            if (idx >= 0 && idx < fname.Length - 1)
            {
                int val;
                if (int.TryParse(fname.Substring(idx + 1), out val)) return val;
            }
            return 0;
        }

        private BitmapImage LoadSingleBitmap(string path)
        {
            try
            {
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.UriSource = new Uri(path, UriKind.Absolute);
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.EndInit();
                bi.Freeze();
                return bi;
            }
            catch { return null; }
        }

        // =========================================================================
        // 2. UI & SCENE COMPOSITION
        // =========================================================================

        private void BuildUI()
        {
            int displayW = NativeWidth * ScaleFactor;
            int displayH = NativeHeight * ScaleFactor;

            rootCanvas = new Canvas
            {
                Width = this.Width,
                Height = this.Height,
                Background = Brushes.Transparent
            };

            owlContainer = new Canvas
            {
                Width = displayW,
                Height = displayH,
                Background = Brushes.Transparent
            };
            Canvas.SetLeft(owlContainer, 15);
            Canvas.SetTop(owlContainer, 15);

            owlScale = new ScaleTransform(1.0, 1.0, displayW / 2.0, displayH);
            owlContainer.RenderTransform = owlScale;

            if (hasLoadedSprites)
            {
                owlSprite = new Image
                {
                    Width = displayW,
                    Height = displayH,
                    Cursor = Cursors.Hand
                };
                RenderOptions.SetBitmapScalingMode(owlSprite, BitmapScalingMode.NearestNeighbor);
                RenderOptions.SetEdgeMode(owlSprite, EdgeMode.Aliased);
                owlSprite.Source = idleFrames[0];
                owlContainer.Children.Add(owlSprite);
            }
            else
            {
                // Placeholder when awaiting low-res graphics
                placeholderCanvas = CreatePlaceholderVisual(displayW, displayH);
                owlContainer.Children.Add(placeholderCanvas);
            }

            rootCanvas.Children.Add(owlContainer);
            this.Content = rootCanvas;
        }

        private Canvas CreatePlaceholderVisual(int w, int h)
        {
            Canvas c = new Canvas { Width = w, Height = h, Cursor = Cursors.Hand };

            // Outer dashed bounding box
            Rectangle border = new Rectangle
            {
                Width = w,
                Height = h,
                Stroke = new SolidColorBrush(Color.FromArgb(180, 229, 169, 60)), // Steampunk Gold
                StrokeThickness = 2,
                StrokeDashArray = new DoubleCollection { 4, 3 },
                Fill = new SolidColorBrush(Color.FromArgb(90, 20, 24, 34))
            };
            c.Children.Add(border);

            // Retro owl pixel icon
            TextBlock iconText = new TextBlock
            {
                Text = "🦉",
                FontSize = Math.Max(24, h * 0.35),
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                Width = w
            };
            Canvas.SetTop(iconText, h * 0.15);
            c.Children.Add(iconText);

            // Label
            TextBlock label = new TextBlock
            {
                Text = string.Format("Low-Res {0}x{1}\n(Ready for Sprites)", NativeWidth, NativeHeight),
                FontSize = 10,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
                TextAlignment = TextAlignment.Center,
                Width = w
            };
            Canvas.SetTop(label, h * 0.60);
            c.Children.Add(label);

            return c;
        }

        // =========================================================================
        // 3. ANIMATION & TIMERS
        // =========================================================================

        private void SetupTimers()
        {
            animTimer = new DispatcherTimer();
            animTimer.Interval = TimeSpan.FromMilliseconds(200);
            animTimer.Tick += OnAnimTick;
            animTimer.Start();

            blinkTimer = new DispatcherTimer();
            blinkTimer.Interval = TimeSpan.FromSeconds(3.5 + rnd.NextDouble() * 2.0);
            blinkTimer.Tick += (s, e) =>
            {
                blinkTimer.Interval = TimeSpan.FromSeconds(3.5 + rnd.NextDouble() * 2.0);
                if (currentState == "IDLE" && !isBlinking && idleBlinkFrame != null)
                {
                    TriggerBlink();
                }
            };
            blinkTimer.Start();

            behaviorTimer = new DispatcherTimer();
            behaviorTimer.Interval = TimeSpan.FromSeconds(5.0);
            behaviorTimer.Tick += (s, e) =>
            {
                behaviorTimer.Interval = TimeSpan.FromSeconds(4.0 + rnd.NextDouble() * 4.0);
                if (isMouseDown) return;
                if (currentState == "SLEEPING" || currentState == "FLYING") return; // Do not walk during sleep or flight!

                if (currentState == "IDLE")
                {
                    if (rnd.NextDouble() < 0.65)
                    {
                        Rect workArea = SystemParameters.WorkArea;
                        if (this.Left <= 60) walkDirection = 1.0;
                        else if (this.Left >= workArea.Right - this.Width - 60) walkDirection = -1.0;
                        else walkDirection = (rnd.Next(2) == 0) ? 1.0 : -1.0;

                        SetState("WALKING");
                    }
                }
                else if (currentState == "WALKING")
                {
                    SetState("IDLE");
                    RegisterInteraction();
                }
            };
            behaviorTimer.Start();

            // AFK Sleep Timer (Checks every 500ms; after 10s of inactivity -> SLEEPING)
            afkTimer = new DispatcherTimer();
            afkTimer.Interval = TimeSpan.FromMilliseconds(500);
            afkTimer.Tick += (s, e) =>
            {
                if (isMouseDown) return;
                if (currentState == "IDLE")
                {
                    double elapsed = (DateTime.UtcNow - lastInteractionTime).TotalSeconds;
                    if (elapsed >= AfkSleepThresholdSeconds)
                    {
                        SetState("SLEEPING");
                    }
                }
            };
            afkTimer.Start();
        }

        private void OnAnimTick(object sender, EventArgs e)
        {
            if (isBlinking && currentState == "IDLE") return;

            BitmapImage[] activeStrip;
            int[] timing;

            if (currentState == "WALKING")
            {
                activeStrip = walkFrames;
                timing = walkTiming;
            }
            else if (currentState == "SLEEPING")
            {
                activeStrip = sleepFrames;
                timing = sleepTiming;
            }
            else if (currentState == "FLYING")
            {
                activeStrip = flyFrames;
                timing = flyTiming;
            }
            else // IDLE
            {
                activeStrip = idleFrames;
                timing = idleTiming;
            }

            if (hasLoadedSprites && activeStrip != null && activeStrip.Length > 0)
            {
                currentFrameIndex = (currentFrameIndex + 1) % activeStrip.Length;
                if (activeStrip[currentFrameIndex] != null && owlSprite != null)
                {
                    owlSprite.Source = activeStrip[currentFrameIndex];
                }
            }
            else
            {
                // Placeholder frame advance (discrete 4-beat pulse)
                currentFrameIndex = (currentFrameIndex + 1) % 4;
            }

            // Step-locked walking
            if (currentState == "WALKING" && !isMouseDown)
            {
                if (currentFrameIndex == 1 || currentFrameIndex == 3)
                {
                    this.Left += walkDirection * WalkStepSize;

                    Rect workArea = SystemParameters.WorkArea;
                    if (this.Left <= 10)
                    {
                        this.Left = 10;
                        walkDirection = 1.0;
                        owlScale.ScaleX = 1.0;
                    }
                    else if (this.Left >= workArea.Right - this.Width - 10)
                    {
                        this.Left = workArea.Right - this.Width - 10;
                        walkDirection = -1.0;
                        owlScale.ScaleX = -1.0;
                    }
                }
            }

            int nextFrame = (currentFrameIndex + 1) % timing.Length;
            int nextMs = timing[nextFrame];
            animTimer.Interval = TimeSpan.FromMilliseconds(nextMs);
        }

        private void SetState(string newState)
        {
            if (currentState == newState) return;
            currentState = newState;
            currentFrameIndex = 0;

            if (currentState == "WALKING")
            {
                owlScale.ScaleX = walkDirection;
                if (hasLoadedSprites && walkFrames != null && walkFrames.Length > 0 && owlSprite != null)
                {
                    owlSprite.Source = walkFrames[0];
                }
                animTimer.Interval = TimeSpan.FromMilliseconds(walkTiming[0]);
            }
            else if (currentState == "SLEEPING")
            {
                owlScale.ScaleX = 1.0;
                if (hasLoadedSprites && sleepFrames != null && sleepFrames.Length > 0 && owlSprite != null)
                {
                    owlSprite.Source = sleepFrames[0];
                }
                animTimer.Interval = TimeSpan.FromMilliseconds(sleepTiming[0]);
            }
            else if (currentState == "FLYING")
            {
                owlScale.ScaleX = walkDirection;
                if (hasLoadedSprites && flyFrames != null && flyFrames.Length > 0 && owlSprite != null)
                {
                    owlSprite.Source = flyFrames[0];
                }
                animTimer.Interval = TimeSpan.FromMilliseconds(flyTiming[0]);
            }
            else // IDLE
            {
                owlScale.ScaleX = 1.0;
                if (hasLoadedSprites && idleFrames != null && idleFrames.Length > 0 && owlSprite != null)
                {
                    owlSprite.Source = idleFrames[0];
                }
                animTimer.Interval = TimeSpan.FromMilliseconds(idleTiming[0]);
            }
        }

        private void RegisterInteraction()
        {
            lastInteractionTime = DateTime.UtcNow;
        }

        private void WakeUp()
        {
            RegisterInteraction();
            SetState("IDLE");
        }

        private void TriggerBlink()
        {
            isBlinking = true;
            if (owlSprite != null && idleBlinkFrame != null)
            {
                owlSprite.Source = idleBlinkFrame;
            }

            DispatcherTimer unblink = new DispatcherTimer();
            unblink.Interval = TimeSpan.FromMilliseconds(130);
            unblink.Tick += (s, args) =>
            {
                unblink.Stop();
                isBlinking = false;
                if (currentState == "IDLE" && hasLoadedSprites && idleFrames != null && currentFrameIndex < idleFrames.Length)
                {
                    if (owlSprite != null) owlSprite.Source = idleFrames[currentFrameIndex];
                }
            };
            unblink.Start();
        }

        // =========================================================================
        // 4. MOUSE INTERACTION & MENUS
        // =========================================================================

        private Point GetCursorScreenDip()
        {
            var pt = Forms.Cursor.Position;
            var source = PresentationSource.FromVisual(this);
            if (source != null && source.CompositionTarget != null)
            {
                return source.CompositionTarget.TransformFromDevice.Transform(new Point(pt.X, pt.Y));
            }
            return new Point(pt.X, pt.Y);
        }

        private void SetupMouseInteraction()
        {
            FrameworkElement target = (FrameworkElement)owlSprite ?? (FrameworkElement)placeholderCanvas ?? (FrameworkElement)owlContainer;

            target.MouseLeftButtonDown += (s, e) =>
            {
                RegisterInteraction();
                if (glideTimer != null && glideTimer.IsEnabled)
                {
                    glideTimer.Stop();
                }

                if (currentState == "SLEEPING")
                {
                    WakeUp();
                }

                isMouseDown = true;
                isDraggingActive = false;
                Point cur = GetCursorScreenDip();
                dragCursorOffset = new Point(cur.X - this.Left, cur.Y - this.Top);
                dragStartScreen = cur;
                lastDragPoint = cur;
                target.CaptureMouse();
                e.Handled = true;
            };

            target.MouseMove += (s, e) =>
            {
                if (isMouseDown)
                {
                    RegisterInteraction();
                    Point cur = GetCursorScreenDip();
                    double dx = cur.X - dragStartScreen.X;
                    double dy = cur.Y - dragStartScreen.Y;
                    double totalDist = Math.Sqrt(dx * dx + dy * dy);

                    // If dragged actively (more than 6px), activate FLIGHT!
                    if (totalDist > 6.0)
                    {
                        isDraggingActive = true;
                        if (currentState != "FLYING")
                        {
                            SetState("FLYING");
                        }

                        // Determine horizontal flight facing direction
                        double deltaX = cur.X - lastDragPoint.X;
                        if (deltaX > 2.0)
                        {
                            walkDirection = 1.0;
                            owlScale.ScaleX = 1.0;
                        }
                        else if (deltaX < -2.0)
                        {
                            walkDirection = -1.0;
                            owlScale.ScaleX = -1.0;
                        }
                    }

                    this.Left = cur.X - dragCursorOffset.X;
                    this.Top = cur.Y - dragCursorOffset.Y;
                    lastDragPoint = cur;
                    e.Handled = true;
                }
            };

            target.MouseLeftButtonUp += (s, e) =>
            {
                if (isMouseDown)
                {
                    isMouseDown = false;
                    target.ReleaseMouseCapture();
                    RegisterInteraction();

                    if (!isDraggingActive)
                    {
                        // Simple click without active drag
                        if (currentState == "IDLE")
                        {
                            SetState("WALKING");
                        }
                        else if (currentState == "WALKING")
                        {
                            SetState("IDLE");
                        }
                    }
                    else
                    {
                        // Finished dragging: check altitude
                        if (this.Top < this.floorY - 6.0)
                        {
                            // Released in the air: smooth gliding descent down to floor!
                            StartGliding();
                        }
                        else
                        {
                            // Released near floor
                            this.Top = this.floorY;
                            SetState("IDLE");
                        }
                    }
                    e.Handled = true;
                }
            };

            // Context Menu
            ContextMenu cm = new ContextMenu();
            MenuItem toggleItem = new MenuItem { Header = "Сменить состояние (Покой / Ходьба)" };
            toggleItem.Click += (s, e) =>
            {
                RegisterInteraction();
                SetState(currentState == "IDLE" ? "WALKING" : "IDLE");
            };

            MenuItem sleepItem = new MenuItem { Header = "Уложить спать / Разбудить" };
            sleepItem.Click += (s, e) =>
            {
                RegisterInteraction();
                if (currentState == "SLEEPING") WakeUp();
                else SetState("SLEEPING");
            };

            MenuItem flyItem = new MenuItem { Header = "Полёт / Приземлиться" };
            flyItem.Click += (s, e) =>
            {
                RegisterInteraction();
                if (currentState == "FLYING")
                {
                    StartGliding();
                }
                else
                {
                    this.Top = Math.Max(100, this.floorY - 200);
                    SetState("FLYING");
                    StartGliding();
                }
            };

            MenuItem exitItem = new MenuItem { Header = "Выход" };
            exitItem.Click += (s, e) => ExitApp();

            cm.Items.Add(toggleItem);
            cm.Items.Add(sleepItem);
            cm.Items.Add(flyItem);
            cm.Items.Add(new Separator());
            cm.Items.Add(exitItem);

            target.ContextMenu = cm;
        }

        private void StartGliding()
        {
            if (glideTimer != null) glideTimer.Stop();

            glideTime = 0.0;
            glideTimer = new DispatcherTimer();
            glideTimer.Interval = TimeSpan.FromMilliseconds(20); // 50 FPS smooth descent
            glideTimer.Tick += (s, e) =>
            {
                if (isMouseDown)
                {
                    glideTimer.Stop();
                    return;
                }

                glideTime += 0.03;
                double descentSpeed = 3.5;
                this.Top += descentSpeed;

                // Gentle horizontal drift
                this.Left += Math.Sin(glideTime * 3.0) * 0.6;

                // Boundary protection
                Rect workArea = SystemParameters.WorkArea;
                if (this.Left < 10) this.Left = 10;
                if (this.Left > workArea.Right - this.Width - 10) this.Left = workArea.Right - this.Width - 10;

                if (this.Top >= this.floorY)
                {
                    this.Top = this.floorY;
                    glideTimer.Stop();
                    SetState("IDLE");
                }
            };
            glideTimer.Start();
        }

        private void SetupTray()
        {
            trayIcon = new Forms.NotifyIcon
            {
                Text = "Cute Owl (Low-Res)",
                Visible = true,
                Icon = System.Drawing.SystemIcons.Application
            };

            Forms.ContextMenuStrip trayMenu = new Forms.ContextMenuStrip();
            trayMenu.Items.Add("Покой / Ходьба", null, (s, e) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    RegisterInteraction();
                    SetState(currentState == "IDLE" ? "WALKING" : "IDLE");
                });
            });
            trayMenu.Items.Add("Уложить спать / Разбудить", null, (s, e) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    RegisterInteraction();
                    if (currentState == "SLEEPING") WakeUp();
                    else SetState("SLEEPING");
                });
            });
            trayMenu.Items.Add("Полёт / Приземлиться", null, (s, e) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    RegisterInteraction();
                    if (currentState == "FLYING")
                    {
                        StartGliding();
                    }
                    else
                    {
                        this.Top = Math.Max(100, this.floorY - 200);
                        SetState("FLYING");
                        StartGliding();
                    }
                });
            });
            trayMenu.Items.Add(new Forms.ToolStripSeparator());
            trayMenu.Items.Add("Выход", null, (s, e) => ExitApp());

            trayIcon.ContextMenuStrip = trayMenu;
        }

        private void ExitApp()
        {
            if (trayIcon != null)
            {
                trayIcon.Visible = false;
                trayIcon.Dispose();
            }
            Application.Current.Shutdown();
        }

        protected override void OnClosed(EventArgs e)
        {
            ExitApp();
            base.OnClosed(e);
        }
    }
}
