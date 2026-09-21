using System;
using TetrisApp.Engine;
using TetrisApp.Models;
using Xunit;

namespace TetrisApp.Tests.Engine
{
    public class TetrisEngineTests
    {
        [Fact]
        public void InitialState_IsNotStarted()
        {
            var engine = new TetrisEngine();

            Assert.Equal(GameState.NotStarted, engine.State);
            Assert.Null(engine.CurrentPiece);
            Assert.Null(engine.NextPiece);
            Assert.Equal(0, engine.Score.Score);
            Assert.Equal(650, engine.DropIntervalMs);
        }

        [Fact]
        public void StartGame_TransitionsToPlayingAndSpawnsPieces()
        {
            var engine = new TetrisEngine();
            bool stateChanged = false;
            bool scoreChanged = false;
            bool boardChanged = false;

            engine.StateChanged += (s, e) => stateChanged = true;
            engine.ScoreChanged += (s, e) => scoreChanged = true;
            engine.BoardChanged += (s, e) => boardChanged = true;

            engine.StartGame();

            Assert.Equal(GameState.Playing, engine.State);
            Assert.NotNull(engine.CurrentPiece);
            Assert.NotNull(engine.NextPiece);
            Assert.Equal(0, engine.CurrentPosition.Row);
            Assert.True(stateChanged);
            Assert.True(scoreChanged);
            Assert.True(boardChanged);
        }

        [Fact]
        public void TogglePause_CyclesBetweenPlayingAndPaused()
        {
            var engine = new TetrisEngine();
            engine.StartGame();

            int stateChangedCount = 0;
            engine.StateChanged += (s, e) => stateChangedCount++;

            // Pause
            engine.TogglePause();
            Assert.Equal(GameState.Paused, engine.State);
            Assert.Equal(1, stateChangedCount);

            // Resume
            engine.TogglePause();
            Assert.Equal(GameState.Playing, engine.State);
            Assert.Equal(2, stateChangedCount);
        }

        [Fact]
        public void TogglePause_WhenNotStarted_DoesNothing()
        {
            var engine = new TetrisEngine();
            engine.TogglePause();

            Assert.Equal(GameState.NotStarted, engine.State);
        }

        [Fact]
        public void Movements_WhenPausedOrNotStarted_DoNothing()
        {
            var engine = new TetrisEngine();
            engine.StartGame();
            engine.TogglePause();

            var initialPos = engine.CurrentPosition;
            var initialPiece = engine.CurrentPiece;

            engine.MoveLeft();
            engine.MoveRight();
            engine.MoveDown();
            engine.Rotate();
            engine.HardDrop();
            engine.Tick();

            Assert.Equal(initialPos, engine.CurrentPosition);
            Assert.Equal(initialPiece, engine.CurrentPiece);
        }

        [Fact]
        public void MoveLeft_DecreasesColumn()
        {
            var engine = new TetrisEngine();
            engine.StartGame();

            int initialCol = engine.CurrentPosition.Column;
            bool boardChanged = false;
            engine.BoardChanged += (s, e) => boardChanged = true;

            engine.MoveLeft();

            Assert.Equal(initialCol - 1, engine.CurrentPosition.Column);
            Assert.True(boardChanged);
        }

        [Fact]
        public void MoveLeft_CannotExceedLeftWall()
        {
            var engine = new TetrisEngine();
            engine.StartGame();

            for (int i = 0; i < 20; i++)
            {
                engine.MoveLeft();
            }

            // Must still be a valid position within board bounds
            Assert.True(engine.Board.CanPlace(engine.CurrentPiece!, engine.CurrentPosition));
            int col = engine.CurrentPosition.Column;

            // Another left move should not change column
            engine.MoveLeft();
            Assert.Equal(col, engine.CurrentPosition.Column);
        }

        [Fact]
        public void MoveRight_IncreasesColumn()
        {
            var engine = new TetrisEngine();
            engine.StartGame();

            int initialCol = engine.CurrentPosition.Column;
            bool boardChanged = false;
            engine.BoardChanged += (s, e) => boardChanged = true;

            engine.MoveRight();

            Assert.Equal(initialCol + 1, engine.CurrentPosition.Column);
            Assert.True(boardChanged);
        }

        [Fact]
        public void MoveRight_CannotExceedRightWall()
        {
            var engine = new TetrisEngine();
            engine.StartGame();

            for (int i = 0; i < 20; i++)
            {
                engine.MoveRight();
            }

            Assert.True(engine.Board.CanPlace(engine.CurrentPiece!, engine.CurrentPosition));
            int col = engine.CurrentPosition.Column;

            engine.MoveRight();
            Assert.Equal(col, engine.CurrentPosition.Column);
        }

        [Fact]
        public void MoveDown_IncreasesRow()
        {
            var engine = new TetrisEngine();
            engine.StartGame();

            int initialRow = engine.CurrentPosition.Row;
            engine.MoveDown();

            Assert.Equal(initialRow + 1, engine.CurrentPosition.Row);
        }

        [Fact]
        public void Tick_AdvancesPieceDownwards()
        {
            var engine = new TetrisEngine();
            engine.StartGame();

            int initialRow = engine.CurrentPosition.Row;
            engine.Tick();

            Assert.Equal(initialRow + 1, engine.CurrentPosition.Row);
        }

