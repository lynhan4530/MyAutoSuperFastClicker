using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using MyAutoSuperFastClicker.Core;
using MyAutoSuperFastClicker.Hotkeys;
using MyAutoSuperFastClicker.Interop;

namespace MyAutoSuperFastClicker
{
    public partial class MainWindow : Window
    {
        private readonly ClickEngine _engine = new();
        private readonly GlobalHotkeyManager _hotkeyManager = new();
        private readonly DispatcherTimer _cursorTimer = new();

        private readonly SolidColorBrush _greenBrush = new((Color)ColorConverter.ConvertFromString("#10B981"));
        private readonly SolidColorBrush _redBrush = new((Color)ColorConverter.ConvertFromString("#EF4444"));
        private readonly SolidColorBrush _mutedBrush = new((Color)ColorConverter.ConvertFromString("#94A3B8"));

        public MainWindow()
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;

            _engine.StateChanged += OnEngineStateChanged;
            _engine.StatsUpdated += OnEngineStatsUpdated;

            _hotkeyManager.ToggleRequested += OnHotkeyToggle;
            _hotkeyManager.PickLocationRequested += OnHotkeyPickLocation;

            // Timer to show live cursor coordinates in the UI
            _cursorTimer.Interval = TimeSpan.FromMilliseconds(100);
            _cursorTimer.Tick += CursorTimer_Tick;
            _cursorTimer.Start();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _hotkeyManager.Initialize(this);
            UpdateSpeedModeUi();
            UpdateLocationModeUi();
            UpdateRepeatModeUi();
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            _cursorTimer.Stop();
            _hotkeyManager.Dispose();
            _engine.Dispose();
        }

        private void CursorTimer_Tick(object? sender, EventArgs e)
        {
            if (NativeMethods.GetCursorPos(out POINT pt))
            {
                TxtCurrentCursorPos.Text = $"Cursor: X={pt.X}, Y={pt.Y}";
            }
        }

        private void OnHotkeyToggle()
        {
            Dispatcher.Invoke(() => ToggleClicking());
        }

        private void OnHotkeyPickLocation()
        {
            Dispatcher.Invoke(() => PickCurrentLocation());
        }

        private void BtnStartStop_Click(object sender, RoutedEventArgs e)
        {
            ToggleClicking();
        }

        private void ToggleClicking()
        {
            if (_engine.IsRunning)
            {
                _engine.Stop();
            }
            else
            {
                ClickConfig config = BuildConfig();
                _engine.Start(config);
            }
        }

        private void OnEngineStateChanged(bool isRunning)
        {
            Dispatcher.Invoke(() =>
            {
                if (isRunning)
                {
                    BtnStartStop.Background = _redBrush;
                    TxtBtnIcon.Text = "■";
                    TxtBtnLabel.Text = "STOP CLICKING (F6)";
                    TxtStatusBadge.Text = "CLICKING...";
                    TxtStatusBadge.Foreground = _greenBrush;
                }
                else
                {
                    BtnStartStop.Background = _greenBrush;
                    TxtBtnIcon.Text = "▶";
                    TxtBtnLabel.Text = "START CLICKING (F6)";
                    TxtStatusBadge.Text = "IDLE";
                    TxtStatusBadge.Foreground = _mutedBrush;
                    TxtLiveCps.Text = "0 CPS";
                }
            });
        }

        private void OnEngineStatsUpdated(long totalClicks, double currentCps)
        {
            Dispatcher.Invoke(() =>
            {
                TxtTotalClicks.Text = totalClicks.ToString("N0", CultureInfo.InvariantCulture);
                TxtLiveCps.Text = $"{currentCps:N0} CPS";
            });
        }

