using System;

namespace TetrisApp.Models
{
    /// <summary>
    /// Encapsula la matriz del tablero (20 filas x 10 columnas), detección de colisiones y borrado de líneas.
    /// </summary>
    public class GameBoard
    {
        public const int Rows = 20;
        public const int Columns = 10;

        private readonly TetrominoType[,] _grid = new TetrominoType[Rows, Columns];

        /// <summary>
        /// Obtiene el tipo de bloque alojado en una celda dada.
        /// </summary>
        public TetrominoType this[int row, int col] => _grid[row, col];

        /// <summary>
        /// Limpia todas las celdas del tablero dejándolo vacío.
        /// </summary>
        public void Clear()
        {
            Array.Clear(_grid, 0, _grid.Length);
        }

        /// <summary>
        /// Comprueba si una pieza puede ubicarse en la posición indicada sin colisionar
        /// con los límites laterales, el suelo o bloques previamente fijados.
        /// </summary>
        public bool CanPlace(Tetromino piece, Position pos)
        {
            int dim = piece.Dimension;

            for (int r = 0; r < dim; r++)
            {
                for (int c = 0; c < dim; c++)
                {
                    if (piece.Matrix[r, c] != 0)
                    {
                        int targetRow = pos.Row + r;
                        int targetCol = pos.Column + c;

                        // 1. Límites laterales
                        if (targetCol < 0 || targetCol >= Columns) return false;

                        // 2. Límite inferior (suelo)
                        if (targetRow >= Rows) return false;

                        // 3. Colisión con bloques fijos (sólo si está dentro de la matriz visible)
                        if (targetRow >= 0 && _grid[targetRow, targetCol] != TetrominoType.Empty) return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Fija los bloques de la pieza en la matriz del tablero.
        /// </summary>
        public void PlacePiece(Tetromino piece, Position pos)
        {
            int dim = piece.Dimension;

            for (int r = 0; r < dim; r++)
            {
                for (int c = 0; c < dim; c++)
                {
                    if (piece.Matrix[r, c] != 0)
                    {
                        int targetRow = pos.Row + r;
                        int targetCol = pos.Column + c;

                        if (targetRow >= 0 && targetRow < Rows && targetCol >= 0 && targetCol < Columns)
                        {
                            _grid[targetRow, targetCol] = piece.Type;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Recorre el tablero de abajo hacia arriba, elimina las filas llenas y desplaza las superiores hacia abajo.
        /// </summary>
        /// <returns>Cantidad de filas eliminadas.</returns>
        public int ClearFullLines()
        {
            int linesCleared = 0;

            for (int r = Rows - 1; r >= 0; r--)
            {
                if (IsRowFull(r))
                {
                    DeleteRow(r);
                    linesCleared++;
                    r++; // Reevaluar la misma fila tras descender los bloques superiores
                }
            }

            return linesCleared;
        }

        private bool IsRowFull(int row)
        {
            for (int c = 0; c < Columns; c++)
            {
                if (_grid[row, c] == TetrominoType.Empty)
                {
                    return false;
                }
            }
            return true;
        }

        private void DeleteRow(int rowToDelete)
        {
            for (int r = rowToDelete; r > 0; r--)
            {
                for (int c = 0; c < Columns; c++)
                {
                    _grid[r, c] = _grid[r - 1, c];
                }
            }

            // La fila superior queda vacía
            for (int c = 0; c < Columns; c++)
            {
                _grid[0, c] = TetrominoType.Empty;
            }
        }
    }
}
