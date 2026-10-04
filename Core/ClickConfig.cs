using System;

namespace MyAutoSuperFastClicker.Core
{
    public enum ClickSpeedMode
    {
        Turbo,      // Maximum possible speed (zero intentional delay)
        Cps,        // Target Clicks Per Second
        Interval    // Explicit millisecond delay between clicks
    }

    public enum ClickMouseButton
    {
        Left,
        Right,
        Middle
    }

    public enum ClickType
    {
        Single,
        Double
    }

    public enum LocationMode
    {
        CurrentLocation,
        FixedLocation
    }

    public enum RepeatMode
    {
        Infinite,
        CountLimit
    }

    public class ClickConfig
    {
        public ClickSpeedMode SpeedMode { get; set; } = ClickSpeedMode.Cps;

        public double TargetCps { get; set; } = 100.0;

        public double IntervalMs { get; set; } = 10.0;

        public ClickMouseButton MouseButton { get; set; } = ClickMouseButton.Left;

        public ClickType ClickType { get; set; } = ClickType.Single;

        public LocationMode LocationMode { get; set; } = LocationMode.CurrentLocation;

        public int FixedX { get; set; } = 0;

        public int FixedY { get; set; } = 0;

        public RepeatMode RepeatMode { get; set; } = RepeatMode.Infinite;

        public long RepeatCount { get; set; } = 100;

        /// <summary>
        /// Gets the target delay between clicks in Stopwatch ticks.
        /// Returns 0 for Turbo mode.
        /// </summary>
        public long GetTargetDelayTicks()
        {
            if (SpeedMode == ClickSpeedMode.Turbo)
            {
                return 0;
            }

            double delaySeconds;
            if (SpeedMode == ClickSpeedMode.Cps)
            {
                double cps = Math.Clamp(TargetCps, 0.1, 100000.0);
                delaySeconds = 1.0 / cps;
            }
            else
            {
                double ms = Math.Max(0.1, IntervalMs);
                delaySeconds = ms / 1000.0;
            }

            return (long)(delaySeconds * System.Diagnostics.Stopwatch.Frequency);
        }

        public void Validate()
        {
            if (TargetCps <= 0)
            {
                TargetCps = 1;
            }

            if (IntervalMs <= 0)
            {
                IntervalMs = 1;
            }

            if (RepeatCount <= 0)
            {
                RepeatCount = 1;
            }
        }
    }
}
