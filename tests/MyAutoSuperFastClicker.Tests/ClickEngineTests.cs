using System.Threading;
using System.Threading.Tasks;
using MyAutoSuperFastClicker.Core;
using Xunit;

namespace MyAutoSuperFastClicker.Tests
{
    public class ClickEngineTests
    {
        [Fact]
        public void StartAndStop_ChangesRunningState()
        {
            using var engine = new ClickEngine();
            Assert.False(engine.IsRunning);

            var config = new ClickConfig
            {
                SpeedMode = ClickSpeedMode.Cps,
                TargetCps = 20,
                RepeatMode = RepeatMode.Infinite
            };

            engine.Start(config);
            Assert.True(engine.IsRunning);

            engine.Stop();
            Assert.False(engine.IsRunning);
        }

        [Fact]
        public void Toggle_AlternatesRunningState()
        {
            using var engine = new ClickEngine();
            var config = new ClickConfig
            {
                SpeedMode = ClickSpeedMode.Cps,
                TargetCps = 20
            };

            Assert.False(engine.IsRunning);

            engine.Toggle(config);
            Assert.True(engine.IsRunning);

            engine.Toggle(config);
            Assert.False(engine.IsRunning);
        }

        [Fact]
        public async Task CountLimit_StopsAutomaticallyWhenLimitReached()
        {
            using var engine = new ClickEngine();
            var config = new ClickConfig
            {
                SpeedMode = ClickSpeedMode.Turbo,
                RepeatMode = RepeatMode.CountLimit,
                RepeatCount = 10
            };

            bool stoppedSignaled = false;
            var tcs = new TaskCompletionSource<bool>();

            engine.StateChanged += (isRunning) =>
            {
                if (!isRunning)
                {
                    stoppedSignaled = true;
                    tcs.TrySetResult(true);
                }
            };

            engine.Start(config);

            // Wait up to 3 seconds for 10 clicks to complete
            var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(3000));
            Assert.Same(tcs.Task, completedTask);
            Assert.False(engine.IsRunning);
            Assert.True(stoppedSignaled);
            Assert.True(engine.TotalClicks >= 10);
        }

        [Fact]
        public void ResetStats_ResetsTotalClicksToZero()
        {
            using var engine = new ClickEngine();
            var config = new ClickConfig
            {
                SpeedMode = ClickSpeedMode.Turbo,
                RepeatMode = RepeatMode.CountLimit,
                RepeatCount = 5
            };

            engine.Start(config);
            Thread.Sleep(50);
            engine.Stop();

            Assert.True(engine.TotalClicks > 0);

            engine.ResetStats();
            Assert.Equal(0, engine.TotalClicks);
        }
    }
}
