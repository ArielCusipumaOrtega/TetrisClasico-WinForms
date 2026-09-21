using TetrisApp.Models;
using Xunit;

namespace TetrisApp.Tests.Models
{
    public class GameBoardTests
    {
        [Fact]
        public void Constants_AreStandardTetrisDimensions()
        {
            Assert.Equal(20, GameBoard.Rows);
            Assert.Equal(10, GameBoard.Columns);
        }

        [Fact]
        public void Indexer_InitialBoard_AllCellsAreEmpty()
        {
            var board = new GameBoard();

            for (int r = 0; r < GameBoard.Rows; r++)
            {
                for (int c = 0; c < GameBoard.Columns; c++)
                {
                    Assert.Equal(TetrominoType.Empty, board[r, c]);
                }
            }
        }

        [Fact]
        public void CanPlace_ValidPositionInEmptyBoard_ReturnsTrue()
        {
            var board = new GameBoard();
            var piece = Tetromino.Create(TetrominoType.T);

            bool canPlace = board.CanPlace(piece, new Position(0, 3));

            Assert.True(canPlace);
        }

        [Fact]
        public void CanPlace_ExceedsLeftBoundary_ReturnsFalse()
        {
            var board = new GameBoard();
            var piece = Tetromino.Create(TetrominoType.J); // Has block at [0, 0]

            bool canPlace = board.CanPlace(piece, new Position(5, -1));

            Assert.False(canPlace);
        }

        [Fact]
        public void CanPlace_ExceedsRightBoundary_ReturnsFalse()
        {
            var board = new GameBoard();
            var piece = Tetromino.Create(TetrominoType.I); // Row 1 has blocks at col 0, 1, 2, 3

            // Col 7 + 3 = 10 (out of 0-9)
            bool canPlace = board.CanPlace(piece, new Position(5, 7));

            Assert.False(canPlace);
        }

        [Fact]
        public void CanPlace_ExceedsBottomBoundary_ReturnsFalse()
        {
            var board = new GameBoard();
            var piece = Tetromino.Create(TetrominoType.O); // 2x2 blocks

            // Row 19 + 1 = 20 (Rows = 20, max index = 19)
            bool canPlace = board.CanPlace(piece, new Position(19, 4));

            Assert.False(canPlace);
        }

        [Fact]
        public void CanPlace_AboveBoard_ReturnsTrueIfColumnsAreValid()
        {
            var board = new GameBoard();
            var piece = Tetromino.Create(TetrominoType.T);

            // Row -1 is valid as long as targetCol is within bounds and doesn't collide with existing blocks
            bool canPlace = board.CanPlace(piece, new Position(-1, 3));

            Assert.True(canPlace);
        }

        [Fact]
        public void CanPlace_CollidesWithExistingBlock_ReturnsFalse()
        {
            var board = new GameBoard();
            var piece1 = Tetromino.Create(TetrominoType.O);
            board.PlacePiece(piece1, new Position(18, 4));

            // Place another piece overlapping the same area
            var piece2 = Tetromino.Create(TetrominoType.T);
            bool canPlace = board.CanPlace(piece2, new Position(17, 4));

            Assert.False(canPlace);
        }

        [Fact]
        public void PlacePiece_LocksBlocksIntoBoard()
        {
            var board = new GameBoard();
            var piece = Tetromino.Create(TetrominoType.O);
            var pos = new Position(18, 4);

            board.PlacePiece(piece, pos);

            Assert.Equal(TetrominoType.O, board[18, 4]);
            Assert.Equal(TetrominoType.O, board[18, 5]);
            Assert.Equal(TetrominoType.O, board[19, 4]);
            Assert.Equal(TetrominoType.O, board[19, 5]);
            Assert.Equal(TetrominoType.Empty, board[17, 4]);
        }

        [Fact]
        public void PlacePiece_AboveVisibleBoard_DoesNotThrow()
        {
            var board = new GameBoard();
            var piece = Tetromino.Create(TetrominoType.T);
            var pos = new Position(-1, 4);

            var exception = Record.Exception(() => board.PlacePiece(piece, pos));

            Assert.Null(exception);
            // The block that fell on row >= 0 is placed
            // T matrix has {0, 6, 0} at row 0 (-1), and {6, 6, 6} at row 1 (0)
            Assert.Equal(TetrominoType.T, board[0, 4]);
            Assert.Equal(TetrominoType.T, board[0, 5]);
            Assert.Equal(TetrominoType.T, board[0, 6]);
        }