        [Fact]
        public void MoveDown_WhenReachingBottom_LocksPieceAndSpawnsNext()
        {
            var engine = new TetrisEngine();
            engine.StartGame();

            var firstPiece = engine.CurrentPiece;

            // Move down until the first piece locks and a new piece spawns
            while (ReferenceEquals(engine.CurrentPiece, firstPiece) && engine.State == GameState.Playing)
            {
                engine.MoveDown();
            }

            // Piece has locked and new piece spawned at top
            Assert.NotSame(firstPiece, engine.CurrentPiece);
            Assert.Equal(0, engine.CurrentPosition.Row);
        }

        [Fact]
        public void HardDrop_ImmediatelyLocksPieceAndAwardsPoints()
        {
            var engine = new TetrisEngine();
            engine.StartGame();

            var initialPiece = engine.CurrentPiece;
            bool scoreChanged = false;
            engine.ScoreChanged += (s, e) => scoreChanged = true;

            engine.HardDrop();

            Assert.True(scoreChanged);
            Assert.True(engine.Score.Score > 0);
            // New piece has spawned at row 0
            Assert.Equal(0, engine.CurrentPosition.Row);
        }

        [Fact]
        public void Rotate_Clockwise_ChangesPieceOrientation()
        {
            var engine = new TetrisEngine();
            engine.StartGame();

            // Bring piece down a bit to avoid top edge constraints
            engine.MoveDown();
            engine.MoveDown();

            var before = engine.CurrentPiece;
            engine.Rotate();
            var after = engine.CurrentPiece;

            Assert.NotSame(before, after);
            Assert.Equal(before!.Type, after!.Type);
        }

        [Fact]
        public void Rotate_WallKick_Left_WhenNearRightWall()
        {
            // Seed engine to always produce I piece
            var engine = new TetrisEngine(new ConstantRandom((int)TetrominoType.I));
            engine.StartGame();

            engine.MoveDown();
            engine.MoveDown();

            // Rotate once to make I piece vertical (blocks at matrix column 2)
            engine.Rotate();

            // Move vertical I piece to the right wall so matrix column 2 is at board column 9
            // With position column 7, 7 + 2 = 9 (rightmost valid column)
            while (engine.CurrentPosition.Column < 7)
            {
                engine.MoveRight();
            }

            Assert.Equal(7, engine.CurrentPosition.Column);

            // Rotating back to horizontal cannot fit at col 7 (needs col 7, 8, 9, 10).
            // It kicks left to col 6 (needs col 6, 7, 8, 9).
            engine.Rotate();

            Assert.Equal(6, engine.CurrentPosition.Column);
            Assert.True(engine.Board.CanPlace(engine.CurrentPiece!, engine.CurrentPosition));
        }

        [Fact]
        public void Rotate_WallKick_Right_WhenNearLeftWall()
        {
            // Seed engine to always produce I piece
            var engine = new TetrisEngine(new ConstantRandom((int)TetrominoType.I));
            engine.StartGame();

            engine.MoveDown();
            engine.MoveDown();

            // Rotate once to make I piece vertical (blocks at matrix column 2)
            engine.Rotate();

            // Move vertical I piece to position column -1 (matrix column 2 is at board column 1)
            while (engine.CurrentPosition.Column > -1)
            {
                engine.MoveLeft();
            }

            Assert.Equal(-1, engine.CurrentPosition.Column);

            // Rotating back to horizontal at col -1 puts matrix col 0 at board col -1 (invalid).
            // It kicks right to col 0 (matrix col 0 is at board col 0, fits 0..3).
            engine.Rotate();

            Assert.Equal(0, engine.CurrentPosition.Column);
            Assert.True(engine.Board.CanPlace(engine.CurrentPiece!, engine.CurrentPosition));
        }

        private sealed class ConstantRandom : Random
        {
            private readonly int _value;
            public ConstantRandom(int value) => _value = value;
            public override int Next(int minValue, int maxValue) => _value;
        }

        [Fact]
        public void GameOver_TriggeredWhenSpawnAreaIsBlocked()
        {
            var engine = new TetrisEngine();
            engine.StartGame();

            // Obstruct the center spawn area in rows 0 and 1 without making the row full
            var oPiece = Tetromino.Create(TetrominoType.O);
            engine.Board.PlacePiece(oPiece, new Position(0, 4));

            bool gameOverTriggered = false;
            engine.GameOver += (s, e) => gameOverTriggered = true;

            // Locking the current piece attempts to spawn the next piece in the blocked area
            engine.HardDrop();

            Assert.Equal(GameState.GameOver, engine.State);
            Assert.True(gameOverTriggered);
        }

        [Fact]
        public void LockingPiece_ClearsFullLinesAndAwardsScore()
        {
            var engine = new TetrisEngine();
            engine.StartGame();

            // Prefill row 19 almost completely: columns 0 through 8 filled
            // Leave column 9 empty so an I or O piece or any block dropped can trigger or test lines
            // Let's place O pieces on rows 18 and 19 for cols 0..7
            var oPiece = Tetromino.Create(TetrominoType.O);
            engine.Board.PlacePiece(oPiece, new Position(18, 0));
            engine.Board.PlacePiece(oPiece, new Position(18, 2));
            engine.Board.PlacePiece(oPiece, new Position(18, 4));
            engine.Board.PlacePiece(oPiece, new Position(18, 6));

            // Place O piece at row 18, col 8: row 18 and 19 now have cols 0 through 9 filled!
            engine.Board.PlacePiece(oPiece, new Position(18, 8));

            // Rows 18 and 19 are now completely full.
            // When hard drop locks the current piece, lines will be cleared and score awarded
            int scoreBefore = engine.Score.Score;
            engine.HardDrop();

            Assert.True(engine.Score.Lines >= 2);
            Assert.True(engine.Score.Score > scoreBefore);
        }
    }
}