        private ClickConfig BuildConfig()
        {
            var config = new ClickConfig();

            // Speed mode
            if (RbSpeedTurbo.IsChecked == true)
            {
                config.SpeedMode = ClickSpeedMode.Turbo;
            }
            else if (RbSpeedInterval.IsChecked == true)
            {
                config.SpeedMode = ClickSpeedMode.Interval;
                if (double.TryParse(TbIntervalMs.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out double interval))
                {
                    config.IntervalMs = interval;
                }
            }
            else
            {
                config.SpeedMode = ClickSpeedMode.Cps;
                if (double.TryParse(TbTargetCps.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out double cps))
                {
                    config.TargetCps = cps;
                }
            }

            // Mouse button
            if (RbBtnRight.IsChecked == true)
            {
                config.MouseButton = ClickMouseButton.Right;
            }
            else if (RbBtnMiddle.IsChecked == true)
            {
                config.MouseButton = ClickMouseButton.Middle;
            }
            else
            {
                config.MouseButton = ClickMouseButton.Left;
            }

            // Click type
            config.ClickType = (RbTypeDouble.IsChecked == true) ? ClickType.Double : ClickType.Single;

            // Location
            if (RbLocFixed.IsChecked == true)
            {
                config.LocationMode = LocationMode.FixedLocation;
                int.TryParse(TbFixedX.Text, out int x);
                int.TryParse(TbFixedY.Text, out int y);
                config.FixedX = x;
                config.FixedY = y;
            }
            else
            {
                config.LocationMode = LocationMode.CurrentLocation;
            }

            // Repeat condition
            if (RbRepeatCount.IsChecked == true)
            {
                config.RepeatMode = RepeatMode.CountLimit;
                if (long.TryParse(TbRepeatCount.Text, out long count))
                {
                    config.RepeatCount = count;
                }
            }
            else
            {
                config.RepeatMode = RepeatMode.Infinite;
            }

            return config;
        }

        private void PickCurrentLocation()
        {
            if (NativeMethods.GetCursorPos(out POINT pt))
            {
                TbFixedX.Text = pt.X.ToString();
                TbFixedY.Text = pt.Y.ToString();
                RbLocFixed.IsChecked = true;
                UpdateLocationModeUi();
            }
        }

        private void BtnPickLocation_Click(object sender, RoutedEventArgs e)
        {
            PickCurrentLocation();
        }

        private void BtnResetStats_Click(object sender, RoutedEventArgs e)
        {
            _engine.ResetStats();
        }

        private void ChkAlwaysOnTop_Changed(object sender, RoutedEventArgs e)
        {
            Topmost = ChkAlwaysOnTop.IsChecked == true;
        }

        private void SpeedMode_Changed(object sender, RoutedEventArgs e)
        {
            UpdateSpeedModeUi();
        }

        private void UpdateSpeedModeUi()
        {
            if (TbTargetCps == null || TbIntervalMs == null) return;

            TbTargetCps.IsEnabled = RbSpeedCps.IsChecked == true;
            TbIntervalMs.IsEnabled = RbSpeedInterval.IsChecked == true;
        }

        private void QuickCps_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Content is string text)
            {
                RbSpeedCps.IsChecked = true;
                TbTargetCps.Text = text;
                UpdateSpeedModeUi();
            }
        }

        private void LocationMode_Changed(object sender, RoutedEventArgs e)
        {
            UpdateLocationModeUi();
        }

        private void UpdateLocationModeUi()
        {
            if (TbFixedX == null || TbFixedY == null || BtnPickLocation == null) return;

            bool isFixed = RbLocFixed.IsChecked == true;
            TbFixedX.IsEnabled = isFixed;
            TbFixedY.IsEnabled = isFixed;
            BtnPickLocation.IsEnabled = isFixed;
        }

        private void RepeatMode_Changed(object sender, RoutedEventArgs e)
        {
            UpdateRepeatModeUi();
        }

        private void UpdateRepeatModeUi()
        {
            if (TbRepeatCount == null) return;
            TbRepeatCount.IsEnabled = RbRepeatCount.IsChecked == true;
        }

        private void ConfigInput_Changed(object sender, TextChangedEventArgs e)
        {
            // Optional: validate or update as user types
        }
    }
}