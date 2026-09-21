using System;
using TetrisApp.Models;
using Xunit;

namespace TetrisApp.Tests.Models
{
    public class TetrominoTests
    {
        [Theory]
        [InlineData(TetrominoType.I, 4)]
        [InlineData(TetrominoType.J, 3)]
        [InlineData(TetrominoType.L, 3)]
        [InlineData(TetrominoType.O, 2)]
        [InlineData(TetrominoType.S, 3)]
        [InlineData(TetrominoType.T, 3)]
        [InlineData(TetrominoType.Z, 3)]
        public void Create_ValidTypes_HaveExpectedDimension(TetrominoType type, int expectedDimension)
        {
            var piece = Tetromino.Create(type);

            Assert.Equal(type, piece.Type);
            Assert.Equal(expectedDimension, piece.Dimension);
            Assert.Equal(expectedDimension, piece.Matrix.GetLength(0));
            Assert.Equal(expectedDimension, piece.Matrix.GetLength(1));
        }

        [Theory]
        [InlineData(TetrominoType.I)]
        [InlineData(TetrominoType.J)]
        [InlineData(TetrominoType.L)]
        [InlineData(TetrominoType.O)]
        [InlineData(TetrominoType.S)]
        [InlineData(TetrominoType.T)]
        [InlineData(TetrominoType.Z)]
        public void Create_StandardPieces_HaveExactlyFourBlocks(TetrominoType type)
        {
            var piece = Tetromino.Create(type);

            int nonZeroCount = 0;
            int dim = piece.Dimension;
            for (int r = 0; r < dim; r++)
            {
                for (int c = 0; c < dim; c++)
                {
                    if (piece.Matrix[r, c] != 0)
                    {
                        nonZeroCount++;
                        Assert.Equal((int)type, piece.Matrix[r, c]);
                    }
                }
            }

            Assert.Equal(4, nonZeroCount);
        }

        [Fact]
        public void Create_InvalidType_ReturnsSingleEmptyCell()
        {
            var piece = Tetromino.Create((TetrominoType)999);

            Assert.Equal(1, piece.Dimension);
            Assert.Equal(0, piece.Matrix[0, 0]);
        }

        [Theory]
        [InlineData(TetrominoType.I)]
        [InlineData(TetrominoType.J)]
        [InlineData(TetrominoType.L)]
        [InlineData(TetrominoType.O)]
        [InlineData(TetrominoType.S)]
        [InlineData(TetrominoType.T)]
        [InlineData(TetrominoType.Z)]
        public void RotateClockwise_FourTimes_ReturnsToOriginalMatrix(TetrominoType type)
        {
            var original = Tetromino.Create(type);
            var rotated = original;

            for (int i = 0; i < 4; i++)
            {
                rotated = rotated.RotateClockwise();
            }

            Assert.Equal(original.Type, rotated.Type);
            Assert.Equal(original.Dimension, rotated.Dimension);

            for (int r = 0; r < original.Dimension; r++)
            {
                for (int c = 0; c < original.Dimension; c++)
                {
                    Assert.Equal(original.Matrix[r, c], rotated.Matrix[r, c]);
                }
            }
        }

        [Fact]
        public void RotateClockwise_O_Piece_StaysUnchanged()
        {
            var piece = Tetromino.Create(TetrominoType.O);
            var rotated = piece.RotateClockwise();

            for (int r = 0; r < 2; r++)
            {
                for (int c = 0; c < 2; c++)
                {
                    Assert.Equal(4, rotated.Matrix[r, c]);
                }
            }
        }

        [Fact]
        public void RotateClockwise_I_Piece_RotatesCorrectly()
        {
            var original = Tetromino.Create(TetrominoType.I);
            // Original: row 1 has 1s
            Assert.Equal(1, original.Matrix[1, 0]);
            Assert.Equal(1, original.Matrix[1, 1]);
            Assert.Equal(1, original.Matrix[1, 2]);
            Assert.Equal(1, original.Matrix[1, 3]);

            var rotated = original.RotateClockwise();
            // After 1 clockwise rotation: column 2 (index 2) has 1s
            // dim = 4; rotated[r, c] = Matrix[dim - 1 - c, r]
            // Matrix has 1s at [1, 0], [1, 1], [1, 2], [1, 3]
            // For Matrix[1, c], dim-1-c' = 1 => c' = 2, r' = c
            // So rotated has 1s at [0, 2], [1, 2], [2, 2], [3, 2]
            Assert.Equal(1, rotated.Matrix[0, 2]);
            Assert.Equal(1, rotated.Matrix[1, 2]);
            Assert.Equal(1, rotated.Matrix[2, 2]);
            Assert.Equal(1, rotated.Matrix[3, 2]);
        }

        [Fact]
        public void RotateClockwise_PreservesType()
        {
            var piece = Tetromino.Create(TetrominoType.T);
            var rotated = piece.RotateClockwise();

            Assert.Equal(TetrominoType.T, rotated.Type);
        }

        [Fact]
        public void CreateRandom_GeneratesValidTetrominos()
        {
            var random = new Random(12345);
            var generatedTypes = new System.Collections.Generic.HashSet<TetrominoType>();

            for (int i = 0; i < 100; i++)
            {
                var piece = Tetromino.CreateRandom(random);
                Assert.NotEqual(TetrominoType.Empty, piece.Type);
                Assert.InRange((int)piece.Type, 1, 7);
                Assert.NotNull(piece.Matrix);
                generatedTypes.Add(piece.Type);
            }

            // Over 100 iterations with seed 12345, all 7 pieces should be produced
            Assert.Equal(7, generatedTypes.Count);
        }
    }
}
