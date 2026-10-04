using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using MyAutoSuperFastClicker.Interop;

namespace MyAutoSuperFastClicker.Core
{
    public class ClickEngine : IDisposable
    {
        private static readonly int InputSize = Marshal.SizeOf<INPUT>();

        private Thread? _workerThread;
        private CancellationTokenSource? _cts;
        private readonly object _stateLock = new();

        private long _totalClicks;
        private bool _isRunning;

        public bool IsRunning
        {
            get
            {
                lock (_stateLock)
                {
                    return _isRunning;
                }
            }
            private set
            {
                lock (_stateLock)
                {
                    _isRunning = value;
                }
            }
        }

        public long TotalClicks => Interlocked.Read(ref _totalClicks);

        public event Action<bool>? StateChanged;
        public event Action<long, double>? StatsUpdated; // totalClicks, currentCps

        public void Start(ClickConfig config)
        {
            lock (_stateLock)
            {
                if (_isRunning)
                {
                    return;
                }

                config.Validate();
                _isRunning = true;
                _cts = new CancellationTokenSource();

                _workerThread = new Thread(() => WorkerLoop(config, _cts.Token))
                {
                    Name = "SuperFastClickerWorker",
                    IsBackground = true,
                    Priority = ThreadPriority.Highest
                };

                _workerThread.Start();
            }

            StateChanged?.Invoke(true);
        }

        public void Stop()
        {
            lock (_stateLock)
            {
                if (!_isRunning)
                {
                    return;
                }

                _cts?.Cancel();
                _isRunning = false;
            }

            StateChanged?.Invoke(false);
        }

        public void Toggle(ClickConfig config)
        {
            if (IsRunning)
            {
                Stop();
            }
            else
            {
                Start(config);
            }
        }

        public void ResetStats()
        {
            Interlocked.Exchange(ref _totalClicks, 0);
            StatsUpdated?.Invoke(0, 0.0);
        }

        private void WorkerLoop(ClickConfig config, CancellationToken token)
        {
            // Request 1ms timer precision from the OS
            NativeMethods.TimeBeginPeriod(1);

            try
            {
                // Pre-configure input buffers to avoid any GC allocations during clicking
                uint downFlag = config.MouseButton switch
                {
                    ClickMouseButton.Right => NativeMethods.MOUSEEVENTF_RIGHTDOWN,
                    ClickMouseButton.Middle => NativeMethods.MOUSEEVENTF_MIDDLEDOWN,
                    _ => NativeMethods.MOUSEEVENTF_LEFTDOWN
                };

                uint upFlag = config.MouseButton switch
                {
                    ClickMouseButton.Right => NativeMethods.MOUSEEVENTF_RIGHTUP,
                    ClickMouseButton.Middle => NativeMethods.MOUSEEVENTF_MIDDLEUP,
                    _ => NativeMethods.MOUSEEVENTF_LEFTUP
                };

                INPUT[] inputs;
                if (config.ClickType == ClickType.Double)
                {
                    inputs = new INPUT[4];
                    inputs[0].type = NativeMethods.INPUT_MOUSE;
                    inputs[0].mi.dwFlags = downFlag;
                    inputs[1].type = NativeMethods.INPUT_MOUSE;
                    inputs[1].mi.dwFlags = upFlag;
                    inputs[2].type = NativeMethods.INPUT_MOUSE;
                    inputs[2].mi.dwFlags = downFlag;
                    inputs[3].type = NativeMethods.INPUT_MOUSE;
                    inputs[3].mi.dwFlags = upFlag;
                }
                else
                {
                    inputs = new INPUT[2];
                    inputs[0].type = NativeMethods.INPUT_MOUSE;
                    inputs[0].mi.dwFlags = downFlag;
                    inputs[1].type = NativeMethods.INPUT_MOUSE;
                    inputs[1].mi.dwFlags = upFlag;
                }

                uint inputCount = (uint)inputs.Length;
                int clicksPerEvent = config.ClickType == ClickType.Double ? 2 : 1;

                long targetDelayTicks = config.GetTargetDelayTicks();
                bool isTurbo = config.SpeedMode == ClickSpeedMode.Turbo;
                bool isFixedLocation = config.LocationMode == LocationMode.FixedLocation;
                int fixedX = config.FixedX;
                int fixedY = config.FixedY;

                long targetClickLimit = config.RepeatMode == RepeatMode.CountLimit ? config.RepeatCount : long.MaxValue;
                long localClicksCount = 0;

                long nextClickTime = Stopwatch.GetTimestamp();
                long lastStatsTimestamp = Stopwatch.GetTimestamp();
                long clicksSinceLastStats = 0;

                while (!token.IsCancellationRequested && localClicksCount < targetClickLimit)
                {
                    // If target position is fixed, position the cursor before sending click
                    if (isFixedLocation)
                    {
                        NativeMethods.SetCursorPos(fixedX, fixedY);
                    }

                    // Send the click event(s)
                    NativeMethods.SendInput(inputCount, inputs, InputSize);

                    localClicksCount += clicksPerEvent;
                    clicksSinceLastStats += clicksPerEvent;
                    Interlocked.Add(ref _totalClicks, clicksPerEvent);

                    // High precision timing delay
                    if (!isTurbo && targetDelayTicks > 0)
                    {
                        nextClickTime += targetDelayTicks;
                        long currentTicks = Stopwatch.GetTimestamp();

                        // Avoid drift if we've fallen behind
                        if (currentTicks - nextClickTime > targetDelayTicks * 2)
                        {
                            nextClickTime = currentTicks + targetDelayTicks;
                        }

                        // Precision hybrid wait
                        while (Stopwatch.GetTimestamp() < nextClickTime)
                        {
                            if (token.IsCancellationRequested)
                            {
                                break;
                            }

                            long remainingTicks = nextClickTime - Stopwatch.GetTimestamp();
                            double remainingMs = (double)remainingTicks * 1000.0 / Stopwatch.Frequency;

                            if (remainingMs > 2.0)
                            {
                                Thread.Sleep(1);
                            }
                            else if (remainingMs > 0.05)
                            {
                                Thread.SpinWait(100);
                            }
                        }
                    }

                    // Update live statistics every ~200ms
                    long now = Stopwatch.GetTimestamp();
                    long elapsedStatsTicks = now - lastStatsTimestamp;
                    if (elapsedStatsTicks >= (Stopwatch.Frequency / 5)) // 200ms
                    {
                        double elapsedSeconds = (double)elapsedStatsTicks / Stopwatch.Frequency;
                        double currentCps = clicksSinceLastStats / elapsedSeconds;
                        lastStatsTimestamp = now;
                        clicksSinceLastStats = 0;

                        StatsUpdated?.Invoke(TotalClicks, currentCps);
                    }
                }
            }
            finally
            {
                // Restore Windows timer resolution
                NativeMethods.TimeEndPeriod(1);

                lock (_stateLock)
                {
                    _isRunning = false;
                }

                StateChanged?.Invoke(false);
                StatsUpdated?.Invoke(TotalClicks, 0.0);
            }
        }

        public void Dispose()
        {
            Stop();
            _cts?.Dispose();
        }
    }
}
