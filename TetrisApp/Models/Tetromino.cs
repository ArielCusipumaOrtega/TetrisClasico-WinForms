using System;

namespace TetrisApp.Models
{
    /// <summary>
    /// Representa una pieza geométrica de Tetris con su forma, dimensión y lógica de rotación.
    /// </summary>
    public class Tetromino
    {
        public TetrominoType Type { get; }
        public int[,] Matrix { get; }
        public int Dimension => Matrix.GetLength(0);

        public Tetromino(TetrominoType type, int[,] matrix)
        {
            Type = type;
            Matrix = matrix;
        }

        /// <summary>
        /// Rota la matriz 90 grados en el sentido horario.
        /// </summary>
        public Tetromino RotateClockwise()
        {
            int dim = Dimension;
            int[,] rotada = new int[dim, dim];

            for (int r = 0; r < dim; r++)
            {
                for (int c = 0; c < dim; c++)
                {
                    rotada[r, c] = Matrix[dim - 1 - c, r];
                }
            }

            return new Tetromino(Type, rotada);
        }

        /// <summary>
        /// Crea un tetrominó base según su tipo oficial.
        /// </summary>
        public static Tetromino Create(TetrominoType type)
        {
            int[,] matrix = type switch
            {
                TetrominoType.I => new int[,]
                {
                    { 0, 0, 0, 0 },
                    { 1, 1, 1, 1 },
                    { 0, 0, 0, 0 },
                    { 0, 0, 0, 0 }
                },
                TetrominoType.J => new int[,]
                {
                    { 2, 0, 0 },
                    { 2, 2, 2 },
                    { 0, 0, 0 }
                },
                TetrominoType.L => new int[,]
                {
                    { 0, 0, 3 },
                    { 3, 3, 3 },
                    { 0, 0, 0 }
                },
                TetrominoType.O => new int[,]
                {
                    { 4, 4 },
                    { 4, 4 }
                },
                TetrominoType.S => new int[,]
                {
                    { 0, 5, 5 },
                    { 5, 5, 0 },
                    { 0, 0, 0 }
                },
                TetrominoType.T => new int[,]
                {
                    { 0, 6, 0 },
                    { 6, 6, 6 },
                    { 0, 0, 0 }
                },
                TetrominoType.Z => new int[,]
                {
                    { 7, 7, 0 },
                    { 0, 7, 7 },
                    { 0, 0, 0 }
                },
                _ => new int[,] { { 0 } }
            };

            return new Tetromino(type, matrix);
        }

        /// <summary>
        /// Genera un tetrominó aleatorio entre los 7 tipos disponibles.
        /// </summary>
        public static Tetromino CreateRandom(Random random)
        {
            int tipoId = random.Next(1, 8);
            return Create((TetrominoType)tipoId);
        }
    }
}