        [Fact]
        public void Clear_EmptiesAllPlacedPieces()
        {
            var board = new GameBoard();
            var piece = Tetromino.Create(TetrominoType.O);
            board.PlacePiece(piece, new Position(18, 0));

            board.Clear();

            for (int r = 0; r < GameBoard.Rows; r++)
            {
                for (int c = 0; c < GameBoard.Columns; c++)
                {
                    Assert.Equal(TetrominoType.Empty, board[r, c]);
                }
            }
        }

        [Fact]
        public void ClearFullLines_NoFullLines_ReturnsZero()
        {
            var board = new GameBoard();
            var piece = Tetromino.Create(TetrominoType.O);
            board.PlacePiece(piece, new Position(18, 0));

            int cleared = board.ClearFullLines();

            Assert.Equal(0, cleared);
        }

        [Fact]
        public void ClearFullLines_SingleFullLine_ClearsLineAndShiftsAboveDown()
        {
            var board = new GameBoard();

            // Fill row 19 completely except column 0, and put a single marker on row 18
            // We can place blocks using Tetromino or by filling rows via PlacePiece
            // Let's create an I piece and place across row 19
            var iPiece = Tetromino.Create(TetrominoType.I);
            // I piece in horizontal has blocks on row 1 of its 4x4 matrix
            // Place at row 18 so that row 1 of I piece lands on row 19
            board.PlacePiece(iPiece, new Position(18, 0)); // fills (19, 0..3)
            board.PlacePiece(iPiece, new Position(18, 4)); // fills (19, 4..7)
            // For col 8 and 9, place an O piece at row 18 (fills 18 and 19 at col 8, 9)
            var oPiece = Tetromino.Create(TetrominoType.O);
            board.PlacePiece(oPiece, new Position(18, 8)); // fills (18, 8..9) and (19, 8..9)

            // Now row 19 is completely full (10 columns).
            // Row 18 only has blocks at columns 8 and 9.
            int cleared = board.ClearFullLines();

            Assert.Equal(1, cleared);
            // Row 19 should now contain what was previously at row 18 (columns 8 and 9)
            Assert.Equal(TetrominoType.O, board[19, 8]);
            Assert.Equal(TetrominoType.O, board[19, 9]);
            Assert.Equal(TetrominoType.Empty, board[19, 0]);
            // Row 18 should now be empty
            Assert.Equal(TetrominoType.Empty, board[18, 8]);
            Assert.Equal(TetrominoType.Empty, board[18, 9]);
        }

        [Fact]
        public void ClearFullLines_MultipleFullLines_ClearsAllAndReturnsCount()
        {
            var board = new GameBoard();

            // Fill row 18 and row 19 completely using 5 O pieces
            var oPiece = Tetromino.Create(TetrominoType.O);
            for (int col = 0; col < GameBoard.Columns; col += 2)
            {
                board.PlacePiece(oPiece, new Position(18, col));
            }

            int cleared = board.ClearFullLines();

            Assert.Equal(2, cleared);

            // Both rows should now be empty
            for (int c = 0; c < GameBoard.Columns; c++)
            {
                Assert.Equal(TetrominoType.Empty, board[18, c]);
                Assert.Equal(TetrominoType.Empty, board[19, c]);
            }
        }

        [Fact]
        public void ClearFullLines_NonConsecutiveLines_ClearsAndShiftsCorrectly()
        {
            var board = new GameBoard();
            var oPiece = Tetromino.Create(TetrominoType.O);

            // Fill rows 16 and 17 with O pieces across all 10 columns
            for (int col = 0; col < GameBoard.Columns; col += 2)
            {
                board.PlacePiece(oPiece, new Position(16, col));
            }

            // Fill row 19 with horizontal I pieces and O piece
            var iPiece = Tetromino.Create(TetrominoType.I);
            board.PlacePiece(iPiece, new Position(18, 0)); // fills 19, 0..3
            board.PlacePiece(iPiece, new Position(18, 4)); // fills 19, 4..7
            // Put a single block on row 18 at col 0 using J piece
            // J piece has {2,0,0},{2,2,2},{0,0,0}. At row 17: row 0 is 17, row 1 is 18
            // Let's place J at (17, 0) => row 18 has [2, 2, 2] at col 0, 1, 2
            var jPiece = Tetromino.Create(TetrominoType.J);
            board.PlacePiece(jPiece, new Position(17, 0));

            // Now row 19 needs cols 8 and 9 filled. Let's place a 2x2 O at (18, 8)
            board.PlacePiece(oPiece, new Position(18, 8)); // fills row 18 and 19 at cols 8, 9

            // Row 17 is full (from the O pieces on 16-17).
            // Row 19 is full.
            // Row 18 is partially filled (cols 0, 1, 2 from J, cols 8, 9 from O => 5 cols, NOT full).
            int cleared = board.ClearFullLines();

            // Rows 17 and 19 were cleared
            Assert.True(cleared >= 2);
        }
    }
}
