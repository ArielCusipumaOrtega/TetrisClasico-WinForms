using System.Drawing;
using TetrisApp.Models;
using TetrisApp.Rendering;
using Xunit;

namespace TetrisApp.Tests.Rendering
{
    public class BoardRendererTests
    {
        [Fact]
        public void CellSize_DefaultValueIs30()
        {
            var renderer = new BoardRenderer();
            Assert.Equal(30, renderer.CellSize);
        }

        [Fact]
        public void CellSize_CanBeUpdated()
        {
            var renderer = new BoardRenderer();
            renderer.CellSize = 25;
            Assert.Equal(25, renderer.CellSize);
        }

        [Fact]
        public void DrawBlock_ExecutesWithoutExceptions()
        {
            var renderer = new BoardRenderer();
            using var bitmap = new Bitmap(100, 100);
            using var graphics = Graphics.FromImage(bitmap);

            var exception = Record.Exception(() =>
            {
                renderer.DrawBlock(graphics, 10, 10, 30, Color.Red);
            });

            Assert.Null(exception);
        }

        [Fact]
        public void DrawBoard_EmptyBoard_ExecutesWithoutExceptions()
        {
            var renderer = new BoardRenderer();
            var board = new GameBoard();
            using var bitmap = new Bitmap(300, 600);
            using var graphics = Graphics.FromImage(bitmap);

            var exception = Record.Exception(() =>
            {
                renderer.DrawBoard(graphics, board, null, new Position(0, 0), bitmap.Size, isPaused: false);
            });

            Assert.Null(exception);
        }

        [Fact]
        public void DrawBoard_WithActivePieceAndPlacedBlocks_ExecutesWithoutExceptions()
        {
            var renderer = new BoardRenderer();
            var board = new GameBoard();
            var piece = Tetromino.Create(TetrominoType.T);
            board.PlacePiece(piece, new Position(18, 3));

            var activePiece = Tetromino.Create(TetrominoType.I);
            using var bitmap = new Bitmap(300, 600);
            using var graphics = Graphics.FromImage(bitmap);

            var exception = Record.Exception(() =>
            {
                renderer.DrawBoard(graphics, board, activePiece, new Position(5, 2), bitmap.Size, isPaused: false);
            });

            Assert.Null(exception);
        }

        [Fact]
        public void DrawBoard_WhenPaused_ExecutesOverlayWithoutExceptions()
        {
            var renderer = new BoardRenderer();
            var board = new GameBoard();
            using var bitmap = new Bitmap(300, 600);
            using var graphics = Graphics.FromImage(bitmap);

            var exception = Record.Exception(() =>
            {
                renderer.DrawBoard(graphics, board, null, new Position(0, 0), bitmap.Size, isPaused: true);
            });

            Assert.Null(exception);
        }

        [Fact]
        public void DrawNextPiecePreview_WithNullPiece_ExecutesWithoutExceptions()
        {
            var renderer = new BoardRenderer();
            using var bitmap = new Bitmap(150, 150);
            using var graphics = Graphics.FromImage(bitmap);

            var exception = Record.Exception(() =>
            {
                renderer.DrawNextPiecePreview(graphics, null, bitmap.Size);
            });

            Assert.Null(exception);
        }

        [Fact]
        public void DrawNextPiecePreview_WithValidPiece_ExecutesWithoutExceptions()
        {
            var renderer = new BoardRenderer();
            var piece = Tetromino.Create(TetrominoType.S);
            using var bitmap = new Bitmap(150, 150);
            using var graphics = Graphics.FromImage(bitmap);

            var exception = Record.Exception(() =>
            {
                renderer.DrawNextPiecePreview(graphics, piece, bitmap.Size);
            });

            Assert.Null(exception);
        }
    }
}
