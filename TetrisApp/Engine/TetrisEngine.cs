using System;
using TetrisApp.Models;

namespace TetrisApp.Engine
{
    /// <summary>
    /// Motor central del juego Tetris. Gestiona el ciclo de vida, piezas activas,
    /// física de movimientos, colisiones y emisión de eventos.
    /// Desacoplado por completo de la interfaz gráfica y de Windows Forms.
    /// </summary>
    public class TetrisEngine
    {
        private readonly Random _random;

        public TetrisEngine(Random? random = null)
        {
            _random = random ?? new Random();
        }

        public GameBoard Board { get; } = new();
        public GameScore Score { get; } = new();
        public GameState State { get; private set; } = GameState.NotStarted;

        public Tetromino? CurrentPiece { get; private set; }
        public Position CurrentPosition { get; private set; }
        public Tetromino? NextPiece { get; private set; }

        public int DropIntervalMs => Score.CalculateDropIntervalMs();

        // Eventos para notificar cambios de estado a cualquier suscriptor (UI, tests, consolas, etc.)
        public event EventHandler? StateChanged;
        public event EventHandler? BoardChanged;
        public event EventHandler? ScoreChanged;
        public event EventHandler? GameOver;

        /// <summary>
        /// Inicializa una nueva partida de Tetris.
        /// </summary>
        public void StartGame()
        {
            Board.Clear();
            Score.Reset();
            State = GameState.Playing;

            NextPiece = Tetromino.CreateRandom(_random);
            SpawnNextPiece();

            StateChanged?.Invoke(this, EventArgs.Empty);
            ScoreChanged?.Invoke(this, EventArgs.Empty);
            BoardChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Alterna entre el estado de juego y pausa.
        /// </summary>
        public void TogglePause()
        {
            if (State == GameState.Playing)
            {
                State = GameState.Paused;
                StateChanged?.Invoke(this, EventArgs.Empty);
                BoardChanged?.Invoke(this, EventArgs.Empty);
            }
            else if (State == GameState.Paused)
            {
                State = GameState.Playing;
                StateChanged?.Invoke(this, EventArgs.Empty);
                BoardChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Ejecuta un ciclo del temporizador de gravedad.
        /// </summary>
        public void Tick()
        {
            if (State != GameState.Playing) return;
            MoveDown();
        }

        public void MoveLeft()
        {
            if (State != GameState.Playing || CurrentPiece == null) return;

            var target = CurrentPosition.Left();
            if (Board.CanPlace(CurrentPiece, target))
            {
                CurrentPosition = target;
                BoardChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void MoveRight()
        {
            if (State != GameState.Playing || CurrentPiece == null) return;

            var target = CurrentPosition.Right();
            if (Board.CanPlace(CurrentPiece, target))
            {
                CurrentPosition = target;
                BoardChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void MoveDown()
        {
            if (State != GameState.Playing || CurrentPiece == null) return;

            var target = CurrentPosition.Down();
            if (Board.CanPlace(CurrentPiece, target))
            {
                CurrentPosition = target;
                BoardChanged?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                LockCurrentPieceAndContinue();
            }
        }

        public void Rotate()
        {
            if (State != GameState.Playing || CurrentPiece == null) return;

            var rotated = CurrentPiece.RotateClockwise();

            // 1. Rotación regular
            if (Board.CanPlace(rotated, CurrentPosition))
            {
                CurrentPiece = rotated;
                BoardChanged?.Invoke(this, EventArgs.Empty);
            }
            // 2. Wall kick básico a la izquierda si choca con el límite derecho
            else if (Board.CanPlace(rotated, CurrentPosition.Left()))
            {
                CurrentPosition = CurrentPosition.Left();
                CurrentPiece = rotated;
                BoardChanged?.Invoke(this, EventArgs.Empty);
            }
            // 3. Wall kick básico a la derecha si choca con el límite izquierdo
            else if (Board.CanPlace(rotated, CurrentPosition.Right()))
            {
                CurrentPosition = CurrentPosition.Right();
                CurrentPiece = rotated;
                BoardChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void HardDrop()
        {
            if (State != GameState.Playing || CurrentPiece == null) return;

            int steps = 0;
            while (Board.CanPlace(CurrentPiece, CurrentPosition.Down()))
            {
                CurrentPosition = CurrentPosition.Down();
                steps++;
            }

            Score.AddHardDropBonus(steps);
            LockCurrentPieceAndContinue();
            ScoreChanged?.Invoke(this, EventArgs.Empty);
        }

        private void LockCurrentPieceAndContinue()
        {
            if (CurrentPiece == null) return;

            // Fija la pieza en la matriz del tablero
            Board.PlacePiece(CurrentPiece, CurrentPosition);

            // Elimina las filas llenas y suma puntuación
            int linesCleared = Board.ClearFullLines();
            if (linesCleared > 0)
            {
                Score.AddLines(linesCleared);
                ScoreChanged?.Invoke(this, EventArgs.Empty);
            }

            // Aparece la siguiente pieza
            SpawnNextPiece();
        }

        private void SpawnNextPiece()
        {
            CurrentPiece = NextPiece;

            if (CurrentPiece != null)
            {
                int startCol = (GameBoard.Columns / 2) - (CurrentPiece.Dimension / 2);
                CurrentPosition = new Position(0, startCol);

                NextPiece = Tetromino.CreateRandom(_random);

                // Si al nacer colisiona con bloques fijos, es Game Over
                if (!Board.CanPlace(CurrentPiece, CurrentPosition))
                {
                    State = GameState.GameOver;
                    GameOver?.Invoke(this, EventArgs.Empty);
                    StateChanged?.Invoke(this, EventArgs.Empty);
                }
            }

            BoardChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
