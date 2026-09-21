using TetrisApp.Audio;
using Xunit;

namespace TetrisApp.Tests.Audio
{
    public class TetrisSoundServiceTests
    {
        [Fact]
        public void InitialState_IsMutedIsFalse_AndBgmNotPlaying()
        {
            using var service = new TetrisSoundService();

            Assert.False(service.IsMuted);
            Assert.False(service.IsBgmPlaying);
        }

        [Fact]
        public void ToggleMute_TogglesMuteState()
        {
            using var service = new TetrisSoundService();

            service.ToggleMute();
            Assert.True(service.IsMuted);

            service.ToggleMute();
            Assert.False(service.IsMuted);
        }

        [Fact]
        public void StartBgm_SetsIsBgmPlayingTrue()
        {
            using var service = new TetrisSoundService();

            service.StartBgm();
            Assert.True(service.IsBgmPlaying);

            service.PauseBgm();
            Assert.False(service.IsBgmPlaying);
        }

        [Fact]
        public void StopBgm_SetsIsBgmPlayingFalse()
        {
            using var service = new TetrisSoundService();

            service.StartBgm();
            service.StopBgm();

            Assert.False(service.IsBgmPlaying);
        }

        [Fact]
        public void PlayRotate_ExecutesWithoutExceptions()
        {
            using var service = new TetrisSoundService();

            var exception = Record.Exception(() => service.PlayRotate());

            Assert.Null(exception);
        }

        [Fact]
        public void PlayLineClear_ExecutesWithoutExceptions()
        {
            using var service = new TetrisSoundService();

            var ex1 = Record.Exception(() => service.PlayLineClear(1));
            var ex4 = Record.Exception(() => service.PlayLineClear(4));

            Assert.Null(ex1);
            Assert.Null(ex4);
        }

        [Fact]
        public void PlayGameOver_ExecutesWithoutExceptions()
        {
            using var service = new TetrisSoundService();

            service.StartBgm();
            var exception = Record.Exception(() => service.PlayGameOver());

            Assert.Null(exception);
            Assert.False(service.IsBgmPlaying);
        }

        [Fact]
        public void Dispose_CanBeCalledMultipleTimesWithoutExceptions()
        {
            var service = new TetrisSoundService();

            service.Dispose();
            var exception = Record.Exception(() => service.Dispose());

            Assert.Null(exception);
        }
    }
}
