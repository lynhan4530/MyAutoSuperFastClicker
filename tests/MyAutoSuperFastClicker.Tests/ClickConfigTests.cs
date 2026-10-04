using System.Diagnostics;
using MyAutoSuperFastClicker.Core;
using Xunit;

namespace MyAutoSuperFastClicker.Tests
{
    public class ClickConfigTests
    {
        [Fact]
        public void DefaultValues_AreSensible()
        {
            var config = new ClickConfig();

            Assert.Equal(ClickSpeedMode.Cps, config.SpeedMode);
            Assert.Equal(100.0, config.TargetCps);
            Assert.Equal(10.0, config.IntervalMs);
            Assert.Equal(ClickMouseButton.Left, config.MouseButton);
            Assert.Equal(ClickType.Single, config.ClickType);
            Assert.Equal(LocationMode.CurrentLocation, config.LocationMode);
            Assert.Equal(RepeatMode.Infinite, config.RepeatMode);
            Assert.Equal(100, config.RepeatCount);
        }

        [Fact]
        public void GetTargetDelayTicks_TurboMode_ReturnsZero()
        {
            var config = new ClickConfig
            {
                SpeedMode = ClickSpeedMode.Turbo
            };

            long ticks = config.GetTargetDelayTicks();
            Assert.Equal(0, ticks);
        }

        [Theory]
        [InlineData(10.0)]
        [InlineData(50.0)]
        [InlineData(100.0)]
        [InlineData(1000.0)]
        public void GetTargetDelayTicks_CpsMode_CalculatesCorrectDelay(double targetCps)
        {
            var config = new ClickConfig
            {
                SpeedMode = ClickSpeedMode.Cps,
                TargetCps = targetCps
            };

            long ticks = config.GetTargetDelayTicks();
            long expectedTicks = (long)((1.0 / targetCps) * Stopwatch.Frequency);

            // Allow +/- 1 tick due to rounding
            Assert.InRange(ticks, expectedTicks - 1, expectedTicks + 1);
        }

        [Theory]
        [InlineData(5.0)]
        [InlineData(20.0)]
        [InlineData(100.0)]
        public void GetTargetDelayTicks_IntervalMode_CalculatesCorrectDelay(double intervalMs)
        {
            var config = new ClickConfig
            {
                SpeedMode = ClickSpeedMode.Interval,
                IntervalMs = intervalMs
            };

            long ticks = config.GetTargetDelayTicks();
            long expectedTicks = (long)((intervalMs / 1000.0) * Stopwatch.Frequency);

            Assert.InRange(ticks, expectedTicks - 1, expectedTicks + 1);
        }

        [Fact]
        public void Validate_CorrectsInvalidValues()
        {
            var config = new ClickConfig
            {
                TargetCps = -50,
                IntervalMs = 0,
                RepeatCount = -10
            };

            config.Validate();

            Assert.True(config.TargetCps >= 1);
            Assert.True(config.IntervalMs >= 1);
            Assert.True(config.RepeatCount >= 1);
        }
    }
}
