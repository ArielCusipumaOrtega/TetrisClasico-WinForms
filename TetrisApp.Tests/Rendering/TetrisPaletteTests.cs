using System.Collections.Generic;
using System.Drawing;
using TetrisApp.Models;
using TetrisApp.Rendering;
using Xunit;

namespace TetrisApp.Tests.Rendering
{
    public class TetrisPaletteTests
    {
        [Theory]
        [InlineData(TetrominoType.I)]
        [InlineData(TetrominoType.J)]
        [InlineData(TetrominoType.L)]
        [InlineData(TetrominoType.O)]
        [InlineData(TetrominoType.S)]
        [InlineData(TetrominoType.T)]
        [InlineData(TetrominoType.Z)]
        public void GetColor_ValidTetrominoType_ReturnsOpaqueDistinctColor(TetrominoType type)
        {
            var color = TetrisPalette.GetColor(type);

            Assert.Equal(255, color.A);
            Assert.NotEqual(TetrisPalette.BoardBackground, color);
            Assert.NotEqual(Color.Transparent, color);
        }

        [Fact]
        public void GetColor_AllSevenPieces_HaveUniqueColors()
        {
            var types = new[]
            {
                TetrominoType.I,
                TetrominoType.J,
                TetrominoType.L,
                TetrominoType.O,
                TetrominoType.S,
                TetrominoType.T,
                TetrominoType.Z
            };

            var colors = new HashSet<Color>();
            foreach (var type in types)
            {
                colors.Add(TetrisPalette.GetColor(type));
            }

            Assert.Equal(7, colors.Count);
        }

        [Theory]
        [InlineData(TetrominoType.Empty)]
        [InlineData((TetrominoType)100)]
        public void GetColor_EmptyOrInvalid_ReturnsBoardBackground(TetrominoType type)
        {
            var color = TetrisPalette.GetColor(type);

            Assert.Equal(TetrisPalette.BoardBackground, color);
        }
    }
}
