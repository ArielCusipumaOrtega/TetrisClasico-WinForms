using TetrisApp.Models;
using Xunit;

namespace TetrisApp.Tests.Models
{
    public class GameScoreTests
    {
        [Fact]
        public void InitialState_IsCorrect()
        {
            var score = new GameScore();

            Assert.Equal(0, score.Score);
            Assert.Equal(0, score.Lines);
            Assert.Equal(1, score.Level);
        }

        [Fact]
        public void Reset_RestoresInitialValues()
        {
            var score = new GameScore();
            score.AddLines(4);
            score.AddHardDropBonus(10);

            score.Reset();

            Assert.Equal(0, score.Score);
            Assert.Equal(0, score.Lines);
            Assert.Equal(1, score.Level);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-5)]
        public void AddLines_ZeroOrNegative_DoesNotChangeStats(int count)
        {
            var score = new GameScore();
            score.AddLines(count);

            Assert.Equal(0, score.Score);
            Assert.Equal(0, score.Lines);
            Assert.Equal(1, score.Level);
        }

        [Theory]
        [InlineData(1, 100)]
        [InlineData(2, 300)]
        [InlineData(3, 500)]
        [InlineData(4, 800)]
        [InlineData(5, 100)] // Default case in switch
        public void AddLines_Level1_CalculatesExpectedScore(int linesCleared, int expectedScore)
        {
            var score = new GameScore();
            score.AddLines(linesCleared);

            Assert.Equal(expectedScore, score.Score);
            Assert.Equal(linesCleared, score.Lines);
        }

        [Fact]
        public void AddLines_LevelIncreasesEvery10Lines()
        {
            var score = new GameScore();

            // Clear 9 lines (3 + 3 + 3)
            score.AddLines(3);
            score.AddLines(3);
            score.AddLines(3);
            Assert.Equal(1, score.Level);
            Assert.Equal(9, score.Lines);

            // 10th line triggers Level 2
            score.AddLines(1);
            Assert.Equal(2, score.Level);
            Assert.Equal(10, score.Lines);

            // 20 lines triggers Level 3
            score.AddLines(4);
            score.AddLines(4);
            score.AddLines(2);
            Assert.Equal(3, score.Level);
            Assert.Equal(20, score.Lines);
        }

        [Fact]
        public void AddLines_HigherLevel_MultipliesScore()
        {
            var score = new GameScore();

            // Bring to level 2 with 10 lines
            score.AddLines(4);
            score.AddLines(4);
            score.AddLines(2);
            Assert.Equal(2, score.Level);

            int scoreBefore = score.Score;
            // Clear 4 lines at level 2: 800 * 2 = 1600 points
            score.AddLines(4);

            Assert.Equal(scoreBefore + 1600, score.Score);
        }

        [Theory]
        [InlineData(5, 10)]
        [InlineData(15, 30)]
        [InlineData(1, 2)]
        public void AddHardDropBonus_PositiveSteps_AddsDoublePoints(int steps, int expectedBonus)
        {
            var score = new GameScore();
            score.AddHardDropBonus(steps);

            Assert.Equal(expectedBonus, score.Score);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void AddHardDropBonus_ZeroOrNegative_DoesNotChangeScore(int steps)
        {
            var score = new GameScore();
            score.AddHardDropBonus(steps);

            Assert.Equal(0, score.Score);
        }

        [Fact]
        public void CalculateDropIntervalMs_Level1_Returns650ms()
        {
            var score = new GameScore();
            Assert.Equal(650, score.CalculateDropIntervalMs());
        }

        [Fact]
        public void CalculateDropIntervalMs_DecreasesWithLevel()
        {
            var score = new GameScore();

            // Reach level 2
            for (int i = 0; i < 10; i++) score.AddLines(1);
            Assert.Equal(2, score.Level);
            // 650 - (1 * 55) = 595
            Assert.Equal(595, score.CalculateDropIntervalMs());

            // Reach level 3
            for (int i = 0; i < 10; i++) score.AddLines(1);
            Assert.Equal(3, score.Level);
            // 650 - (2 * 55) = 540
            Assert.Equal(540, score.CalculateDropIntervalMs());
        }

        [Fact]
        public void CalculateDropIntervalMs_ClampedAt100msMinimum()
        {
            var score = new GameScore();

            // Reach level 15 (140+ lines)
            for (int i = 0; i < 35; i++) score.AddLines(4);
            Assert.True(score.Level >= 15);

            Assert.Equal(100, score.CalculateDropIntervalMs());
        }
    }
}
